using ChatSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JsonTester
{
    class Program
    {
        static IrcUser test = new IrcUser();

        static void Main(string[] args)
        {

            if(File.Exists(Environment.CurrentDirectory + @"\test.json"))
            {
                //TODO: deserialize
            }

            Console.ReadLine();

        }
    }
}
