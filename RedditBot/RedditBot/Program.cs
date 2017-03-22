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
            var redditBot = new RedditBot("anna_bot");

            redditBot.Authenticate("itggot-vilhelm", "6wm1MJBmHD7J", "56QyJLn_eRVwpA", "cEmNQOeLCfX4J5OawP5DfWGRr54");


            var posts = redditBot.GetPostsFromSubreddit("sandboxtest");
            var post = JObject.Parse(posts).SelectToken("children[0]").ToString();
            Console.WriteLine(post);

            redditBot.GetCommentsFromPost(post);

            Console.ReadKey();
        }
    }
}
