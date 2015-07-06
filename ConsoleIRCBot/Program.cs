using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ConsoleIRCBot
{
    class Program
    {
        static IRC cIRC = new IRC("Luigibot2015", "#luigibot");
        static Thread inputThread = new Thread(listenForInput);
        static Thread updateNamesListThread = new Thread(UpdateUserList);

        static void Main(string[] args)
        {
            inputThread.Start();
            updateNamesListThread.Start(); //starting threads

            cIRC.IrcRealName = "Luigibot";
            cIRC.IrcUser = "luigibott";
            cIRC.IsInvisble = false;
            cIRC.eventReceiving += (IrcCommandReceived);
            cIRC.eventJoin += cIRC_eventJoin;
            cIRC.Connect("irc.stardustfields.net", int.Parse("6667"));
        }

        static void UpdateUserList()
        {
            bool done = false;
            while(!done)
            {
                if (cIRC.IrcWriter != null)
                {
                    cIRC.IrcWriter.WriteLine("NAMES {0}", cIRC.IrcChannel);
                    cIRC.IrcWriter.Flush();
                }
                Thread.Sleep(10 * 1000); //update names every 10 seconds
            }
        }

        static void listenForInput()
        {
            bool done = false;
            while (!done)
            {
                string command = Console.ReadLine();

                if (command.Contains("/join"))
                {
                    String[] split = command.Split(new char[] { ' ' }, 2);
                    if (!split[1].Contains("#"))
                    {
                        //TODO: Error
                    }
                    else
                    {
                        cIRC.IrcChannel = split[1];
                        cIRC.IrcWriter.WriteLine(String.Format("JOIN {0}", split[1]));
                        cIRC.IrcWriter.Flush();
                    }
                }
                else if (command.Contains("/say"))
                {
                    String[] split = command.Split(new char[] { ' ' }, 2);
                    String ccommand = String.Format("PRIVMSG {0} :{1}\r", cIRC.IrcChannel, split[1]);
                    cIRC.IrcWriter.WriteLine(ccommand);
                    cIRC.IrcWriter.Flush();
                }
            }
        }

        static void cIRC_eventJoin(string IrcChannel, string IrcUser)
        {
            Console.WriteLine(String.Format("{0} joins {1}", IrcUser, cIRC.IrcChannel));
            cIRC.IrcWriter.WriteLine(String.Format("NOTICE {0} :Hello {0}, welcome to {1}!", IrcUser, cIRC.IrcChannel));
            cIRC.IrcWriter.Flush();
        }

        static void IrcCommandReceived(string IrcCommand)
        {
            //:Luigifan2010!~mike@fl-71-52-118-194.dhcp.embarqhsd.net PRIVMSG #luigibot :test

            String[] split = IrcCommand.Split(new char[]{' '}, 2);

            if (split[1].Contains("353"))
            { }
            else if (split[1].Contains("366"))
            { /*nothing, these are annoying*/ }
            else if (!split[1].Contains("421") && !split[1].Contains("Unknown command"))
                Console.WriteLine(split[1]);

            String split1 = split[1];
            if(split[1].Contains("PRIVMSG"))
            {
                String[] splitForSlap = split[1].Split(new char[] {':'}, 2);
                if(splitForSlap[1].Contains("!slap"))
                {
                    cIRC.IrcWriter.WriteLine("Slap test");
                    cIRC.IrcWriter.Flush();
                }
            }
        }

    }
}
