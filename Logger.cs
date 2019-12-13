using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace the_game_wpf
{
    public class Logger
    {
        Stopwatch sw = new Stopwatch();
        StringBuilder sb = new StringBuilder();

        public Logger()
        {
            sw.Start();
        }

        public void Write(string text, bool useConsole = true, string suffix = "\n") 
        {
            string str = "[" + sw.ElapsedMilliseconds + "] " + text;
            sb.Append(str + suffix);
            
            if(useConsole)
                Console.WriteLine(str);
        }
    }
}
