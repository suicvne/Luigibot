using System.Linq;

namespace ChatSharp
{
    public class PrivateMessage
    {
        public PrivateMessage(IrcMessage message, ServerInfo serverInfo)
        {
            Source = message.Parameters[0];
            Message = message.Parameters[1];

            User = new IrcUser(message.Prefix);
            if (serverInfo.ChannelTypes.Any(c => Source.StartsWith(c.ToString())))
                IsChannelMessage = true;
            else
                Source = User.Nick;
        }

        public IrcUser User { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public bool IsChannelMessage { get; set; }
    }
}
