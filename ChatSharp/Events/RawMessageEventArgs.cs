using System;

namespace ChatSharp.Events
{
    public class RawMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool Outgoing { get; set; }

        public RawMessageEventArgs(string message, bool outgoing)
        {
            Message = message;
            Outgoing = outgoing;
        }
    }
}
