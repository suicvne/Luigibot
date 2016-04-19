using System;
using LuigibotCommon.Integrations;

namespace LuigibotSlack
{
	public class SlackChannel : IChannel
	{
		public string Name 
		{ 
			get {
				return (RawChannel != null ? RawChannel.name : "");
			}
			set{ }
		}

		public string ID 
		{ 
			get {
				return (RawChannel != null ? RawChannel.id : "");
			}
			set{ }
		}

		private SlackAPI.Channel RawChannel;

		public SlackChannel(){}

		public SlackChannel (SlackAPI.Channel channel)
		{
			RawChannel = channel;
		}
	}
}

