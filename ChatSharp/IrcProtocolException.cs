using System;

namespace ChatSharp
{
    public class IrcProtocolException : Exception
    {
        public IrcProtocolException()
        {
        }

        public IrcProtocolException(string message) : base(message)
        {
            
        }
    }
}
