using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace RedditBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var redditBot = new RedditBot("WikiBot");

            redditBot.Authenticate("itggot-vilhelm", "6wm1MJBmHD7J", "56QyJLn_eRVwpA", "cEmNQOeLCfX4J5OawP5DfWGRr54");

            Random rand = new Random();
            string[] wikis = File.ReadAllLines("C:\\DEV\\RedditBot\\RedditBot\\RedditBot\\wikipedia_subdomains.txt").OrderBy(x => rand.Next()).ToArray();

            while (true)
            {
                foreach (var wiki in wikis)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.WriteLine("WIKI: " + wiki);
                    Console.ResetColor();

                    var links = redditBot.GetWikipediaLinksFromDomainAsync(wiki + ".wikipedia.org").GetAwaiter().GetResult();
                    foreach (var link in links)
                    {
                        if (redditBot.HasCommentedOnPostAsync(link).GetAwaiter().GetResult() == false)
                        {
                            redditBot.PostCommentWikipediaSummaryAsync(link).GetAwaiter().GetResult();
                        }

                    }

                    Console.WriteLine("\n");
                }
            }
        }
    }
}
