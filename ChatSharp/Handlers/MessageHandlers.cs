using ChatSharp.Events;
using System.Linq;
using System;

namespace ChatSharp.Handlers
{
    internal static class MessageHandlers
    {
        public static void RegisterDefaultHandlers(IrcClient client)
        {
            // General
            client.SetHandler("PING", HandlePing);
            client.SetHandler("NOTICE", HandleNotice);
            client.SetHandler("PRIVMSG", HandlePrivmsg);
            client.SetHandler("MODE", HandleMode);
            //client.SetHandler("324", HandleMode);
            client.SetHandler("NICK", HandleNick);
            client.SetHandler("QUIT", HandleQuit);
            client.SetHandler("431", HandleErronousNick);
            client.SetHandler("432", HandleErronousNick);
            client.SetHandler("433", HandleErronousNick);
            client.SetHandler("436", HandleErronousNick);

            // MOTD Handlers
            client.SetHandler("375", MOTDHandlers.HandleMOTDStart);
            client.SetHandler("372", MOTDHandlers.HandleMOTD);
            client.SetHandler("376", MOTDHandlers.HandleEndOfMOTD);
            client.SetHandler("422", MOTDHandlers.HandleMOTDNotFound);

            // Channel handlers
            client.SetHandler("JOIN", ChannelHandlers.HandleJoin);
            client.SetHandler("PART", ChannelHandlers.HandlePart);
            client.SetHandler("332", ChannelHandlers.HandleGetTopic);
            client.SetHandler("331", ChannelHandlers.HandleGetEmptyTopic);
            client.SetHandler("353", ChannelHandlers.HandleUserListPart);
            client.SetHandler("366", ChannelHandlers.HandleUserListEnd);
            client.SetHandler("KICK", ChannelHandlers.HandleKick);

            // User handlers
            client.SetHandler("311", UserHandlers.HandleWhoIsUser);
            client.SetHandler("312", UserHandlers.HandleWhoIsServer);
            client.SetHandler("313", UserHandlers.HandleWhoIsOperator);
            client.SetHandler("317", UserHandlers.HandleWhoIsIdle);
            client.SetHandler("318", UserHandlers.HandleWhoIsEnd);
            client.SetHandler("319", UserHandlers.HandleWhoIsChannels);
            client.SetHandler("330", UserHandlers.HandleWhoIsLoggedInAs);

            // Listing handlers
            client.SetHandler("367", ListingHandlers.HandleBanListPart);
            client.SetHandler("368", ListingHandlers.HandleBanListEnd);
            client.SetHandler("348", ListingHandlers.HandleExceptionListPart);
            client.SetHandler("349", ListingHandlers.HandleExceptionListEnd);
            client.SetHandler("346", ListingHandlers.HandleInviteListPart);
            client.SetHandler("347", ListingHandlers.HandleInviteListEnd);
            client.SetHandler("728", ListingHandlers.HandleQuietListPart);
            client.SetHandler("729", ListingHandlers.HandleQuietListEnd);

            // Server handlers
            client.SetHandler("004", ServerHandlers.HandleMyInfo);
            client.SetHandler("005", ServerHandlers.HandleISupport);
        }

        public static void HandleNick(IrcClient client, IrcMessage message)
        {
            var ircUser = new IrcUser(message.Prefix);

            var oldNick = ircUser.Nick;
            var users = client
                    .Channels
                    .SelectMany(c => c.Users.Where(u => u.Nick.Equals(ircUser.Nick, StringComparison.OrdinalIgnoreCase)));

            foreach (var user in users)
            {
                if (user != null)
                {
                    user.Nick = message.Parameters[0];
                }
            }

            if (client.User.Nick.Equals(ircUser.Nick, System.StringComparison.OrdinalIgnoreCase))
            {
                client.User.Nick = message.Parameters[0];
            }

            client.OnNickChanged(new NickChangedEventArgs
            {
                User = ircUser,
                OldNick = oldNick,
                NewNick = message.Parameters[0]
            });
        }

        public static void HandleQuit(IrcClient client, IrcMessage message)
        {
            var user = new IrcUser(message.Prefix);
            if (client.User.Nick != user.Nick)
            {
                var channels = client
                                .Channels
                                .Where(c => c.Users.Any(u => u.Nick.Equals(user.Nick)));
                var users = channels
                    .SelectMany(c=> c.UsersByMode)
                    .Where(m => m.Value.Contains(user.Nick))
                    .Select(c => c.Value);

                foreach (var userCollection in users)
                {
                    userCollection.Remove(user.Nick);
                }

                foreach (var channel in channels)
                {
                    channel.Users.Remove(user.Nick);
                }


            }
        }

        public static void HandlePing(IrcClient client, IrcMessage message)
        {
            client.ServerNameFromPing = message.Parameters[0];
            client.SendRawMessage("PONG :{0}", message.Parameters[0]);
        }

        public static void HandleNotice(IrcClient client, IrcMessage message)
        {
            client.OnNoticeRecieved(new IrcNoticeEventArgs(message));
        }

        public static void HandlePrivmsg(IrcClient client, IrcMessage message)
        {
            var eventArgs = new PrivateMessageEventArgs(message, client.ServerInfo);
            client.OnPrivateMessageRecieved(eventArgs);
            if (eventArgs.PrivateMessage.IsChannelMessage)
            {
                try
                {
                    // Populate this user's hostname and user from the message
                    // TODO: Merge all users from all channels into one list and keep references to which channels they're in
                    var channel = client.Channels[eventArgs.PrivateMessage.Source];
                    var u = channel.Users[eventArgs.PrivateMessage.User.Nick];
                    u.Hostname = eventArgs.PrivateMessage.User.Hostname;
                    u.User = eventArgs.PrivateMessage.User.User;
                }
                catch { /* silently ignored */ }
                client.OnChannelMessageRecieved(eventArgs);
            }
            else
                client.OnUserMessageRecieved(eventArgs);
        }

        public static void HandleErronousNick(IrcClient client, IrcMessage message)
        {
            var eventArgs = new ErronousNickEventArgs(client.User.Nick);
            if (message.Command == "433") // Nick in use
                client.OnNickInUse(eventArgs);
            // else ... TODO
            if (!eventArgs.DoNotHandle && client.Settings.GenerateRandomNickIfRefused)
                client.Nick(eventArgs.NewNick);
        }

        public static void HandleMode(IrcClient client, IrcMessage message)
        {
            string target, mode = null;
            int i = 2;
            if (message.Command == "MODE")
            {
                target = message.Parameters[0];
                mode = message.Parameters[1];
            }
            else
            {
                target = message.Parameters[1];
                mode = message.Parameters[2];
                i++;
            }
            // Handle change
            bool add = true;
            if (target.StartsWith("#"))
            {
                var channel = client.Channels[target];
                try
                {
                    foreach (char c in mode)
                    {
                        if (c == '+')
                        {
                            add = true;
                            continue;
                        }
                        if (c == '-')
                        {
                            add = false;
                            continue;
                        }
                        if (channel.Mode == null)
                            channel.Mode = string.Empty;
                        // TODO: Support the ones here that aren't done properly
                        if (client.ServerInfo.SupportedChannelModes.ParameterizedSettings.Contains(c))
                        {
                            client.OnModeChanged(new ModeChangeEventArgs(channel.Name, new IrcUser(message.Prefix), 
                                (add ? "+" : "-") + c.ToString() + " " + message.Parameters[i++]));
                        }
                        else if (client.ServerInfo.SupportedChannelModes.ChannelLists.Contains(c))
                        {
                            client.OnModeChanged(new ModeChangeEventArgs(channel.Name, new IrcUser(message.Prefix), 
                                (add ? "+" : "-") + c.ToString() + " " + message.Parameters[i++]));
                        }
                        else if (client.ServerInfo.SupportedChannelModes.ChannelUserModes.Contains(c))
                        {
                            if (!channel.UsersByMode.ContainsKey(c)) channel.UsersByMode.Add(c, new UserCollection());
                            var user = new IrcUser(message.Parameters[i]);
                            if (add)
                            {
                                if (!channel.UsersByMode[c].Contains(user.Nick))
                                    channel.UsersByMode[c].Add(user);
                            }
                            else
                            {
                                if (channel.UsersByMode[c].Contains(user.Nick))
                                    channel.UsersByMode[c].Remove(user);
                            }
                            client.OnModeChanged(new ModeChangeEventArgs(channel.Name, new IrcUser(message.Prefix), 
                                (add ? "+" : "-") + c.ToString() + " " + message.Parameters[i++]));
                        }
                        if (client.ServerInfo.SupportedChannelModes.Settings.Contains(c))
                        {
                            if (add)
                            {
                                if (!channel.Mode.Contains(c))
                                    channel.Mode += c.ToString();
                            }
                            else
                                channel.Mode = channel.Mode.Replace(c.ToString(), string.Empty);
                            client.OnModeChanged(new ModeChangeEventArgs(channel.Name, new IrcUser(message.Prefix), 
                                (add ? "+" : "-") + c.ToString()));
                        }
                    }
                }
                catch { }
                if (message.Command == "324")
                {
                    var operation = client.RequestManager.DequeueOperation("MODE " + channel.Name);
                    operation.Callback(operation);
                }
            }
            else
            {
                // TODO: Handle user modes other than ourselves?
                foreach (char c in mode)
                {
                    if (add)
                    {
                        if (!client.User.Mode.Contains(c))
                            client.User.Mode += c;
                    }
                    else
                        client.User.Mode = client.User.Mode.Replace(c.ToString(), string.Empty);
                }
            }
        }
    }
}
