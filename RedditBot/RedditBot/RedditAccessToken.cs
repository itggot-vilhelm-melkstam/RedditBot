using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditBot
{
    class RedditAccessToken
    {
        public string AccessToken { get; private set; }
        public string tokenType;
        public string scope;
        public DateTime createdAt;
        public int expiresInSeconds;

        public RedditAccessToken()
        {
       
        }

        public RedditAccessToken(string tokenAsString)
        {
            createdAt = DateTime.Now;

            AccessToken = JObject.Parse(tokenAsString).SelectToken("access_token").ToString();
            tokenType = JObject.Parse(tokenAsString).SelectToken("token_type").ToString();
            scope = JObject.Parse(tokenAsString).SelectToken("scope").ToString();
            expiresInSeconds = Convert.ToInt32(JObject.Parse(tokenAsString).SelectToken("expires_in").ToString());
        }

        public int TimeLeft()
        {
            return expiresInSeconds - Convert.ToInt32(DateTime.Now.Subtract(createdAt).TotalSeconds);
        }
    }
}
