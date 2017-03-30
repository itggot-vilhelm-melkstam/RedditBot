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

        public int MillisecondsTilRefill()
        {
            if (DateTime.Now.Subtract(lastRefreshed).TotalSeconds >= interval)
            {
                return 0;
            }
            else
            {
                return (interval * 1000) - Convert.ToInt32(DateTime.Now.Subtract(lastRefreshed).TotalMilliseconds);
            }
        }
    }
}