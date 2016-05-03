using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot
{
    public class IntegrationConfiguration
    {
        /// <summary>
        /// A dictionary of integrations to use.
        /// The key being the name, the value being the path to the executable.
        /// </summary>
        [JsonProperty("integrations")]
        public Dictionary<string, string> Integrations { get; set; }

        /// <summary>
        /// Key = name of integration
        /// Value = true if enabled; false if disabled.
        /// </summary>
        [JsonProperty("enabled_integrations")]
        public Dictionary<string, bool> IntegrationsEnabled { get; set; }

        /// <summary>
        /// A dictionary of the command prefixes to use internally.
        /// Key is the name of the integration and the value is the actual prefix char.
        /// </summary>
        [JsonProperty("command_prefixes")]
        public Dictionary<string, char> Prefixes { get; set; }

        /// <summary>
        /// A dictionary of the owners.
        /// Key is the integration and the value is the owner's ID in the integration.
        /// </summary>
        [JsonProperty("owners")]
        public Dictionary<string, string> Owners { get; set; }

        public IntegrationConfiguration()
        {
            Integrations = new Dictionary<string, string>();
            Prefixes = new Dictionary<string, char>();
            IntegrationsEnabled = new Dictionary<string, bool>();
            Owners = new Dictionary<string, string>();
        }
        
        public bool IntegrationEnabled(string key)
        {
            if(IntegrationsEnabled.ContainsKey(key))
            {
                return IntegrationsEnabled[key];
            }
            else
            {
                IntegrationsEnabled[key] = true;
                return true; //enabled by default
            }
        }
    }
}
