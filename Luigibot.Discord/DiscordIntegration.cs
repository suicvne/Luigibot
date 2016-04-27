using DiscordSharp;
using LuigibotCommon.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luigibot.Discord
{
    public class DiscordMessageReceivedEventArgs : IMessageReceivedEventArgs
    {
        public override IChannel Channel { get; set; }

        public override IMember Member { get; set; }

        public override string Text { get; set; }
    }

    public class DiscordConnectionClosedEventArgs : IConnectionClosedEventArgs
    { }

    public class DiscordIntegration : IIntegration
    {
        private DiscordClient client;
        public DiscordClient GetRawClient() => client;

        public ConsoleColor LogColor
        {
            get
            {
                return ConsoleColor.DarkMagenta;
            }
            set { }
        }

        public string BoldText(string text)
        {
            return $"**{text}**";
        }

        public string ItalicizeText(string text)
        {
            return $"*{text}*";
        }

        public DiscordIntegration(string token)
        {
            client = new DiscordClient(token, true, true);
        }

        public string IntegrationDescription
        {
            get
            {
                return "";
            }
            set
            {}
        }

        public string IntegrationName
        {
            get
            {
                return "Discord";
            }
            set
            {}
        }

        public bool IsIntegrationRunning
        {
            get
            {
                return client.WebsocketAlive;
            }
            set
            {}
        }

        public event EventHandler<EventArgs> Connected;
        public event EventHandler<IMessageReceivedEventArgs> MessageReceived;
        public event EventHandler<IMessageReceivedEventArgs> MentionReceived;
        public event EventHandler<IErrorReceivedEventArgs> ErrorReceived;
        public event EventHandler<IConnectionClosedEventArgs> ConnectionClosed;

        public void SendMessage(string text, IChannel target)
        {
            if(target is LuigibotDiscordChannel)
            {
                client.SendMessageToChannel(text, (target as LuigibotDiscordChannel).GetRawChannel());
            }
        }

        public void StartIntegration()
        {
            Task.Run(() =>
            {
                client.Connected += (sender, e) =>
                {
                    Connected?.Invoke(this, null);
                };
                client.SocketClosed += (sender, e) =>
                {
                    IConnectionClosedEventArgs args = new DiscordConnectionClosedEventArgs
                    {
                        Code = e.Code,
                        Reason = e.Reason,
                        Clean = e.WasClean
                    };
                    ConnectionClosed?.Invoke(this, args);
                };
                client.MessageReceived += (sender, e) =>
                {
                    DiscordMessageReceivedEventArgs args = new DiscordMessageReceivedEventArgs
                    {
                        Channel = new LuigibotDiscordChannel(e.Channel),
                        Member = new LuigibotDiscordMember(e.Author),
                        Text = e.MessageText
                    };
                    MessageReceived?.Invoke(this, args);
                };
                client.MentionReceived += (sender, e) =>
                {
                    DiscordMessageReceivedEventArgs args = new DiscordMessageReceivedEventArgs
                    {
                        Channel = new LuigibotDiscordChannel(e.Channel),
                        Member = new LuigibotDiscordMember(e.Author),
                        Text = e.MessageText
                    };
                    MentionReceived?.Invoke(this, args);
                };
                client.SendLoginRequest();
                client.Connect(true /*use the .Net ClientWebSocket vs. WebSocketSharp*/);
            });
        }

        public void StopIntegration()
        {
            client.Logout();
            client.Dispose();
        }
    }
}

