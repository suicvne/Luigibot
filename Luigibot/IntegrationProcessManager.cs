using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection;
using LuigibotCommon.Integrations;
using LuigibotCommon;
using DiscordSharp.Commands;
using Luigibot.Commands.CommonCommands.Modules;

namespace Luigibot
{
    public class IntegrationProcessManager
    {
        public IntegrationConfiguration Configuration { get; internal set; }

        private Dictionary<string, IIntegration> IntegrationProcesses;
        private Dictionary<string, CommandsManager> CommandManagers;

        private bool RequiresVerification = false;

        public IntegrationProcessManager()
        {
            IntegrationProcesses = new Dictionary<string, IIntegration>();
            Configuration = new IntegrationConfiguration();
            CommandManagers = new Dictionary<string, CommandsManager>();

            if (File.Exists("config.json"))
            {
                JsonSerializer js = new JsonSerializer();
                js.Formatting = Formatting.Indented;
                using (StreamReader ser = new StreamReader(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "config.json"))
                {
                    using (JsonReader jsr = new JsonTextReader(ser))
                    {
                        Configuration = js.Deserialize<IntegrationConfiguration>(jsr);
                    }
                }
            }
            else
                Configuration = new IntegrationConfiguration();
        }
        
        public void AddIntegration(string name, string path)
        {
            Configuration.Integrations[name] = path;
            Configuration.IntegrationsEnabled[name] = true;
            Configuration.Prefixes[name] = '-';
        }

        private bool IsValidIntegration(string integrationPath)
        {
            if (File.Exists(integrationPath))
            {
                Assembly integration = Assembly.LoadFrom(integrationPath);
                Type type = integration.GetType("EntryPoint");
                if (type != null)
                {
                    object o = Activator.CreateInstance(type);
                    if (o != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private IIntegration GetIntegration(string integrationDllFile)
        {
            Assembly integration = Assembly.LoadFile(integrationDllFile);
            Type type = integration.GetType("EntryPoint");
            object o = Activator.CreateInstance(type); //todo check this
            if(o != null)
            {
                IIntegration theIntegration = (o as IEntryPoint).CreateIntegration();
                return theIntegration;
            }
            return null;
        }

        private void SetupCommands(CommandsManager manager)
        {
            BaseOwnerModule owner = new BaseOwnerModule(this);
            owner.Install(manager);

            FunModule fModule = new FunModule();
            fModule.Install(manager);
        }

        public void StopIntegration(string integration)
        {
            if(IntegrationProcesses.ContainsKey(integration))
            {
                IntegrationProcesses[integration].StopIntegration();
                IntegrationProcesses.Remove(integration);
            }
        }

        public void StartIntegration(string integration)
        {
            if(Configuration.Integrations.ContainsKey(integration))
            {
                if(IsValidIntegration(Configuration.Integrations[integration]))
                {
                    IntegrationSetup(integration, Configuration.Integrations[integration]);
                }
            }
        }

        public void RestartIntegration(string integration)
        {
            StopIntegration(integration);
            StartIntegration(integration);
        }

        private void IntegrationSetup(string integrationKey, string integrationPath)
        {
            var theIntegration = GetIntegration(integrationPath);
            CommandManagers[integrationKey] = new CommandsManager(theIntegration);
            if(!RequiresVerification)
            {
                CommandManagers[integrationKey].AddPermission(Configuration.Owners[integrationKey], PermissionType.Owner);
            }
            SetupCommands(CommandManagers[integrationKey]);
            theIntegration.MessageReceived += (sender, e) =>
            {
                var formatted = $"Message received from {e.Member.Name} in #{e.Channel.Name}: {e.Text}";
                IO.Log(theIntegration.IntegrationName, theIntegration.LogColor, formatted);

                if (e.Text.Length > 0 && e.Text[0] == Configuration.Prefixes[integrationKey])
                {
                    try
                    {
                        CommandManagers[integrationKey].ExecuteOnMessageCommand(e.Text.Substring(1), e.Channel, e.Member);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine($"Error in commands for integration {integrationKey}: {ex.Message}\n\t{ex.StackTrace}");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                        theIntegration.SendMessage($"Error: `{ex.Message}`", e.Channel);
                    }
                }

                if(RequiresVerification)
                {
                    if(e.Text.Length > 0 && e.Text.StartsWith("?authenticate"))
                    {
                        if (Verification(e.Text, e.Member, integrationKey))
                            theIntegration.SendMessage("Verified!", e.Channel);
                    }
                }
            };
            theIntegration.Connected += (sender, e) =>
            {
                IO.Log(theIntegration.IntegrationName, theIntegration.LogColor, "Connected!");
            };
            theIntegration.ConnectionClosed += (sender, e) =>
            {
                var formatted = $"";
                IO.Log(theIntegration.IntegrationName, ConsoleColor.Red, formatted);
            };
            IntegrationProcesses[integrationKey] = theIntegration;
            IntegrationProcesses[integrationKey].StartIntegration();
        }

        private OwnerSetup Setup;
        private bool Verification(string rawText, IMember member, string integration)
        {
            string code = rawText.Substring(rawText.IndexOf(" ") + 1).Trim();
            if(code == Setup.Code)
            {
                Console.WriteLine($"Verified {member.Name} with ID {member.ID} as owner of integration {integration}");
                Configuration.Owners.Add(integration, member.ID);
                CommandManagers[integration].AddPermission(member, PermissionType.Owner);
                SaveSettings();
                return true;
            }
            return false;
        }

        public void BeginIntegrations()
        {
            foreach(var integration in Configuration.Integrations)
            {
                if(Configuration.IntegrationEnabled(integration.Key))
                {
                    if (IsValidIntegration(integration.Value))
                    {
                        if (!Configuration.Owners.ContainsKey(integration.Key))
                            RequiresVerification = true;
                        IntegrationSetup(integration.Key, integration.Value);
                    }
                    else
                        Console.WriteLine($"'{integration.Key}' is not a valid integration.");
                }
            }
            if(RequiresVerification)
            {
                Setup = new OwnerSetup(Configuration);
                IO.Log("Luigibot", ConsoleColor.Cyan, $"You need to verify yourself as owner in some or all of your integrations.\n\tPlease enter this in the integration service to verify:\n\t?authenticate {Setup.Code}");
            }
        }

        public void SaveSettings()
        {
            JsonSerializer js = new JsonSerializer();
            js.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "config.json"))
            {
                using (JsonWriter jsw = new JsonTextWriter(sw))
                {
                    js.Serialize(jsw, Configuration);
                }
            }
        }

    }
}
