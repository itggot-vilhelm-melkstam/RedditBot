using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditBot
{
    class TokenBucket
    {
        static public int currentTokens, capacity, interval;
        static public DateTime lastRefreshed;

        public TokenBucket(int bucketCapacity, int intervalInSeconds)
        {
            currentTokens = bucketCapacity;
            capacity = bucketCapacity;
            interval = intervalInSeconds;
            lastRefreshed = DateTime.Now;
        }

        public bool RequestIsAllowed()
        {
            Refill();
            if (currentTokens >= 1)
            {
                currentTokens = currentTokens - 1;
                return true;
            }
            return false;
        }

        static public bool Refill()
        {
            if (DateTime.Now.Subtract(lastRefreshed).TotalSeconds >= interval)
            {
                currentTokens = capacity;
                lastRefreshed = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}