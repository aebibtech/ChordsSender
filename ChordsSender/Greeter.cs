using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsSender
{
    public class Greeter
    {
        public string GetGreeting(int hour)
        {
            var greeting = "Good {} po team. Ito po ang ating chords para po ngayong Linggo. See you all! :)";
            if (hour >= 0 && hour <= 11)
            {
                greeting = greeting.Replace("{}", "morning");
            }
            if (hour >= 12 && hour <= 17)
            {
                greeting = greeting.Replace("{}", "afternoon");
            }
            if (hour >= 18 && hour <= 23)
            {
                greeting = greeting.Replace("{}", "evening");
            }
            return greeting;
        }
    }
}
