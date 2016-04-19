using DiscordSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LuigibotDiscord
{
    public class Program
    {
        static DiscordClient test = new DiscordClient();

        public static void InputThread()
        {
            bool accept = true;
            while(accept)
            {
                string input = Console.ReadLine();
                test.SendMessageToChannel(input, "general", "Super Mario Bros X");
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello world!");
            Console.ReadLine();

            Console.WriteLine("Running Discord bot test...");
            test = new DiscordClient();
            test.LoginInformation = new DiscordLoginInformation();
            test.LoginInformation.email[0] = "miketheripper1@gmail.com";
            test.LoginInformation.password[0] = "papabear12";
            Console.WriteLine("Attempting login..");
            //Console.WriteLine("out: {0}", test.SendLoginRequest());
            try
            {
                Thread t = new Thread(InputThread);
                t.Start();
                test.MessageReceived += (sender, e) => 
                {
                    Console.WriteLine("[- Message from {0} in {1} on {2}: {3}", e.username, e.ChannelName, e.ServerName, e.message);
                    if (e.message.StartsWith("?status"))
                        test.SendMessageToChannel("I work ;)", e.ChannelName, e.ServerName);
                };
                test.SendLoginRequest();
                Thread tt = new Thread(test.ConnectAndReadMessages);
                tt.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

    }
}
