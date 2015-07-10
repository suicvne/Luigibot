using ChatSharp.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChatSharp.Handlers
{
    internal static class ChannelHandlers
    {
        public static void HandleJoin(IrcClient client, IrcMessage message)
        {
            IrcChannel channel = null;
            if (client.User.Nick == new IrcUser(message.Prefix).Nick)
            {
                // We've joined this channel
                channel = new IrcChannel(client, message.Parameters[0]);
                client.Channels.Add(channel);
            }
            else
            {
                // Someone has joined a channel we're already in
                channel = client.Channels[message.Parameters[0]];
                channel.Users.Add(new IrcUser(message.Prefix));
            }
            if (channel != null)
                client.OnUserJoinedChannel(new ChannelUserEventArgs(channel, new IrcUser(message.Prefix)));
        }

        public static void HandleGetTopic(IrcClient client, IrcMessage message)
        {
            IrcChannel channel = null;
            string topic;
            if (client.Channels.Contains(message.Parameters[1]))
            {
                channel = client.Channels[message.Parameters[1]];
                topic = message.Parameters[2];
                channel.Topic = topic;
            }
            else
            {
                channel = new IrcChannel(client, message.Parameters[1]);
                topic = message.Parameters[2];
            }
            client.OnChannelTopicReceived(new ChannelTopicEventArgs(channel, topic));
        }

        public static void HandleGetEmptyTopic(IrcClient client, IrcMessage message)
        {
            var channel = client.Channels[message.Parameters[1]];
            var topic = message.Parameters[2];
            client.OnChannelTopicReceived(new ChannelTopicEventArgs(channel, topic));
        }

        public static void HandlePart(IrcClient client, IrcMessage message)
        {
            if (!client.Channels.Contains(message.Parameters[0]))
                return; // we already parted the channel, ignore

            if (client.User.Match(message.Prefix)) // We've parted this channel
                client.Channels.Remove(client.Channels[message.Parameters[0]]);
            else // Someone has parted a channel we're already in
            {
                var user = new IrcUser(message.Prefix).Nick;
                var channel = client.Channels[message.Parameters[0]];
                if (channel.Users.Contains(user))
                    channel.Users.Remove(user);
                foreach (var mode in channel.UsersByMode)
                {
                    if (mode.Value.Contains(user))
                        mode.Value.Remove(user);
                }
                client.OnUserPartedChannel(new ChannelUserEventArgs(client.Channels[message.Parameters[0]], new IrcUser(message.Prefix)));
            }
        }

        public static void HandleUserListPart(IrcClient client, IrcMessage message)
        {
            var channel = client.Channels[message.Parameters[2]];
            var users = message.Parameters[3].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user)) continue;
                var mode = client.ServerInfo.GetModeForPrefix(user[0]);
                if (mode == null)
                    channel.Users.Add(new IrcUser(user));
                else
                {
                    channel.Users.Add(new IrcUser(user.Substring(1)));
                    if (!channel.UsersByMode.ContainsKey(mode.Value))
                        channel.UsersByMode.Add(mode.Value, new UserCollection());
                    channel.UsersByMode[mode.Value].Add(new IrcUser(user.Substring(1)));
                }
            }
        }

        public static void HandleUserListEnd(IrcClient client, IrcMessage message)
        {
            var channel = client.Channels[message.Parameters[1]];
            client.OnChannelListRecieved(new ChannelEventArgs(channel));
            if (client.Settings.ModeOnJoin)
            {
                try
                {
                    client.GetMode(channel.Name, c => Console.WriteLine(c.Mode));
                }
                catch { }
            }
            if (client.Settings.WhoIsOnJoin)
            {
                Task.Factory.StartNew(() => WhoIsChannel(channel, client, 0));
            }
        }

        private static void WhoIsChannel(IrcChannel channel, IrcClient client, int index)
        {
            // Note: joins and parts that happen during this will cause strange behavior here
            Thread.Sleep(client.Settings.JoinWhoIsDelay * 1000);
            var user = channel.Users[index];
            client.WhoIs(user.Nick, (whois) =>
                {
                    user.User = whois.User.User;
                    user.Hostname = whois.User.Hostname;
                    user.RealName = whois.User.RealName;
                    Task.Factory.StartNew(() => WhoIsChannel(channel, client, index + 1));
                });
        }

        public static void HandleKick(IrcClient client, IrcMessage message)
        {
            var channel = client.Channels[message.Parameters[0]];
            var kicked = channel.Users[message.Parameters[1]];
            if (string.Equals(message.Parameters[1], client.User.Nick, StringComparison.OrdinalIgnoreCase)) // We've been kicked
                client.Channels.Remove(client.Channels[message.Parameters[0]]);
            else
            {
                if (channel.Users.Contains(message.Parameters[1]))
                    channel.Users.Remove(message.Parameters[1]);
                foreach (var mode in channel.UsersByMode.Where(mode => mode.Value.Contains(message.Parameters[1])))
                {
                    mode.Value.Remove(message.Parameters[1]);
                }
            }
            client.OnUserKicked(new KickEventArgs(channel, new IrcUser(message.Prefix),
                kicked, message.Parameters[2]));
        }
    }
}
