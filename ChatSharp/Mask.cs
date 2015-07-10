using System;

namespace ChatSharp
{
    public class Mask
    {
        public Mask(string value, IrcUser creator, DateTime creationTime)
        {
            Value = value;
            Creator = creator;
            CreationTime = creationTime;
        }

        public IrcUser Creator { get; set; }
        public DateTime CreationTime { get; set; }
        public string Value { get; set; }
    }
}
