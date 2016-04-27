using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuigibotCommon
{
    public static class IO
    {
        public static void Log(string integration, ConsoleColor color, string log)
        {
            Console.ForegroundColor = color;
            Console.Write($"[{integration}]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(log);
        }

        public static void LogError(string error)
        {
            Log("Error", ConsoleColor.Red, error);
        }
    }
}
