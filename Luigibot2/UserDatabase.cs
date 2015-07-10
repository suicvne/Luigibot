using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;

namespace Luigibot2
{
	public class UserDatabase
	{
        List<IrcUserAndSeen> UsersSeenDatabase;
		public UserDatabase()
		{
            UsersSeenDatabase = new List<IrcUserAndSeen>();
		}

		public void LoadDatabase(string path)
		{

		}

		public void WriteDatabase(string path)
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

