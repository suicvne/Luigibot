using System;

namespace ChatSharp.Events
{
    public class NickChangedEventArgs : EventArgs
    {
        public IrcUser User { get; set; }
        public string OldNick { get; set; }
        public string NewNick { get; set; }
    }
}