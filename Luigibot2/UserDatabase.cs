using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Luigibot2
{
	public class UserDatabase
	{
		List<IrcUserAndSeen> UserDatabase List<IrcUserAndSeen>();

		public UserDatabase()
		{
		}

		public void LoadDatabase(string path)
		{

		}

		public void WriteDatabase(string path)
		{
			MemoryStream ms = new MemoryStream ();
			JsonSerializer js = new JsonSerializer ();
			BsonWriter bWriter = new BsonWriter (ms);

			foreach (var user in UserDatabase) 
			{
				js.Serialize (bWriter, IrcUserAndSeen);
			}

		}

	}
}

