using System;
using SlackAPI;
using LuigibotCommon;
using LuigibotCommon.Integrations;

namespace LuigibotSlack
{
    public class SlackMessageEventArgs : IMessageReceivedEventArgs
	{
        public override IChannel Channel { get; set; }
        public override IMember Member { get; set;}
		public override string Text { get; set; }
	}

	public class SlackIntegration : IIntegration
	{
		#region Events
		public event EventHandler<EventArgs> Connected;
        #endregion

        public string BoldText(string text)
        {
            return $"*{text}*";
        }

        public string ItalicizeText(string text)
        {
            return $"_{text}_";
        }

		public string IntegrationName {get{ return "Slack"; }set{ }}
		public string IntegrationDescription
		{
			get{ return "";}
			set{ }
		}

        public event EventHandler<IMessageReceivedEventArgs> MessageReceived;

		public SlackSocketClient RawClient {get{return client;}}
		SlackSocketClient client;

		public bool IsIntegrationRunning 
		{
			get
			{ 
				return (client != null ? client.IsConnected : false);
			}
			set{ }
		}

		private string Token;

		public SlackIntegration(string token)
		{
			Token = token;
		}

		/// <summary>
		/// Performs the initial Slack integration.
		/// </summary>
		public void StartIntegration()
		{
			client = new SlackSocketClient (Token);

			client.Connect ((loginResponse) => 
			{
			    Connected.Invoke(this, null);
				client.OnMessageReceived += (obj) => 
				{
				    SlackMessageEventArgs e = new SlackMessageEventArgs
					{
					    Text = obj.text,
						Channel = new SlackChannel(GetChannelByName(obj.channel)),
						Member = new SlackMember(GetUserByName(obj.user))
                    };
					MessageReceived?.Invoke(this, e);
                };
			}, 
			() => 
			{
                //socket connected
			});
		}

		private SlackAPI.Channel GetChannelByName(string name)
		{
			return client.Channels.Find (x => x.id == name);
		}

		private SlackAPI.User GetUserByName(string name)
		{
			return client.Users.Find (x => x.id == name);
		}

		public void StopIntegration()
		{
			client.CloseSocket ();
		}

		public void SendTest()
		{
		}

		public void SendMessage(string text, IChannel target)
		{
			if (target is SlackChannel) 
			{
                client.SendMessage((onSent) => { }, target.ID, text);
			}
            else
				return;
		}
	}
}

