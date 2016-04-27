using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuigibotCommon.Integrations
{
    public interface IEntryPoint
    {
        IIntegration CreateIntegration();
    }
}
