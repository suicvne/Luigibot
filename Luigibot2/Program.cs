using ChatSharp;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Luigibot2
{
	static class Program
	{
        private static SimpleAES encrypter = new SimpleAES();
		private static List<IrcUserAndSeen> UsersList = new List<IrcUserAndSeen> ();
		private static Thread updateUsersListThread = new Thread (UpdateUsersList);
		private static Thread InputThread = new Thread (Input);
		private static IrcClient client;
		private static string[] EightballMessages = new string[] {
			"Signs point to yes.",
			"Yes.",
			"Reply hazy, try again.",
			"Without a doubt",
			"My sources say no",
			"As I see it, yes.",
			"You may rely on it.",
			"Concentrate and ask again",
			"Outlook not so good",
			"It is decidedly so",
			"Better not tell you now.",
			"Very doubtful",
			"Yes - definitely",
			"It is certain",
			"Cannot predict now",
			"Most likely",
			"Ask again later",
			"My reply is no",
			"Outlook good",
			"Don't count on it"
		};
		private static Random random = new Random (Environment.TickCount);

		private static UserDatabase UsersSeenDatabase = new UserDatabase ();
		private static Settings ProgramSettings = new Settings ();

        private static Version ProgramVersion = Assembly.GetEntryAssembly().GetName().Version;

		[STAThread]
		public static void Main (string[] args)
		{
            Console.Title = "Luigibot - " + ProgramVersion.ToString();

			ProgramSettings.LoadSettings ();

			OutputStatusMessage (String.Format ("Enter nick to use (enter for {0}): ", ProgramSettings.settings.LastUsedNick), ':', false);
			string nick = Console.ReadLine ();
			if (nick.Trim () != "")
				ProgramSettings.settings.LastUsedNick = nick;

			OutputStatusMessage (String.Format ("Enter server to join (enter for {0}): ", ProgramSettings.settings.LastJoinedServer), ':', false);
			string server = Console.ReadLine ();
			if (server.Trim () != "")
				ProgramSettings.settings.LastJoinedServer = server;

			OutputStatusMessage (String.Format ("Enter channel to join, starting with # (enter for {0}): ", ProgramSettings.settings.LastJoinedChannel), ':', false);
			string channel = Console.ReadLine ();
			if (channel.Trim () != "")
				ProgramSettings.settings.LastJoinedChannel = channel;

			InputThread.Start ();
			RunConsoleOnly ();
		}

		private static void RunConsoleOnly ()
		{
			RunBotTest ();
		}

		private static void OutputStatusMessage (string message, bool newLine)
		{
			string[] split = message.Split (new char[] { ' ' }, 2);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write (split [0]);
			Console.ForegroundColor = ConsoleColor.White;
			if (newLine)
				Console.Write (split [1] + "\n");
			else
				Console.Write (split [1]);
		}

		private static void OutputStatusMessage (string message, char splitAt, bool newLine)
		{
			string[] split = message.Split (new char[] { splitAt }, 2);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write (split [0]);
			Console.ForegroundColor = ConsoleColor.White;
			if (newLine)
				Console.Write (split [1] + "\n");
			else
				Console.Write (split [1]);
		}


		private static void OutputHelpMessage (string message)
		{
			string[] split = message.Split (new char[]{ ' ' }, 2);
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write (split [0]);
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write (split [1] + "\n");
		}

		private static void Input ()
		{
			bool accept = true;
			while (accept)
			{
				string input = Console.ReadLine ();
				if (input.StartsWith ("/"))
				{
					if (input.StartsWith ("/help"))
					{ 
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.WriteLine ("Commands list");
						Console.ForegroundColor = ConsoleColor.White;
						OutputHelpMessage ("/me - sends an ACTION under the current bot's user.");
                        OutputHelpMessage("/msg - sends a PRIVMSG to the specified user.");
						OutputHelpMessage ("/exit - safely exists, saving databases/settings");
						OutputHelpMessage ("/disableslap - disables the slap command");
						OutputHelpMessage ("/enableslap - enables the slap command");
						OutputHelpMessage ("/disable8ball,/disableeightball - disables the eight ball command");
						OutputHelpMessage ("/enable8ball,/enableeightball - enables the eight ball command");
						OutputHelpMessage ("/disablewelcome - disables the welcome message on user entry.");
						OutputHelpMessage ("/enablewelcome - enables the welcome message on user entry.");
                        OutputHelpMessage("/setnickservpass - sets the password for NickServ");
                        OutputHelpMessage("/identify - identifies with NickServ, usually does this automagically");
                        OutputHelpMessage("/register - registers the bot's current nick with NickServ");
                        OutputHelpMessage("/changeprefix - changes the command prefix for the people down there in the IRC chat."); 
                        Console.ForegroundColor = ConsoleColor.Cyan;
						Console.WriteLine ("---End Commands List---");
						Console.ForegroundColor = ConsoleColor.White;
					}
					if (input.StartsWith ("/me"))
					{
						string[] split = input.Split (new char[] { ' ' }, 2);
						if (split.Length > 1)
							client.SendAction (split [1], client.Channels [0].Name);
						else
							Console.WriteLine ("Nothing to action!");
					}
                    else if(input.StartsWith("/setnickservpass"))
                    {
                        string[] split = input.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            string encrypted = encrypter.EncryptToString(split[1]);
                            ProgramSettings.settings.NickServPass = encrypted;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Password set!");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Use /identify to identify now!");
                        }
                        else
                            Console.WriteLine("Can't have a blank password!");
                    }
                    else if(input.StartsWith("/register"))
                    {
                        // /register <email> <pass>
                        string[] split = input.Split(new char[] { ' ' }, 3);
                        if (split.Length > 2)
                        {
                            client.SendRawMessage("PRIVMSG NickServ :REGISTER {0} {1}", split[1], split[2]);

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("If this was successful, please do a few things.");
                            Console.WriteLine("1. Confirm your registration email, you may need to use the /msg command from here");
                            Console.WriteLine("2. Set your NickServ pass via the /setnickservpass <password> command.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                            Console.WriteLine("Syntax: /register <email> <pass>");
                    }
                    else if (input.StartsWith("/identify"))
                    {
                        if (ProgramSettings.settings.NickServPass.Trim() == "")
                        {
                            Console.WriteLine("You have no password set!");
                            break;
                        }
                        else
                        {
                            string decrypted = encrypter.DecryptString(ProgramSettings.settings.NickServPass);
                            client.SendRawMessage("PRIVMSG NickServ :IDENTIFY {0}", decrypted);

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Attempted authentication!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else if (input.StartsWith("/msg"))
                    {
                        //0 command, 1 user, 2 msg
                        string[] split = input.Split(new char[] { ' ' }, 3);
                        if (split.Length > 2)
                            client.SendRawMessage("PRIVMSG {0} :{1}", split[1], split[2]);
                        else
                            Console.WriteLine("No message!");
                    }
                    else if (input.StartsWith("/exit"))
                    {
                        ExitSafely();
                    }
                    else if(input.StartsWith("/changeprefix"))
                    {
                        string[] split = input.Split(new char[] { ' ' }, 2);
                        if (split.Length > 1)
                        {
                            if(split[1].Length > 1)
                            {
                                Console.WriteLine("Only single characters are accepted here!");
                            }
                            else
                            {
                                try
                                {
                                    ProgramSettings.settings.CommandPrefix = char.Parse(split[1]);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Command prefix changed to '{0}'!", ProgramSettings.settings.CommandPrefix);
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                catch(Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("UH OH BIG ERROR\nUH OH BIG ERROR");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine(ex.Message + "\n");
                                }
                            }
                        }
                        else
                            Console.WriteLine("No character given!");
                    }
                    else if (input.StartsWith("/enablewelcome"))
                    {
                        ProgramSettings.settings.WelcomeUserEnabled = true;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Disabling slap.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (input.StartsWith("/disablewelcome"))
                    {
                        ProgramSettings.settings.WelcomeUserEnabled = false;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Disabling welcome.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (input.StartsWith("/disableslap"))
                    {
                        ProgramSettings.settings.SlapEnabled = false;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Disabling slap.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (input.StartsWith("/enableslap"))
                    {
                        ProgramSettings.settings.SlapEnabled = true;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Enabling slap");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (input.StartsWith("/enable8ball") || input.StartsWith("/enableeightball"))
                    {
                        ProgramSettings.settings.EightballEnabled = true;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Enabling Eight Ball");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (input.StartsWith("/disable8ball") || input.StartsWith("disableeightball"))
                    {
                        ProgramSettings.settings.EightballEnabled = false;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Disabling Eight Ball");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
				} else
				{
					if (client.Channels.Count () > 0)
						client.SendRawMessage (String.Format ("PRIVMSG {0} :{1}", client.Channels [0].Name, input));
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write ("ERROR: ");
						Console.ForegroundColor = ConsoleColor.White;
						Console.Write ("Not in a channel.\n");
					}
				}
			}
		}

		private static void UpdateUsersList ()
		{
			bool done = false;
			while (!done)
			{
				UsersList.Clear ();
                
				if (client != null)
				{
					if (client.Channels.Count () > 0)
					{
						IrcChannel channel = client.Channels [0];
						foreach (var user in channel.Users)
						{
							UsersList.Add (new IrcUserAndSeen ((IrcUser)user));
						}
					}
				}
				Thread.Sleep (3 * 1000); //every 3 seconds, update the users list
			}
		}

		private static void RunBotTest ()
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine ("Connecting...");
			Console.ForegroundColor = ConsoleColor.White;

			client = new IrcClient (ProgramSettings.settings.LastJoinedServer, new IrcUser (ProgramSettings.settings.LastUsedNick, "luigibot-oss"));
			client.ConnectionComplete += (s, e) =>
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine ("Connected! Joining {0}..", ProgramSettings.settings.LastJoinedChannel);
				Console.ForegroundColor = ConsoleColor.White;

				Console.ForegroundColor = ConsoleColor.Green;
				client.JoinChannel (ProgramSettings.settings.LastJoinedChannel);
				Console.WriteLine ("Connected!");
				Console.ForegroundColor = ConsoleColor.White;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Attempting NickServ authentication..");
                Console.ForegroundColor = ConsoleColor.White;
                if (ProgramSettings.settings.NickServPass.Trim() != "")
                {
                    client.SendRawMessage("PRIVMSG NickServ :IDENTIFY {0}", encrypter.DecryptString(ProgramSettings.settings.NickServPass));
                }
                else
                    Console.WriteLine("No NickServ password set! Set using /identify");

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine ("Loading database..");
				Console.ForegroundColor = ConsoleColor.White;
				UsersSeenDatabase.LoadDatabase (client);

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine ("Success!");
				Console.ForegroundColor = ConsoleColor.White;

				updateUsersListThread.Start ();
			};
			client.UserPartedChannel += (s, e) =>
			{
				DateTime now = DateTime.Now;
				foreach (IrcUserAndSeen u in UsersSeenDatabase.UsersSeenDatabase)
				{
					if (u.User == e.User)
						UsersSeenDatabase.UsersSeenDatabase.Remove (u);
				}
				UsersSeenDatabase.UsersSeenDatabase.Add (new IrcUserAndSeen (e.User, now));

				//Console.Beep(4400, 1000);
				Console.WriteLine ("Added new user to database {0} left at {1}", e.User.Nick, now.ToString ());
			};
			client.UserJoinedChannel += (s, e) =>
			{
				if (e.User.Nick.ToLower () == "luigifan2010")
				{
					client.SendAction ("welcomes daddy!", client.Channels [0].Name);
					return;
				} 
				if (ProgramSettings.settings.WelcomeUserEnabled)
				{
                    if (e.User.Nick != client.User.Nick)
                    {
                        if (ProgramSettings.settings.WelcomeMessage.Contains("/me"))
                        {
                            //set substring 4 to account for "/me" and the space after
                            client.SendAction(String.Format(ProgramSettings.settings.WelcomeMessage.Substring(4), e.User.Nick), client.Channels[0].Name);
                        }
                        else
                        {
                            client.SendRawMessage("PRIVMSG {0} :{1}",
                                client.Channels[0].Name,
                                String.Format(ProgramSettings.settings.WelcomeMessage, e.User.Nick));
                        }
                    }
				}
			};
			client.NoticeRecieved += (s, e) =>
			{
				Console.WriteLine ("NOTICE FROM {0}: {1}", e.Source, e.Notice.ToString ());
			};
            bool checkForNextHighfive = false;
            string firstuser = "";
			client.ChannelMessageRecieved += (s, e) =>
			{
                if (checkForNextHighfive)
                {
                    if (e.PrivateMessage.Message == "\\o")
                    {
                        if(e.PrivateMessage.User.Nick != firstuser)
                            client.SendRawMessage("PRIVMSG {0} :{1} o/ *HIGHFIVED* \\o {2}!", client.Channels[0].Name, firstuser, e.PrivateMessage.User.Nick);
                    }
                    checkForNextHighfive = false;
                }

				var channel = client.Channels [e.PrivateMessage.Source];
				Console.WriteLine ("{0} - {1}: {2}", e.PrivateMessage.Source, e.PrivateMessage.User, e.PrivateMessage.Message); //just output the stuff boss

                if (e.PrivateMessage.Message.StartsWith(ProgramSettings.settings.CommandPrefix.ToString()))
                {
                    InterpretCommand(e.PrivateMessage.Message, e.PrivateMessage.User, client);
                }
                else if (e.PrivateMessage.Message.StartsWith(client.User.Nick))
                {
                    if (e.PrivateMessage.Message.Contains("help"))
                    {
                        client.SendRawMessage("PRIVMSG {0} :The current command prefix is: '{1}'. You can also find a list of my commands here: https://github.com/Luigifan/Luigibot/wiki/Luigibot-Commands", 
                            client.Channels[0].Name, ProgramSettings.settings.CommandPrefix);
                    }
                }
                else if (e.PrivateMessage.Message == "o/")
                {
                    checkForNextHighfive = true;
                    firstuser = e.PrivateMessage.User.Nick;
                }
                
			};
            client.ModeChanged += (s, e) =>
            {
                if (e.User.Nick == client.User.Nick)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Mode changed, added: " + e.Change);
                    //string[] split = e.Change.Split(new char[] { ' ' }, 2);
                    //client.User.Mode += split[0].Trim('+');
                    Console.WriteLine("New mode: " + client.User.Mode);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            };
			client.RawMessageRecieved += (s, e) =>
			{
				//:Luigifan2010!~mike@107-145-175-125.res.bhn.net QUIT :Client Quit
				string rawMessage = e.Message.ToString ();
				string[] split = rawMessage.Split (new char[] { ' ' }, 3);
				if (split.Length > 0)
				{
					if (split [1] == "QUIT")
					{
						string[] username = split [0].Split (new char[] { '!' }, 2);
						IrcUser ee = new IrcUser (username [0].ToString ().Trim (':'), username [1].ToString ().Trim ('~'));
						DateTime now = DateTime.Now;
						foreach (IrcUserAndSeen u in UsersSeenDatabase.UsersSeenDatabase)
						{
							if (u.User == ee)
								UsersSeenDatabase.UsersSeenDatabase.Remove (u);
						}
						UsersSeenDatabase.UsersSeenDatabase.Add (new IrcUserAndSeen (ee, now));
						Console.WriteLine ("Added new user to database {0} left at {1} (Quit)", ee.Nick, now.ToString ());
					}
				}

			};
			client.ConnectAsync ();

			while (true)
				; //just keeps everything going
		}

		private static void ExitSafely ()
		{
			client.Quit ("Shutting down safely!");
			UsersSeenDatabase.WriteDatabase ();
			ProgramSettings.WriteSettings ();

			Environment.Exit (0);
		}

		/// <summary>
		/// Interprets commands
		/// </summary>
		/// <param name="command">The full message</param>
		/// <param name="sender">The full sender</param>
		/// <param name="recipient">The full recipient</param>
		private static void InterpretCommand (string command, IrcUser sender, IrcClient client)
		{
            command = command.TrimStart(ProgramSettings.settings.CommandPrefix); //simply removes the prefix
            
			if (command.StartsWith ("slap"))
			{
				if (ProgramSettings.settings.SlapEnabled)
				{
					string[] splitCommand = command.Split (new char[] { ' ' }, 2);
					if (splitCommand.Length > 1)
					{
						var listCopy = UsersList;
						bool foundUser = false;
						try
						{
							foreach (IrcUserAndSeen user in listCopy)
							{
								if (user.User.Nick.ToLower () == splitCommand [1].ToLower ().Trim ())
								{
									if (user.User.Nick == client.User.Nick)
										client.SendRawMessage ("PRIVMSG {0} :What'd I do? :(", client.Channels [0].Name);
									else
										client.SendRawMessage ("PRIVMSG {0} :" + "\x01" + "ACTION slaps {1} with a giant fish.\x01", client.Channels [0].Name, user.User.Nick);
									foundUser = true;
									break;
								}
							}
							if (!foundUser)
								client.SendRawMessage ("PRIVMSG {0} :User not found!");
						} 
                        catch{}
					} else
						client.SendRawMessage ("PRIVMSG {0} :Slap who?", client.Channels [0].Name);
				}
			}
            if(command.StartsWith("version"))
            {
                client.SendRawMessage("PRIVMSG {0} :Luigibot v{1} - http://www.github.com/Luigifan/Luigibot", client.Channels[0].Name, ProgramVersion.ToString());
            }
            if(command.StartsWith("changeprefix"))
            {
                string[] splitCommand = command.Split(new char[] { ' ' }, 2);
                if (splitCommand.Length > 1)
                {
                    foreach (string user in ProgramSettings.settings.UsersAllowedToDisable)
                    {
                        if(user.ToLower() == sender.Nick.ToLower())
                        {
                            if (splitCommand[1].Length == 1)
                            {
                                if (splitCommand[1] == "/")
                                {
                                    client.SendRawMessage("PRIVMSG {0} :oi u cheeky cunt ill bash ur ead in i swear on me mum", client.Channels[0].Name);
                                    return;
                                }
                                try
                                {
                                    char newPrefix = char.Parse(splitCommand[1]);
                                    ProgramSettings.settings.CommandPrefix = newPrefix;

                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Command prefix changed to '{0}' successfully!", newPrefix.ToString());
                                    Console.ForegroundColor = ConsoleColor.White;

                                    client.SendRawMessage("PRIVMSG {0} :Command prefix changed to '{1}' successfully!", client.Channels[0].Name, newPrefix.ToString());
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    client.SendRawMessage("PRIVMSG {0} :Uh-oh! Luigibot encountered an error. Email Luigifan @ miketheripper1@gmail.com",
                                    client.Channels[0].Name);

                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("UH OH BIG ERROR\nUH OH BIG ERROR");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine(ex.Message + "\n");
                                }
                            }
                            else
                            {
                                client.SendRawMessage("PRIVMSG {0} :The prefix must be a single character!", client.Channels[0].Name);
                            }
                        }
                    }
                }
                else
                    client.SendRawMessage("PRIVMSG {0} :What character will prefix my commands?", client.Channels[0].Name);
            }
            if(command.StartsWith("kick"))
            {
                //0: command
                //1: user
                //2: reason
                string[] splitCommand = command.Split(new char[] { ' ' }, 3);
                foreach (string user in ProgramSettings.settings.UsersAllowedToDisable)
                {
                    if(user.ToLower() == sender.Nick.ToLower())
                    {
                        try
                        {
                            if (splitCommand.Length > 2)
                            {
                                client.KickUser(client.Channels[0].Name, splitCommand[1], splitCommand[2]);
                            }
                            else
                                client.KickUser(client.Channels[0].Name, splitCommand[1]);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR: {0}", ex.Message);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
            if(command.StartsWith("getmodes"))
            {
                client.SendMessage("Modes: " + client.User.Mode, client.Channels[0].Name);
            }
			if (command.StartsWith ("ann"))
			{
				string[] splitCommand = command.Split (new char[]{ ' ' }, 2);
				if (splitCommand.Length > 1)
				{
					foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
					{
						if (sender.Nick.ToLower () == nick)
						{
							if (splitCommand [1] == "test")
							{
								if (ProgramSettings.settings.WelcomeMessage.Contains ("/me"))
								{
									client.SendAction (String.Format (ProgramSettings.settings.WelcomeMessage.Substring (4), sender.Nick), client.Channels [0].Name);
								} else
								{
									client.SendRawMessage ("PRIVMSG {0} :{1}", 
										client.Channels [0].Name, 
										String.Format (ProgramSettings.settings.WelcomeMessage, sender.Nick));
								}
							} 
                            else
							{
                                if(splitCommand[1].Contains("{") && splitCommand[1].Contains("}"))
                                {
                                    string[] tester = splitCommand[1].Split(new char[] { '{', '}' }, 3);
                                    foreach(string x in tester)
                                    {
                                        int xf = -1;
                                        bool parsed = false;
                                        try
                                        {
                                            xf = int.Parse(x.ToString());
                                            parsed = true;
                                        }
                                        catch{parsed = false;}

                                        if (parsed)
                                        {
                                            if (xf > 0 || xf < 0)
                                            {
                                                client.SendRawMessage("PRIVMSG {0} :ERROR: Index numbers can't be bigger than 0 or less than 0. MUST BE 0!", client.Channels[0].Name);
                                                return;
                                            }
                                        }
                                    }
                                }
								ProgramSettings.settings.WelcomeMessage = splitCommand [1];
								client.SendRawMessage ("PRIVMSG {0} :Welcome message set!", client.Channels [0].Name);
								Console.ForegroundColor = ConsoleColor.Yellow;
								Console.WriteLine ("New welcome message set: " + splitCommand [1]);
								Console.ForegroundColor = ConsoleColor.White;
							}
						}
					}
				}
				else
				{
					client.SendRawMessage ("PRIVMSG {0} :What message do I set?", client.Channels[0].Name);
				}
			}
			if (command.StartsWith ("enableslap"))
			{
				foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
				{
					if (sender.Nick.ToLower () == nick)
					{
						ProgramSettings.settings.SlapEnabled = true;

						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine ("Enabling slap");
						Console.ForegroundColor = ConsoleColor.White;

						break;
					}
				}
			}
			if (command.StartsWith ("enablewelcome"))
			{
				foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
				{
					if (sender.Nick.ToLower () == nick)
					{
						ProgramSettings.settings.WelcomeUserEnabled = true;

						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine ("Enabling welcome messages");
						Console.ForegroundColor = ConsoleColor.White;

						break;
					}
				}
			}
			if (command.StartsWith ("disablewelcome"))
			{
				foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
				{
					if (sender.Nick.ToLower () == nick)
					{
						ProgramSettings.settings.WelcomeUserEnabled = false;

						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine ("Disabling welcome messages");
						Console.ForegroundColor = ConsoleColor.White;

						break;
					}
				}
			}
			if (command.StartsWith ("disableslap"))
			{
				foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
				{
					if (sender.Nick.ToLower () == nick)
					{
						ProgramSettings.settings.SlapEnabled = false;

						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine ("Disabling slap");
						Console.ForegroundColor = ConsoleColor.White;

						break;
					}
				}
			}
			if (command.StartsWith ("eightball") || command.StartsWith ("8ball") || command.StartsWith ("fortune"))
			{
				if (ProgramSettings.settings.EightballEnabled)
				{
					int ranMessage = random.Next (EightballMessages.Length - 1);
					if (command.ToLower ().Contains ("waluigibot1337") || command.ToLower ().Contains ("waluigibot") || command.ToLower ().Contains ("waluigi bot"))
						client.SendRawMessage ("PRIVMSG {0} :Waluigibot is a tool and will never come to fruition.", client.Channels [0].Name);
					else if (command.ToLower ().Contains ("bot") && !command.ToLower ().Contains ("luigi"))
						client.SendRawMessage ("PRIVMSG {0} :Your bot is inadequate for the job.", client.Channels [0].Name);
					else
						client.SendRawMessage ("PRIVMSG {0} :{1}", client.Channels [0].Name, EightballMessages [ranMessage]);
				}
			}
			if (command.StartsWith ("status"))
			{
				client.SendRawMessage ("PRIVMSG {0} :Slap Enabled: {1}. Eight Ball Enabled: {2}. Welcome Enabled: {3}. Command Prefix: {4}", 
					client.Channels [0].Name,
					ProgramSettings.settings.SlapEnabled,
					ProgramSettings.settings.EightballEnabled,
					ProgramSettings.settings.WelcomeUserEnabled,
                    ProgramSettings.settings.CommandPrefix.ToString());
			}
			if (command.StartsWith ("lastfm"))
			{
				string[] split = command.Split (new char[] { ' ' }, 2);
				if (split.Length > 1)
				{
                    client.SendRawMessage("PRIVMSG {0} :Sending LastFM request, this may take a few seconds..", client.Channels[0].Name);
					try
					{
						var lastfmClient = new LastfmClient ("4de0532fe30150ee7a553e160fbbe0e0", "0686c5e41f20d2dc80b64958f2df0f0c");
						var response = lastfmClient.User.GetRecentScrobbles (split [1].ToString ().Trim (), null, 0, 1);
						LastTrack lastTrack = response.Result.Content [0];
						client.SendRawMessage ("PRIVMSG {0} :{1} last listened to {2} by {3}.", client.Channels [0].Name, split [1].Trim (), lastTrack.Name, lastTrack.ArtistName);
					} 
                    catch (ArgumentOutOfRangeException iex)
					{
						client.SendRawMessage ("PRIVMSG {0} :That user doesn't exist or hasn't scrobbled anything yet!", 
							client.Channels [0].Name);
					} 
                    catch (Exception ex)
					{
						client.SendRawMessage ("PRIVMSG {0} :Uh-oh! Luigibot encountered an error. Email Luigifan @ miketheripper1@gmail.com",
							client.Channels [0].Name);

						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine ("UH OH BIG ERROR\nUH OH BIG ERROR");
						Console.ForegroundColor = ConsoleColor.White;
						Console.WriteLine (ex.Message + "\n");
					}
				}
			}
			if (command.StartsWith ("enable8ball") || command.StartsWith ("enableeightball"))
			{
				foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
				{
					if (sender.Nick.ToLower () == nick)
					{
						ProgramSettings.settings.EightballEnabled = true;

						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine ("Enabling eight ball");
						Console.ForegroundColor = ConsoleColor.White;

						break;
					}
				}
			}
			if (command.StartsWith ("disable8ball") || command.StartsWith ("disableeightball"))
			{
				foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
				{
					if (sender.Nick.ToLower () == nick)
					{
						ProgramSettings.settings.EightballEnabled = false;

						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine ("Disabling eight ball");
						Console.ForegroundColor = ConsoleColor.White;

						break;
					}
				}
			}
			if (command.StartsWith ("selfdestruct"))
			{
				foreach (var nick in ProgramSettings.settings.UsersAllowedToDisable)
				{
					if (sender.Nick.ToLower () == nick)
					{
						ExitSafely ();
						break;
					}
				}
			}
			if (command.StartsWith ("42"))
			{
				client.SendRawMessage ("PRIVMSG {0} :The Answer to Life, the Universe, and Everything.", client.Channels [0].Name);
			}
			if (command.StartsWith ("commands"))
			{
				client.SendRawMessage ("PRIVMSG {0} :You can find a list of my commands here: https://github.com/Luigifan/Luigibot/wiki/Luigibot-Commands", client.Channels [0].Name);
			}
			if (command.StartsWith ("seen"))
			{
				string[] splitCommand = command.Split (new char[] { ' ' }, 2);
				if (splitCommand.Length > 1)
				{
					if (splitCommand [1].ToLower () == "knux" || splitCommand [1].ToLower () == "knuckles" || splitCommand [1].ToLower () == "knuckles96")
					{
						client.SendRawMessage ("PRIVMSG {0} :Never.", client.Channels [0].Name);
						return;
					}
					var UsersListCopy = UsersList;
					var UserDatabaseCopy = UsersSeenDatabase.UsersSeenDatabase;
					//First, check to see if the user is on now
					bool foundInOnline = false;
					foreach (IrcUserAndSeen user in UsersListCopy)
					{
						if (user.User.Nick.ToLower () == splitCommand [1].ToLower ().Trim ())
						{
							foundInOnline = true;
							if (user.User.Nick == client.User.Nick)
								client.SendRawMessage ("PRIVMSG {0} :I'm always online c:", client.Channels [0].Name);
							else
								client.SendRawMessage ("PRIVMSG {0} :That user is online now!", client.Channels [0].Name);
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
                            if (user.User.Nick == null)
                            {
                                UsersSeenDatabase.UsersSeenDatabase.Remove(user);
                            }
                            else
                            {
                                if (user.User.Nick.ToLower() == splitCommand[1].ToLower().Trim())
                                {
                                    foundInDatabase = true;
                                    client.SendRawMessage("PRIVMSG {0} :{1} was last seen at {2} (EST)", client.Channels[0].Name, user.User.Nick, user.LastSeen.ToString());
                                    break;
                                }
                            }
                        }
						if (!foundInDatabase && !foundInOnline)
							client.SendRawMessage ("PRIVMSG {0} :I'm not sure :(", client.Channels [0].Name);
					}
				} else
					client.SendRawMessage ("PRIVMSG {0} :Last seen who?", client.Channels [0].Name);
			}
		}

	}
}
