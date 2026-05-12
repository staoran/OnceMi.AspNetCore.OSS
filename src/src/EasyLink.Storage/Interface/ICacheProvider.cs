using System;

namespace EasyLink.Storage
{
    /// <summary>
    /// Provides cache operations used by EasyLink.Storage.
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Removes a cached value.
        /// </summary>
        /// <param name="key">The cache key.</param>
        void Remove(string key);

        /// <summary>
        /// Gets a cached value by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <typeparam name="T">The cached value type.</typeparam>
        /// <returns>The cached value, or null when it does not exist.</returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// Stores a value in the cache.
        /// </summary>
        /// <typeparam name="T">The cached value type.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="ts">The cache lifetime.</param>
        void Set<T>(string key, T value, TimeSpan ts) where T : class;
    }
}
