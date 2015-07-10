using System;

namespace ChatSharp.Events
{
    public class WhoIsReceivedEventArgs : EventArgs
    {
        public WhoIs WhoIsResponse
        {
            get;
            set;
        }

        public WhoIsReceivedEventArgs(WhoIs whoIsResponse)
        {
            WhoIsResponse = whoIsResponse;
        }
    }
}
