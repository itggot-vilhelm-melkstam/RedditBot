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

        public RedditBot(string clientName)
        {
            _clientName = clientName;
        }

        public RedditAccessToken Authenticate(string redditUsername, string redditPassword, string clientId, string clientSecret)
        {
            using (var client = new HttpClient())
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

                return token;

            }
        }
    }
}
