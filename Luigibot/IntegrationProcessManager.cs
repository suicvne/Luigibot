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

namespace Luigibot
{
    public class IntegrationProcessManager
    {
        public IntegrationConfiguration Configuration { get; internal set; }

        private Dictionary<string, IIntegration> IntegrationProcesses;

        public IntegrationProcessManager()
        {
            IntegrationProcesses = new Dictionary<string, IIntegration>();
            Configuration = new IntegrationConfiguration();

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

        public void BeginIntegrations()
        {
            foreach(var integration in Configuration.Integrations)
            {
                if(Configuration.IntegrationEnabled(integration.Key))
                {
                    if (IsValidIntegration(integration.Value))
                    {
                        var theIntegration = GetIntegration(integration.Value);
                        theIntegration.MessageReceived += (sender, e) =>
                        {
                            var formatted = $"Message received from {e.Member.Name} in #{e.Channel.Name}: {e.Text}";
                            IO.Log(theIntegration.IntegrationName, theIntegration.LogColor, formatted);
                            //TODO: commands
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
                        IntegrationProcesses[integration.Key] = theIntegration;
                        IntegrationProcesses[integration.Key].StartIntegration();
                    }
                    else
                        Console.WriteLine($"'{integration.Key}' is not a valid integration.");
                }
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
