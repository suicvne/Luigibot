using System;
using LuigibotSlack;
using System.Threading;
using DiscordSharp.Commands;
using Luigibot.Commands.CommonCommands.Modules;
using System.Collections.Generic;
using LuigibotCommon.Integrations;
using System.Threading.Tasks;
using Luigibot.Discord;

namespace Luigibot
{
    enum IntegrationType
    {
        Discord, Slack, IRC
    }

    public class Luigibot
    {
        private Dictionary<IntegrationType, CommandsManager> CommandManagers;
        private Dictionary<IntegrationType, Task> IntegrationThreads;
        private Dictionary<IntegrationType, CancellationTokenSource> IntegrationCancellations;

        private void Log(IntegrationType integration, string text)
        {
            switch(integration)
            {
                case IntegrationType.Slack:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"[Slack] ");
                    break;
                case IntegrationType.Discord:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write($"[Discord] ");
                    break;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
        }

        public Luigibot()
        {
            CommandManagers = new Dictionary<IntegrationType, CommandsManager>();
            IntegrationThreads = new Dictionary<IntegrationType, Task>();
            IntegrationCancellations = new Dictionary<IntegrationType, CancellationTokenSource>();
        }

        public void Begin()
        {
            SetupIntegrations();
        }

        public void End()
        {
            StopIntegrations();
        }
        
        private void SetupCommands(IntegrationType type, IIntegration integration)
        {
            if (!CommandManagers.ContainsKey(type))
            {
                CommandManagers[type] = new CommandsManager(integration);
            }

            BaseOwnerModule owner = new BaseOwnerModule(this);
            owner.Install(CommandManagers[type]);

            FunModule fModule = new FunModule();
            fModule.Install(CommandManagers[type]);
        }

        private void SetupIntegrations()
        {
            IntegrationThreads.Clear();
            foreach (IntegrationType integration in Enum.GetValues(typeof(IntegrationType)))
            {
                if (integration == IntegrationType.Discord)
                {
                    IntegrationCancellations[integration] = new CancellationTokenSource();
                    IntegrationThreads[integration] = Task.Run(() =>
                    {
                        DiscordIntegration discord = new DiscordIntegration("");
                        SetupCommands(IntegrationType.Discord, discord);

                        discord.StartIntegration();

                        discord.Connected += (sender, e) =>
                        {
                            Log(IntegrationType.Discord, $"Discord connected!");
                        };
                        discord.MessageReceived += (sender, e) =>
                        {
                            Log(IntegrationType.Discord, string.Format("Message from {0} in #{1}: {2}", e.Member.Name, e.Channel.Name, e.Text));
                            if(e.Text.Length > 0 && e.Text[0] == '-')
                            {
                                try
                                {
                                    CommandManagers[IntegrationType.Discord].ExecuteOnMessageCommand(e.Text.Substring(1), e.Channel, e.Member);
                                }
                                catch { }
                            }
                        };
                    }, IntegrationCancellations[integration].Token);
                }
                else if (integration == IntegrationType.Slack)
                {
                    IntegrationCancellations[integration] = new CancellationTokenSource();
                    IntegrationThreads[integration] = Task.Run(() =>
                    {
                        SlackIntegration slackClient = new SlackIntegration("");
                        SetupCommands(IntegrationType.Slack, slackClient);

                        slackClient.StartIntegration();

                        slackClient.Connected += (sender, e) =>
                        {
                            Log(integration, "Connected to Slack!");
                        };
                        slackClient.MessageReceived += (x, s) =>
                        {
                            Log(integration, string.Format("Message from {0} in {1}: {2}", s.Member.Name, s.Channel.Name, s.Text));
                            if (s.Text.Length > 0 && s.Text[0] == '-')
                            {
                                try
                                {
                                    CommandManagers[IntegrationType.Slack].ExecuteOnMessageCommand(s.Text.Substring(1), s.Channel, s.Member);
                                }
                                catch { }
                            }
                        };
                    }, IntegrationCancellations[integration].Token);
                }
            }
        }

        private void StopIntegrations()
        {
            foreach (IntegrationType integration in Enum.GetValues(typeof(IntegrationType)))
            {
                if (IntegrationThreads.ContainsKey(integration))
                {
                    IntegrationCancellations[integration].Cancel(false);
                    
                }
            }
        }
    }

	public class MainClass
	{
		public static void Main (string[] args)
		{
            var luigibot = new Luigibot();
            luigibot.Begin();
			Console.ReadLine ();
            luigibot.End();
		}
	}
}
