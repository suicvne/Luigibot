using ChatSharp.Events;
using ChatSharp.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace ChatSharp
{
    public partial class IrcClient
    {
        public delegate void MessageHandler(IrcClient client, IrcMessage message);
        private Dictionary<string, MessageHandler> Handlers { get; set; }
        public void SetHandler(string message, MessageHandler handler)
        {
#if DEBUG
            // This is the default behavior if 3rd parties want to handle certain messages themselves
            // However, if it happens from our own code, we probably did something wrong
            if (Handlers.ContainsKey(message.ToUpper()))
                Console.WriteLine("Warning: {0} handler has been overwritten", message);
#endif
            message = message.ToUpper();
            Handlers[message] = handler;
        }

        internal static DateTime DateTimeFromIrcTime(int time)
        {
            return new DateTime(1970, 1, 1).AddSeconds(time);
        }

        private const int ReadBufferLength = 1024;

        private byte[] ReadBuffer { get; set; }
        private int ReadBufferIndex { get; set; }
        private string ServerHostname { get; set; }
        private int ServerPort { get; set; }
        private Timer PingTimer { get; set; }
        private Socket Socket { get; set; }
        private ConcurrentQueue<string> WriteQueue { get; set; }
        private bool IsWriting { get; set; }

        internal string ServerNameFromPing { get; set; }

        public string ServerAddress
        {
            get
            {
                return ServerHostname + ":" + ServerPort;
            }
            internal set
            {
                string[] parts = value.Split(':');
                if (parts.Length > 2 || parts.Length == 0)
                    throw new FormatException("Server address is not in correct format ('hostname:port')");
                ServerHostname = parts[0];
                if (parts.Length > 1)
                    ServerPort = int.Parse(parts[1]);
                else
                    ServerPort = 6667;
            }
        }

        public Stream NetworkStream { get; set; }
        public bool UseSSL { get; private set; }
        public bool IgnoreInvalidSSL { get; set; }
        public Encoding Encoding { get; set; }
        public IrcUser User { get; set; }
        public ChannelCollection Channels { get; private set; }
        public ClientSettings Settings { get; set; }
        public RequestManager RequestManager { get; set; }
        public ServerInfo ServerInfo { get; set; }
        public string PrivmsgPrefix { get; set; }

        public IrcClient(string serverAddress, IrcUser user, bool useSSL = false)
        {
            if (serverAddress == null) throw new ArgumentNullException("serverAddress");
            if (user == null) throw new ArgumentNullException("user");

            User = user;
            ServerAddress = serverAddress;
            Encoding = Encoding.UTF8;
            Channels = new ChannelCollection(this);
            Settings = new ClientSettings();
            Handlers = new Dictionary<string, MessageHandler>();
            MessageHandlers.RegisterDefaultHandlers(this);
            RequestManager = new RequestManager();
            UseSSL = useSSL;
            WriteQueue = new ConcurrentQueue<string>();
            PrivmsgPrefix = "";
        }

        public void ConnectAsync()
        {
            if (Socket != null && Socket.Connected) throw new InvalidOperationException("Socket is already connected to server.");
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReadBuffer = new byte[ReadBufferLength];
            ReadBufferIndex = 0;
            PingTimer = new Timer(30000);
            PingTimer.Elapsed += (sender, e) => 
            {
                if (!string.IsNullOrEmpty(ServerNameFromPing))
                    SendRawMessage("PING :{0}", ServerNameFromPing);
            };
            var checkQueue = new Timer(1000);
            checkQueue.Elapsed += (sender, e) =>
            {
                string nextMessage;
                if (WriteQueue.Count > 0)
                {
                    while (!WriteQueue.TryDequeue(out nextMessage));
                    SendRawMessage(nextMessage);
                }
            };
            checkQueue.Start();
            Socket.BeginConnect(ServerHostname, ServerPort, ConnectComplete, null);
        }

        public void Quit()
        {
            Quit(null);
        }

        public void Quit(string reason)
        {
            if (reason == null)
                SendRawMessage("QUIT");
            else
                SendRawMessage("QUIT :{0}", reason);
            Socket.BeginDisconnect(false, ar =>
            {
                Socket.EndDisconnect(ar);
                NetworkStream.Dispose();
                NetworkStream = null;
            }, null);
            PingTimer.Dispose();
        }

        private void ConnectComplete(IAsyncResult result)
        {
            Socket.EndConnect(result);

            NetworkStream = new NetworkStream(Socket);
            if (UseSSL)
            {
                if (IgnoreInvalidSSL)
                    NetworkStream = new SslStream(NetworkStream, false, (sender, certificate, chain, policyErrors) => true);
                else
                    NetworkStream = new SslStream(NetworkStream);
                ((SslStream)NetworkStream).AuthenticateAsClient(ServerHostname);
            }

            NetworkStream.BeginRead(ReadBuffer, ReadBufferIndex, ReadBuffer.Length, DataRecieved, null);
            // Write login info
            if (!string.IsNullOrEmpty(User.Password))
                SendRawMessage("PASS {0}", User.Password);
            SendRawMessage("NICK {0}", User.Nick);
            // hostname, servername are ignored by most IRC servers
            SendRawMessage("USER {0} hostname servername :{1}", User.User, User.RealName);
            PingTimer.Start();
        }

        private void DataRecieved(IAsyncResult result)
        {
            if (NetworkStream == null)
            {
                OnNetworkError(new SocketErrorEventArgs(SocketError.NotConnected));
                return;
            }

            int length;
            try
            {
                length = NetworkStream.EndRead(result) + ReadBufferIndex;
            }
            catch (IOException e)
            {
                var socketException = e.InnerException as SocketException;
                if (socketException != null)
                    OnNetworkError(new SocketErrorEventArgs(socketException.SocketErrorCode));
                else
                    throw;
                return;
            }

            ReadBufferIndex = 0;
            while (length > 0)
            {
                int messageLength = Array.IndexOf(ReadBuffer, (byte)'\n', 0, length);
                if (messageLength == -1) // Incomplete message
                {
                    ReadBufferIndex = length;
                    break;
                }
                messageLength++;
                var message = Encoding.GetString(ReadBuffer, 0, messageLength - 2); // -2 to remove \r\n
                HandleMessage(message);
                Array.Copy(ReadBuffer, messageLength, ReadBuffer, 0, length - messageLength);
                length -= messageLength;
            }
            NetworkStream.BeginRead(ReadBuffer, ReadBufferIndex, ReadBuffer.Length - ReadBufferIndex, DataRecieved, null);
        }

        private void HandleMessage(string rawMessage)
        {
            OnRawMessageRecieved(new RawMessageEventArgs(rawMessage, false));
            var message = new IrcMessage(rawMessage);
            if (Handlers.ContainsKey(message.Command.ToUpper()))
                Handlers[message.Command.ToUpper()](this, message);
            else
            {
                // TODO: Fire an event or something
            }
        }

        public void SendRawMessage(string message, params object[] format)
        {
            if (NetworkStream == null)
            {
                OnNetworkError(new SocketErrorEventArgs(SocketError.NotConnected));
                return;
            }

            message = string.Format(message, format);
            var data = Encoding.GetBytes(message + "\r\n");

            if (!IsWriting)
            {
                IsWriting = true;
                NetworkStream.BeginWrite(data, 0, data.Length, MessageSent, message);
            }
            else
            {
                WriteQueue.Enqueue(message);
            }
        }

        public void SendIrcMessage(IrcMessage message)
        {
            SendRawMessage(message.RawMessage);
        }

        private void MessageSent(IAsyncResult result)
        {
            if (NetworkStream == null)
            {
                OnNetworkError(new SocketErrorEventArgs(SocketError.NotConnected));
                IsWriting = false;
                return;
            }

            try
            {
                NetworkStream.EndWrite(result);
            }
            catch (IOException e)
            {
                var socketException = e.InnerException as SocketException;
                if (socketException != null)
                    OnNetworkError(new SocketErrorEventArgs(socketException.SocketErrorCode));
                else
                    throw;
                return;
            }
            finally
            {
                IsWriting = false;
            }

            OnRawMessageSent(new RawMessageEventArgs((string)result.AsyncState, true));

            string nextMessage;
            if (WriteQueue.Count > 0)
            {
                while (!WriteQueue.TryDequeue(out nextMessage));
                SendRawMessage(nextMessage);
            }
        }

        public event EventHandler<SocketErrorEventArgs> NetworkError;
        protected internal virtual void OnNetworkError(SocketErrorEventArgs e)
        {
            if (NetworkError != null) NetworkError(this, e);
        }
        public event EventHandler<RawMessageEventArgs> RawMessageSent;
        protected internal virtual void OnRawMessageSent(RawMessageEventArgs e)
        {
            if (RawMessageSent != null) RawMessageSent(this, e);
        }
        public event EventHandler<RawMessageEventArgs> RawMessageRecieved;
        protected internal virtual void OnRawMessageRecieved(RawMessageEventArgs e)
        {
            if (RawMessageRecieved != null) RawMessageRecieved(this, e);
        }
        public event EventHandler<IrcNoticeEventArgs> NoticeRecieved;
        protected internal virtual void OnNoticeRecieved(IrcNoticeEventArgs e)
        {
            if (NoticeRecieved != null) NoticeRecieved(this, e);
        }
        public event EventHandler<ServerMOTDEventArgs> MOTDPartRecieved;
        protected internal virtual void OnMOTDPartRecieved(ServerMOTDEventArgs e)
        {
            if (MOTDPartRecieved != null) MOTDPartRecieved(this, e);
        }
        public event EventHandler<ServerMOTDEventArgs> MOTDRecieved;
        protected internal virtual void OnMOTDRecieved(ServerMOTDEventArgs e)
        {
            if (MOTDRecieved != null) MOTDRecieved(this, e);
        }
        public event EventHandler<PrivateMessageEventArgs> PrivateMessageRecieved;
        protected internal virtual void OnPrivateMessageRecieved(PrivateMessageEventArgs e)
        {
            if (PrivateMessageRecieved != null) PrivateMessageRecieved(this, e);
        }
        public event EventHandler<PrivateMessageEventArgs> ChannelMessageRecieved;
        protected internal virtual void OnChannelMessageRecieved(PrivateMessageEventArgs e)
        {
            if (ChannelMessageRecieved != null) ChannelMessageRecieved(this, e);
        }
        public event EventHandler<PrivateMessageEventArgs> UserMessageRecieved;
        protected internal virtual void OnUserMessageRecieved(PrivateMessageEventArgs e)
        {
            if (UserMessageRecieved != null) UserMessageRecieved(this, e);
        }
        public event EventHandler<ErronousNickEventArgs> NickInUse;
        protected internal virtual void OnNickInUse(ErronousNickEventArgs e)
        {
            if (NickInUse != null) NickInUse(this, e);
        }
        public event EventHandler<ModeChangeEventArgs> ModeChanged;
        protected internal virtual void OnModeChanged(ModeChangeEventArgs e)
        {
            if (ModeChanged != null) ModeChanged(this, e);
        }
        public event EventHandler<ChannelUserEventArgs> UserJoinedChannel;
        protected internal virtual void OnUserJoinedChannel(ChannelUserEventArgs e)
        {
            if (UserJoinedChannel != null) UserJoinedChannel(this, e);
        }
        public event EventHandler<ChannelUserEventArgs> UserPartedChannel;
        protected internal virtual void OnUserPartedChannel(ChannelUserEventArgs e)
        {
            if (UserPartedChannel != null) UserPartedChannel(this, e);
        }
        public event EventHandler<ChannelUserEventArgs> UserQuitChannel;
        protected internal virtual void OnUserQuitChannel(ChannelUserEventArgs e)
        {
            if (UserQuitChannel != null) UserQuitChannel(this, e);
        }
        public event EventHandler<ChannelEventArgs> ChannelListRecieved;
        protected internal virtual void OnChannelListRecieved(ChannelEventArgs e)
        {
            if (ChannelListRecieved != null) ChannelListRecieved(this, e);
        }
        public event EventHandler<ChannelTopicEventArgs> ChannelTopicReceived;
        protected internal virtual void OnChannelTopicReceived(ChannelTopicEventArgs e)
        {
            if (ChannelTopicReceived != null) ChannelTopicReceived(this, e);
        }
        public event EventHandler<EventArgs> ConnectionComplete;
        protected internal virtual void OnConnectionComplete(EventArgs e)
        {
            if (ConnectionComplete != null) ConnectionComplete(this, e);
        }
        public event EventHandler<SupportsEventArgs> ServerInfoRecieved;
        protected internal virtual void OnServerInfoRecieved(SupportsEventArgs e)
        {
            if (ServerInfoRecieved != null) ServerInfoRecieved(this, e);
        }
        public event EventHandler<KickEventArgs> UserKicked;
        protected internal virtual void OnUserKicked(KickEventArgs e)
        {
            if (UserKicked != null) UserKicked(this, e);
        }

        public event EventHandler<WhoIsReceivedEventArgs> WhoIsReceived;
        protected internal virtual void OnWhoIsReceived(WhoIsReceivedEventArgs e)
        {
            if (WhoIsReceived != null) WhoIsReceived(this, e);
        }

        public event EventHandler<NickChangedEventArgs> NickChanged;
        protected internal virtual void OnNickChanged(NickChangedEventArgs e)
        {
            if (NickChanged != null) NickChanged(this, e);
        }
    }
}
