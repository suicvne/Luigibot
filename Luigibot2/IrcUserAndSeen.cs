using ChatSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luigibot2
{
    public class IrcUserAndSeen
    {
        public IrcUser User {get; set;}
        public DateTime LastSeen { get; set; }


        public IrcUserAndSeen(IrcUser user) 
        {
            this.User = user;
        }

        public IrcUserAndSeen(IrcUser user, DateTime lastSeen)
        {
            this.User = user;
            LastSeen = lastSeen;
        }

        public IrcUserAndSeen()
        {

        }
    }
}
