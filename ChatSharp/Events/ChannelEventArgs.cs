using System;

namespace ChatSharp.Events
{
    public class ChannelEventArgs : EventArgs
    {
        public IrcChannel Channel { get; set; }

        public ChannelEventArgs(IrcChannel channel)
        {
            Channel = channel;
        }
    }
}
