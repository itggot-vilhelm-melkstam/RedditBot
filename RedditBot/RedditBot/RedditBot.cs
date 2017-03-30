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

        public RedditBot(string clientName)
        {
            _clientName = clientName;
        }

        public RedditAccessToken Authenticate(string redditUsername, string redditPassword, string clientId, string clientSecret)
        {
            
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


        public async Task<HttpResponseMessage> GetRequestAsync(string method)
        {
            if (throttler.RequestIsAllowed())
            {
                Console.WriteLine($"OK: {method}");
                return await client.GetAsync(method);
            }
            else
            {
                Console.WriteLine($"Not accepted: {method}");
                return new HttpResponseMessage(new System.Net.HttpStatusCode());
            }
        }

        public async Task<HttpResponseMessage> PostRequestAsync(string method, HttpContent content)
        {
            if (throttler.RequestIsAllowed())
            {
                Console.WriteLine($"OK: {method}");
                return await client.PostAsync(method, content);
            }
            else
            {
                Console.WriteLine($"Not accepted: {method}");
                return new HttpResponseMessage(new System.Net.HttpStatusCode());
            }
        }


        public async Task<HttpResponseMessage> PostCommentWikipediaSummaryAsync(JToken post)
        {

            var formData = new Dictionary<string, string>{
                { "api_type", "json" },
                { "text", "this is a test" },
                { "thing_id",  post.SelectToken("data.name").ToString()}
            };

            var encodedFormData = new FormUrlEncodedContent(formData);


            Console.WriteLine("Posted comment on: https://reddit.com" + post.SelectToken("data.permalink"));

            var response = await PostRequestAsync("https://oauth.reddit.com/api/comment", encodedFormData);

            return response;
        }

        public async Task<JArray> GetUnansweredWikipediaLinksFromSubredditAsync(string subreddit)
        {
            var response = await GetRequestAsync($"https://oauth.reddit.com/r/{subreddit}/new.json?sort=new&limit=100");

            if (response.StatusCode.ToString() == "OK")
            {
                var data = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).SelectToken("data");
                var links = new JArray();
                var posts = JArray.Parse(JObject.Parse(data.ToString()).SelectToken("children").ToString());

                foreach (var post in posts.Children())
                {
                    if ((Regex.Match(post.SelectToken("data.domain").ToString(), @"[a-z -]+.wikipedia.org").Success) && (HasCommentedOnPostAsync(post, _clientName).GetAwaiter().GetResult() == false))
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
        
        public async Task<JArray> GetCommentsFromPostAsync(JToken post)
        {
            string permalink = post.SelectToken("data.permalink").ToString();

            var response = await GetRequestAsync($"https://oauth.reddit.com/{permalink}/.json");
            if (response.StatusCode.ToString() == "OK")
            {
                var data = JArray.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                data.Remove(data.First);
                var comments = JArray.Parse(data.First.SelectToken("data.children").ToString());
                return comments;
            }
            else
            {
                return new JArray();
            }
        }

        public async Task<bool> HasCommentedOnPostAsync(JToken post, string username)
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

    }
}
