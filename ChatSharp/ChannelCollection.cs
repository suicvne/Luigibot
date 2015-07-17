using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChatSharp
{
    public class ChannelCollection : IEnumerable<IrcChannel>
    {
        internal ChannelCollection(IrcClient client)
        {
            Channels = new List<IrcChannel>();
            Client = client;
        }

        private IrcClient Client { get; set; }
        private List<IrcChannel> Channels { get; set; }

        internal void Add(IrcChannel channel)
        {
            if (Channels.Any(c => c.Name == channel.Name))
                throw new InvalidOperationException("That channel already exists in this collection.");
            Channels.Add(channel);
        }

        internal void Remove(IrcChannel channel)
        {
            Channels.Remove(channel);
        }

		public int Count()
		{
			return Channels.Count;
		}

        public void Join(string name)
        {
            Client.JoinChannel(name);
        }

        public bool Contains(string name)
        {
            return Channels.Any(c => c.Name == name);
        }

        public IrcChannel this[int index]
        {
            get
            {
                return Channels[index];
            }
        }

        public IrcChannel this[string name]
        {
            get
            {
                var channel = Channels.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
                if (channel == null)
                    throw new KeyNotFoundException();
                return channel;
            }
        }

        public IEnumerator<IrcChannel> GetEnumerator()
        {
            return Channels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
