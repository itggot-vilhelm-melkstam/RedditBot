using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            Console.WriteLine();

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
        


        public String GetPostsFromSubreddit(string subreddit)
        {
            var response = GetRequestAsync($"https://oauth.reddit.com/r/{subreddit}/new.json?sort=new&limit=5").GetAwaiter().GetResult();

            if (response.StatusCode.ToString() == "OK")
            {
                var data = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).SelectToken("data").ToString();
                return data;
            }else
            {
                return "";
            }
        }

        public Array GetCommentsFromPost(string fullname)
        {
            var response = GetRequestAsync($"https://oauth.reddit.com/t3_{fullname}.json").GetAwaiter().GetResult();


            if (response.StatusCode.ToString() == "OK")
            {
                var data = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).SelectToken("data").ToArray();
                Console.WriteLine(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                return data;
            }
            else
            {
                String[] empty = new String[0];
                return empty;
            }
        }


    }
}
