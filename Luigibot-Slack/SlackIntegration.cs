using System;
using Luigibot;
using SlackAPI;
using LuigibotCommon;

namespace LuigibotSlack
{
	public class SlackMessageEventArgs : EventArgs
	{
		public SlackChannel Channel {get;internal set;}
		public SlackMember Member {get;internal set;}
		public string Content { get;internal set; }
	}

	public class SlackIntegration : IIntegration
	{
		#region Events
		public event EventHandler<EventArgs> Connected;
		#endregion

		public string IntegrationName {get{ return "Luigibot Slack Integration"; }set{ }}
		public string IntegrationDescription
		{
			get{ return "";}
			set{ }
		}

		public event EventHandler<SlackMessageEventArgs> MessageReceived;

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
					//logged in
					Console.WriteLine("Logged in! User: " + loginResponse.self.name +
						"\nGood? " + client.IsConnected);
					foreach(var chan in client.Channels)
						Console.WriteLine(chan.name);
					
					Connected.Invoke(this, null);
					client.OnMessageReceived += (obj) => 
					{
						SlackMessageEventArgs e = new SlackMessageEventArgs
						{
							Content = obj.text,
							Channel = new SlackChannel(GetChannelByName(obj.channel)),
							Member = new SlackMember(GetUserByName(obj.user))
						};
						MessageReceived?.Invoke(this, e);
					};
			}, 
			() => 
			{
					//socket connected
					Console.WriteLine("Socket started.");
			});
		}

		private SlackAPI.Channel GetChannelByName(string name)
		{
			return client.Channels.Find (x => x.name == name);
		}

		private SlackAPI.User GetUserByName(string name)
		{
			return client.Users.Find (x => x.name == name);
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
				client.PostMessage ((pmr) => 
				{
						Console.WriteLine(pmr.error);
					}, target.ID, text, "mikebot", as_user: true);
			} else
				return;
		}
	}
}

