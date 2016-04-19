using DiscordSharp.Objects;
using LuigibotCommon.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot.Discord
{
    public class LuigibotDiscordMember : IMember
    {
        private DiscordMember RawMember;

        public string ID
        {
            get
            {
                return RawMember.ID;
            }
            set{}
        }

        public string Name
        {
            get
            {
                return RawMember.Username;
            }
            set{}
        }

        public DiscordMember GetRawMember() => RawMember;

        public string Mention()
        {
            return $"<@{RawMember.ID}>";
        }

        public LuigibotDiscordMember(DiscordMember member)
        {
            RawMember = member;
        }
    }
}
