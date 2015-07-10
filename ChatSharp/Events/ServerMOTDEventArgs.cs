using System;

namespace ChatSharp.Events
{
    public class ServerMOTDEventArgs : EventArgs
    {
        public string MOTD { get; set; }

        public ServerMOTDEventArgs(string motd)
        {
            MOTD = motd;
        }
    }
}
