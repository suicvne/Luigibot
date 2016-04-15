using System;
using LuigibotSlack;
using System.Threading;

namespace Luigibot
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			SlackIntegration slackClient = new SlackIntegration ("xoxb-35027232647-GlnCEkB3VHKz8IT2NqYYXCBc");

			slackClient.StartIntegration ();

			slackClient.Connected += (sender, e) => 
			{
				slackClient.MessageReceived += (x, s) =>
				{
					Console.WriteLine("[Slack] Message from {0} in {1}: {2}", s.Member, s.Channel.Name, s.Content);

				};
			};

			Console.ReadLine ();
			slackClient.StopIntegration ();
		}
	}
}
