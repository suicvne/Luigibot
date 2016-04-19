using DiscordSharp.Objects;
using LuigibotCommon.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot.Discord
{
    public class LuigibotDiscordChannel : IChannel
    {
        private DiscordChannel RawChannel;
        public DiscordChannel GetRawChannel() => RawChannel;

        public LuigibotDiscordChannel(DiscordChannel channel)
        {
            RawChannel = channel;
        }

        public string ID
        {
            get
            {
                return RawChannel.ID;
            }
            set
            {}
        }

        public string Name
        {
            get
            {
                return RawChannel.Name;
            }
            set
            {}
        }
    }
}
