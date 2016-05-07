namespace Owin.ApiGateway.Cache
{
    using System;

    public interface ICache
    {
            /// <summary>
            /// The get from cache.
            /// </summary>
            /// <param name="cacheId">
            /// The cache id.
            /// </param>
            /// <returns>
            /// The <see cref="object"/>.
            /// </returns>
            object GetFromCache(string cacheId);

            /// <summary>
            /// The remove from cache.
            /// </summary>
            /// <param name="cacheId">
            /// The cache id.
            /// </param>
            void RemoveFromCache(string cacheId);

            /// <summary>
            /// The set in cache.
            /// </summary>
            /// <param name="cacheId">
            /// The cache id.
            /// </param>
            /// <param name="cacheObject">
            /// The cache object.
            /// </param>
            /// <param name="slidingExpiration">
            /// The sliding expiration.
            /// </param>
            /// <param name="onRemoveCallback">
            /// The on remove callback.
            /// </param>
            void SetInCache(string cacheId, object cacheObject, TimeSpan slidingExpiration);
    }
}
