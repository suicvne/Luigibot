using System;

namespace LuigibotCommon.Integrations
{
    public abstract class IConnectionClosedEventArgs : EventArgs
    {
        public string Reason { get; set; }
        public int? Code { get; set; }
        public bool? Clean { get; set; }
    }
}