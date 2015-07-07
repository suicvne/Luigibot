using ChatSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Luigibot2
{
    static class Program
    {
        private static List<IrcUserAndSeen> UsersList = new List<IrcUserAndSeen>();
        private static List<IrcUserAndSeen> UserDatabase = new List<IrcUserAndSeen>();
        private static Thread updateUsersListThread = new Thread(UpdateUsersList);
        private static Thread InputThread = new Thread(Input);
        private static IrcClient client;
        private static string[] EightballMessages = new string[] { "Signs point to yes.", "Yes.", "Reply hazy, try again.", "Without a doubt", "My sources say no", "As I see it, yes.", "You may rely on it.", "Concentrate and ask again", "Outlook not so good", "It is decidedly so", "Better not tell you now.", "Very doubtful", "Yes - definitely", "It is certain", "Cannot predict now", "Most likely", "Ask again later", "My reply is no", "Outlook good", "Don't count on it" };
        private static Random random = new Random(Environment.TickCount);

        private static bool eightballEnabled = true;

        [STAThread]
        public static void Main(string[] args)
        {
            InputThread.Start();
            RunConsoleOnly();
        }

        private static void RunConsoleOnly()
        {
            RunBotTest();
        }

        private static void Input()
        {
            bool accept = true;
            while(accept)
            {
                string input = Console.ReadLine();
                if(input.StartsWith("/"))
                {
                    if (input.StartsWith("/enable8ball") || input.StartsWith("/enableeightball"))
                    {
                        eightballEnabled = true;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Enabling Eight Ball");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (input.StartsWith("/disable8ball") || input.StartsWith("disableeightball"))
                    {
                        eightballEnabled = false;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Disabling Eight Ball");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    if(client.Channels.Count() > 0)
                        client.SendRawMessage(String.Format("PRIVMSG {0} :{1}", client.Channels[0].Name, input));
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("ERROR: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Not in a channel.\n");
                    }
                }
            }
        }

        private static void UpdateUsersList()
        {
            bool done = false;
            while(!done)
            {
                UsersList.Clear();
                
                if(client != null)
                {
                    if (client.Channels.Count() > 0)
                    {
                        IrcChannel channel = client.Channels[0];
                        foreach (var user in channel.Users)
                        {
                            UsersList.Add(new IrcUserAndSeen((IrcUser)user));
                        }
                    }
                }
                Thread.Sleep(3 * 1000); //every 3 seconds, update the users list
            }
        }

        private static void RunBotTest()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connecting...");
            Console.ForegroundColor = ConsoleColor.White;

            client = new IrcClient("irc.stardustfields.net", new IrcUser("Luigibot2015", "luigibot2"));
            client.ConnectionComplete += (s, e) => 
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Connected! Joining #smbx..");
                Console.ForegroundColor = ConsoleColor.White;

                Console.ForegroundColor = ConsoleColor.Green;
                client.JoinChannel("#luigibot");
                Console.WriteLine("Connected!");
                Console.ForegroundColor = ConsoleColor.White;

                updateUsersListThread.Start();
            };
            client.UserPartedChannel += (s, e) =>
                {
                    DateTime now = DateTime.Now;
                    UserDatabase.Add(new IrcUserAndSeen(e.User, now));
                };
            client.UserJoinedChannel += (s, e) =>
                {
                    if(e.User.Nick != client.User.Nick)
                        client.SendAction("welcomes " + e.User.Nick + "!", client.Channels[0].Name);
                    UsersList.Add(new IrcUserAndSeen(e.User));
                };
            client.NoticeRecieved += (s, e) =>
            {
                Console.WriteLine("NOTICE FROM {0}: {1}", e.Source, e.Notice.ToString());
            };
            client.ChannelMessageRecieved += (s, e) =>
                {
                    var channel = client.Channels[e.PrivateMessage.Source];
                    Console.WriteLine("{0} - {1}: {2}", e.PrivateMessage.Source, e.PrivateMessage.User, e.PrivateMessage.Message); //just output the stuff boss

                    if(e.PrivateMessage.Message.StartsWith("!"))
                    {
                        InterpretCommand(e.PrivateMessage.Message, e.PrivateMessage.User, client);
                    }
                };
            client.ConnectAsync();

            while (true) ; //just keeps everything going
        }

        /// <summary>
        /// Interprets commands
        /// </summary>
        /// <param name="command">The full message</param>
        /// <param name="sender">The full sender</param>
        /// <param name="recipient">The full recipient</param>
        private static void InterpretCommand(string command, IrcUser sender, IrcClient client)
        {
            if(command.StartsWith("!slap"))
            {
                string[] splitCommand = command.Split(new char[]{' '}, 2);
                if (splitCommand.Length > 1)
                {
                    var listCopy = UsersList;
                    bool foundUser = false;
                    try
                    {
                        foreach (IrcUserAndSeen user in listCopy)
                        {
                            if (user.User.Nick.ToLower() == splitCommand[1].ToLower().Trim())
                            {
                                if (user.User.Nick == client.User.Nick)
                                    client.SendRawMessage("PRIVMSG {0} :What'd I do? :(", client.Channels[0].Name);
                                else
                                    client.SendRawMessage("PRIVMSG {0} :" + "\x01" + "ACTION slaps {1} with a giant fish.\x01", client.Channels[0].Name, user.User.Nick);
                                foundUser = true;
                                break;
                            }
                        }
                        if (!foundUser)
                            client.SendRawMessage("PRIVMSG {0} :User not found!");
                    }
                    catch (Exception ex) { }
                }
                else
                    client.SendRawMessage("PRIVMSG {0} :Slap who?", client.Channels[0].Name);
            }
            if(command.StartsWith("!eightball") || command.StartsWith("!8ball") || command.StartsWith("!fortune"))
            {
                if (eightballEnabled)
                {
                    int ranMessage = random.Next(EightballMessages.Length - 1);
                    if (command.ToLower().Contains("waluigibot1337"))
                        client.SendRawMessage("PRIVMSG {0} :Waluigibot is a tool and will never come to fruition.", client.Channels[0].Name);
                    else
                        client.SendRawMessage("PRIVMSG {0} :{1}", client.Channels[0].Name, EightballMessages[ranMessage]);
                }
            }
            if(command.StartsWith("!enable8ball") || command.StartsWith("!enableeightball"))
            {
                if (sender.Nick.ToLower() == "luigifan2010"
                    || sender.Nick.ToLower() == "joey"
                    || sender.Nick.ToLower() == "ghosthawk"
                    || sender.Nick.ToLower() == "aeromatter")
                    eightballEnabled = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Enabling Eight Ball");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (command.StartsWith("!disable8ball") || command.StartsWith("!disableeightball"))
            {
                if (sender.Nick.ToLower() == "luigifan2010"
                    || sender.Nick.ToLower() == "joey"
                    || sender.Nick.ToLower() == "ghosthawk"
                    || sender.Nick.ToLower() == "aeromatter")
                    eightballEnabled = false;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Disabling Eight Ball");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if(command.StartsWith("!selfdestruct"))
            {
                if (sender.Nick.ToLower() == "luigifan2010" 
                    || sender.Nick.ToLower() == "joey" 
                    || sender.Nick.ToLower() == "ghosthawk" 
                    || sender.Nick.ToLower() == "aeromatter") //yes i know, this is super insecure but Luigibot isn't a security bot anyway he's a fun bot
                    Environment.Exit(0);
            }
            if(command.StartsWith("!42"))
            {
                client.SendRawMessage("PRIVMSG {0} :The Answer to Life, the Universe, and Everything.", client.Channels[0].Name);
            }
            if(command.StartsWith("!seen"))
            {
                string[] splitCommand = command.Split(new char[] { ' ' }, 2);
                if (splitCommand.Length > 1)
                {
                    var UsersListCopy = UsersList;
                    var UserDatabaseCopy = UserDatabase;
                    //First, check to see if the user is on now
                    bool foundInOnline = false;
                    foreach(IrcUserAndSeen user in UsersListCopy)
                    {
                        if(user.User.Nick.ToLower() == splitCommand[1].ToLower().Trim())
                        {
                            foundInOnline = true;
                            if (user.User.Nick == client.User.Nick)
                                client.SendRawMessage("PRIVMSG {0} :I'm always online c:", client.Channels[0].Name);
                            else
                                client.SendRawMessage("PRIVMSG {0} :That user is online now!", client.Channels[0].Name);
                            foundInOnline = true;
                            break;
                        }
                    }
                    //Then, we'll check the database
                    bool foundInDatabase = false;
                    if (!foundInOnline)
                    {
                        foreach (IrcUserAndSeen user in UserDatabaseCopy)
                        {
                            if (user.User.Nick.ToLower() == splitCommand[1].ToLower().Trim())
                            {
                                foundInDatabase = true;
                                client.SendRawMessage("PRIVMSG {0} :{1} was last seen at {2} (EST)", client.Channels[0].Name, user.User.Nick, user.LastSeen.ToString());
                                break;
                            }
                        }
                        if (!foundInDatabase && !foundInOnline)
                            client.SendRawMessage("PRIVMSG {0} :I'm not sure :(", client.Channels[0].Name);
                    }
                }
                else
                    client.SendRawMessage("PRIVMSG {0} :Last seen who?", client.Channels[0].Name);
            }
        }

    }
}
