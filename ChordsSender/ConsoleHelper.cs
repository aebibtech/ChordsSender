using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsSender
{
    public class ConsoleHelper
    {
        public static void ShowMessage(string message = "")
        {
            Console.Clear();
            Console.WriteLine("Chords Sheet Sender");
            Console.WriteLine($"(c) {DateTime.Now.Year} Aebibtech\n");
            Console.WriteLine(message);
        }
    }
}
