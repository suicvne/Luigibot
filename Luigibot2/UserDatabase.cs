using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using ChatSharp;

namespace Luigibot2
{
	public class UserDatabase
	{
        public List<IrcUserAndSeen> UsersSeenDatabase {get;set;}
		public UserDatabase()
		{
            UsersSeenDatabase = new List<IrcUserAndSeen>();
		}

        /// <summary>
        /// Be sure that this is only called AFTER you join a channel!
        /// </summary>
        /// <param name="path"></param>
		public void LoadDatabase(IrcClient client)
		{
            JsonSerializer js = new JsonSerializer();

            using(StreamReader sr = new StreamReader(Environment.CurrentDirectory + @"\seen_database.json"))
            {
                using(JsonReader jsr = new JsonTextReader(sr))
                {
                    IrcUserAndSeen des = new IrcUserAndSeen(new ChatSharp.IrcUser("test", "testt"), DateTime.Now); //filler
                    while(des != null)
                    {
                        des = js.Deserialize<IrcUserAndSeen>(jsr);
                        Console.WriteLine("Adding {0} to database, last seen at {1}", des.User.Nick, des.LastSeen.ToString());
                        UsersSeenDatabase.Add(des);
                        sr.ReadLine();
                    }
                }
            }
		}

		public void WriteDatabase()
		{
            JsonSerializer js = new JsonSerializer();

			using(StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\seen_database.json"))
            {
                using(JsonWriter jsw = new JsonTextWriter(sw))
                {
                    foreach(IrcUserAndSeen u in UsersSeenDatabase)
                    {
                        js.Serialize(jsw, u);
                    }
                }
            }

		}

	}
}

