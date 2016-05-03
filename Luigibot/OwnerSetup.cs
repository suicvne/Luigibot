using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot
{
    /// <summary>
    /// Handles proper setting of owners and whatnot
    /// </summary>
    public class OwnerSetup
    {
        private IntegrationConfiguration ConfigurationToApplyTo { get; set; }
        public string Code = RandomCodeGenerator.GenerateRandomCode();
        public int IntegrationVerificationCount { get; set; } = 0;
        public OwnerSetup(IntegrationConfiguration conf)
        {
            ConfigurationToApplyTo = conf;
        }
    }
}
