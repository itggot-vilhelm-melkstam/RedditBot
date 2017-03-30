using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var redditBot = new RedditBot("WikiBot");

            redditBot.Authenticate("itggot-vilhelm", "6wm1MJBmHD7J", "56QyJLn_eRVwpA", "cEmNQOeLCfX4J5OawP5DfWGRr54");


            var links = redditBot.GetUnansweredWikipediaLinksFromSubredditAsync("sandboxtest").GetAwaiter().GetResult();
            //var post = links.First;

            Console.WriteLine(links.Count);

            //var response = redditBot.PostCommentWikipediaSummaryAsync(post).GetAwaiter().GetResult();

            //Console.WriteLine(response); 

            Console.ReadKey();
        }
    }
}
