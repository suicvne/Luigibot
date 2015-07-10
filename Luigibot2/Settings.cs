using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luigibot2
{
    public class Settings
    {
        public bool EightballEnabled { get; set; }
        public bool SlapEnabled { get; set; }
        public string[] UsersAllowedToDisable { get; set; }

        public Settings()
        {
            EightballEnabled = true;
            SlapEnabled = true;
            UsersAllowedToDisable = new string[] { "luigifan2010", "joey", "aeromatter", "ghosthawk" };
        }

        public void LoadSettings()
        {
            JsonSerializer js = new JsonSerializer();
            using(StreamReader sr = new StreamReader(Environment.CurrentDirectory + @"\settings.json"))
            {
                using(JsonReader jsr = new JsonTextReader(sr))
                {
                    EightballEnabled = js.Deserialize<bool>(jsr);
                    sr.ReadLine();
                    SlapEnabled = js.Deserialize<bool>(jsr);
                    sr.ReadLine();
                    UsersAllowedToDisable = js.Deserialize<string[]>(jsr);
                    sr.ReadLine();
                }
            }
        }

        public void WriteSettings()
        {
            JsonSerializer js = new JsonSerializer();

            using(StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\settings.json"))
            {
                using(JsonWriter jsw = new JsonTextWriter(sw))
                {
                    js.Serialize(jsw, EightballEnabled);
                    js.Serialize(jsw, SlapEnabled);
                    js.Serialize(jsw, UsersAllowedToDisable);
                }
            }

        }

    }
}
