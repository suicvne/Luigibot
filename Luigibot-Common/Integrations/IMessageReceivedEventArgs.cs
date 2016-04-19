using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuigibotCommon.Integrations
{
    public abstract class IMessageReceivedEventArgs : EventArgs
    {
        public abstract string Text { get; set; }
        public abstract IChannel Channel { get; set; }
        public abstract IMember Member { get; set; }
    }
}
