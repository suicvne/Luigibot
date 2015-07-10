using System;

namespace ChatSharp.Events
{
    public class PrivateMessageEventArgs : EventArgs
    {
        public IrcMessage IrcMessage { get; set; }
        public PrivateMessage PrivateMessage { get; set; }

        public PrivateMessageEventArgs(IrcMessage ircMessage, ServerInfo serverInfo)
        {
            IrcMessage = ircMessage;
            PrivateMessage = new PrivateMessage(IrcMessage, serverInfo);
        }
    }
}
