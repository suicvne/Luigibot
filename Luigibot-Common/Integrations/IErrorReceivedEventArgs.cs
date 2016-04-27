using System;

namespace LuigibotCommon.Integrations
{
    public abstract class IErrorReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}