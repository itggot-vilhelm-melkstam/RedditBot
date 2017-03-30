using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace RedditBot
{
    class RedditBot
    {
        string _clientName;
        RedditAccessToken token = new RedditAccessToken();

        TokenBucket throttler = new TokenBucket(30, 60);
        HttpClient client = new HttpClient();

        string username;

        public RedditBot(string clientName)
        {
            _clientName = clientName;
        }

        public RedditAccessToken Authenticate(string redditUsername, string redditPassword, string clientId, string clientSecret)
        {
            username = redditUsername;
               
            var authenticationArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            var encodedAuthenticationString = Convert.ToBase64String(authenticationArray);

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedAuthenticationString);

            client.DefaultRequestHeaders.Add("User-Agent", $"{_clientName} by {redditUsername}");

            var formData = new Dictionary<string, string>{
                { "grant_type", "password" },
                { "username", redditUsername },
                { "password", redditPassword }
            };

            var encodedFormData = new FormUrlEncodedContent(formData);
            var authUrl = "https://www.reddit.com/api/v1/access_token";
            var response = client.PostAsync(authUrl, encodedFormData).GetAwaiter().GetResult();

            // Response Code
            Console.WriteLine(response.StatusCode);

            // Actual Token
            var responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var accessToken = JObject.Parse(responseData).SelectToken("access_token").ToString();

            // Update AuthorizationHeader
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

            responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine(responseData);

            RedditAccessToken token = new RedditAccessToken(responseData);

            Console.WriteLine("-- Authentication OK! --");
            Console.WriteLine("\n\n");

            return token;
           
        }


        private void Sleep()
        {
            var sleeptime = throttler.MillisecondsTilRefill() + 10;

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Going in to sleep for {sleeptime / 1000.0} seconds");
            Console.ResetColor();
            System.Threading.Thread.Sleep(sleeptime);
        }

        public async Task<HttpResponseMessage> GetRedditRequestAsync(string method)
        {
            if (throttler.RequestIsAllowed())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("OK: ");
                Console.ResetColor();
                Console.WriteLine(method);
                return await client.GetAsync("https://oauth.reddit.com" + method);
            }
            else
            {
                Sleep();
                return await GetRedditRequestAsync(method);
            }
        }

        public async Task<HttpResponseMessage> GetRequestAsync(string method)
        {
            if (throttler.RequestIsAllowed())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("OK: ");
                Console.ResetColor();
                Console.WriteLine(method);
                return await client.GetAsync(method);
            }
            else
            {
                Sleep();
                return await GetRequestAsync(method);
            }
        }

        public async Task<HttpResponseMessage> PostRedditRequestAsync(string method, HttpContent content)
        {
            if (throttler.RequestIsAllowed())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("OK: ");
                Console.ResetColor();
                Console.WriteLine(method);
                return await client.PostAsync("https://oauth.reddit.com" + method, content);
            }
            else
            {
                Sleep();
                return await PostRedditRequestAsync(method, content);
            }
        }


        public async Task<HttpResponseMessage> PostCommentWikipediaSummaryAsync(JToken post)
        {
            string summary = GetWikipediaSummaryFromPostAsync(post).GetAwaiter().GetResult();
            
            if (summary != "")
            {
                string text = "Summary from: " + post.SelectToken("data.url").ToString() + "\n\n>" + summary + "\n\n --- \n\n *I am a bot and not responsible for what is written in the article*";

                var formData = new Dictionary<string, string>{
                    { "api_type", "json" },
                    { "text", text },
                    { "thing_id",  post.SelectToken("data.name").ToString()}
                };

                var encodedFormData = new FormUrlEncodedContent(formData);


                Console.WriteLine("Posted comment on: https://reddit.com" + post.SelectToken("data.permalink"));

                var response = await PostRedditRequestAsync("/api/comment", encodedFormData);

                return response;
            }
            else
            {
                return new HttpResponseMessage();
            }
            
        }

        public async Task<JArray> GetWikipediaLinksFromSubredditAsync(string subreddit)
        {
            var response = await GetRedditRequestAsync($"/r/{subreddit}/new.json?sort=new&limit=10");

            if (response.StatusCode.ToString() == "OK")
            {
                var data = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).SelectToken("data");
                var links = new JArray();
                var posts = JArray.Parse(JObject.Parse(data.ToString()).SelectToken("children").ToString());

                foreach (var post in posts.Children())
                {
                    if (Regex.Match(post.SelectToken("data.domain").ToString(), @"[a-z -]+.wikipedia.org").Success)
                    {
                        links.Add(post);
                    }
                }
                return links;
            }else
            {
                return new JArray();
            }
        }

        public async Task<JArray> GetWikipediaLinksFromDomainAsync(string domain)
        {
            var response = await GetRedditRequestAsync($"/domain/{domain}/new.json?sort=new&limit=100");

            if (response.StatusCode.ToString() == "OK")
            {
                var data = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).SelectToken("data");
                var links = new JArray();
                var posts = JArray.Parse(JObject.Parse(data.ToString()).SelectToken("children").ToString());

                foreach (var post in posts.Children())
                {
                    if (Regex.Match(post.SelectToken("data.domain").ToString(), @"[a-z -]+.wikipedia.org").Success)
                    {
                        links.Add(post);
                    }
                }
                return links;
            }
            else
            {
                return new JArray();
            }
        }

        public async Task<JArray> GetCommentsFromPostAsync(JToken post)
        {
            string permalink = post.SelectToken("data.permalink").ToString();

            var response = await GetRedditRequestAsync($"/{permalink}/.json");

            if (response.StatusCode.ToString() == "OK")
            {
                try
                {
                    var data = JArray.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    data.Remove(data.First);
                    var comments = JArray.Parse(data.First.SelectToken("data.children").ToString());
                    return comments;
                }
                catch { return new JArray(); }
            }
            else
            {
                return new JArray();
            }
        }

        public async Task<bool> HasCommentedOnPostAsync(JToken post)
        {
            var comments = await GetCommentsFromPostAsync(post);
            foreach (var comment in comments)
            {
                if (comment.SelectToken("data.author").ToString() == username)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<string> GetWikipediaSummaryFromPostAsync(JToken post)
        {
            var domain = post.SelectToken("data.domain").ToString();
            var splittedURL = post.SelectToken("data.url").ToString().Split('/');
            var page = splittedURL[splittedURL.Length -1].Replace("_", "%20");

            var response = await GetRequestAsync("https://" + domain + "/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles=" + page);

            if (response.StatusCode.ToString() == "OK")
            {
                try
                {
                    var summary = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).SelectToken("query.pages.*.extract", true).ToString();
                    return summary;
                }
                catch
                {
                    return "";
                }
                
            }
            else
            {
                return "";
            }
        }

    }
}
