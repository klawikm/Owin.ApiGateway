namespace Owin.ApiGateway.Cache
{
    using System;
    using System.Runtime.Caching;

    public class MemoryCacheProvider : ICache
    {
        private readonly MemoryCache cache;

        public MemoryCacheProvider()
        {
            this.cache = MemoryCache.Default;
        }

        public object GetFromCache(string cacheId)
        {
            if (this.cache.Contains(cacheId))
            {
                return this.cache[cacheId];
            }

            return null;
        }

        public void RemoveFromCache(string cacheId)
        {
            if (this.cache.Contains(cacheId))
            {
                this.cache.Remove(cacheId);
            }
        }

        public void SetInCache(string cacheId, object cacheObject, TimeSpan slidingExpiration)
        {
            this.cache.Set(cacheId, cacheObject, new DateTimeOffset(DateTime.Now.Add(slidingExpiration)));
        }
    }
}
