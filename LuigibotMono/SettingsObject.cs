using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot2
{
    public class SettingsObject
    {
        public bool EightballEnabled { get; set; }
        public bool SlapEnabled { get; set; }
        public bool WelcomeUserEnabled { get; set; }
		public bool UrlParsingEnabled {get;set;}
        public string LastUsedNick { get; set; }
        public string LastJoinedServer { get; set; }
        public string LastJoinedChannel { get; set; }
		public string WelcomeMessage { get; set; }
        //This string is AES Encrypted
        public string NickServPass { get; set; }
        public string[] UsersAllowedToDisable { get; set; }
        public char CommandPrefix { get; set; }

        public SettingsObject()
        {
            EightballEnabled = true;
            SlapEnabled = true;
            WelcomeUserEnabled = true;
            UsersAllowedToDisable = new string[] { "luigifan2010", "ghosthawk", "aeromatter", "joey" };
            LastUsedNick = "Luigibot";
            LastJoinedServer = "irc.stardustfields.net";
            NickServPass = "";
            CommandPrefix = '!';
            LastJoinedChannel = "#smbx";
			WelcomeMessage = "/me welcomes {0}";
        }
    }
}
