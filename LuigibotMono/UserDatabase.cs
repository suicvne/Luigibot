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
            if (File.Exists(Environment.CurrentDirectory + Path.DirectorySeparatorChar +  "seen_database.json"))
            {
                JsonSerializer js = new JsonSerializer();
                js.Formatting = Formatting.Indented;

                using (StreamReader sr = new StreamReader(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "seen_database.json"))
                {
                    using (JsonReader jsr = new JsonTextReader(sr))
                    {
                        List<IrcUserAndSeen> des;
                        des = js.Deserialize<List<IrcUserAndSeen>>(jsr);
                        UsersSeenDatabase = des;
                    }
                }
            }
            else
                Console.WriteLine("No database");
		}

        public void RemoveBrokenEntries()
        {
            IrcUserAndSeen nullUser = new IrcUserAndSeen();
            UsersSeenDatabase.RemoveAll(i=>i==nullUser);
        }

		public void WriteDatabase()
		{
            JsonSerializer js = new JsonSerializer();
            js.Formatting = Formatting.Indented;

            using(StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "seen_database.json"))
            {
                using(JsonWriter jsw = new JsonTextWriter(sw))
                {
                    js.Serialize(jsw, UsersSeenDatabase);
                }
            }

		}

	}
}

