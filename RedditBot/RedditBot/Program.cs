using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var redditBot = new RedditBot("anna_bot");

            redditBot.Authenticate("itggot-vilhelm", "6wm1MJBmHD7J", "56QyJLn_eRVwpA", "cEmNQOeLCfX4J5OawP5DfWGRr54");

            Console.ReadKey();
        }
    }
}
