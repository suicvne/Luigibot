using System;
using LuigibotCommon.Integrations;

namespace LuigibotSlack
{
	public class SlackChannel : IChannel
	{
        private string _Name;
		public string Name 
		{ 
			get {
                return _Name;
			}
			set{ _Name = value; }
		}

        private string _ID;
		public string ID 
		{ 
			get {
                return _ID;
			}
			set{ _ID = value; }
		}

		private SlackAPI.Channel RawChannel;

		public SlackChannel(){}

		public SlackChannel (SlackAPI.Channel channel)
		{
			RawChannel = channel;
            Name = channel.name;
            ID = channel.id;
		}

        public SlackChannel(string name, string id)
        {
            RawChannel = null;
            Name = name;
            ID = id;
        }
	}
}

