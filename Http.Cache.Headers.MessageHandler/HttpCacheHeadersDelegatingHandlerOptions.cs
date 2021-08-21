using System;
using System.Text.RegularExpressions;

namespace Http.Cache.Headers.MessageHandler
{
    public class HttpCacheHeadersDelegatingHandlerOptions
    {

        private string _cacheEntryPrefix;

        private const string CacheEntryPrefixRegex = "(\\w*\\d*)+";
        
        /// <summary>
        /// Use this to ignore non GET requests.
        /// <remarks>Think about the implications of caching non GET requests before toggling this.</remarks>
        /// </summary>
        public bool IgnoreNonGetRequest { get; set; }

        /// <summary>
        /// Use this to set a prefix string for your cache entry. Defaults to <value>ETag</value>
        /// <exception cref="ArgumentException">When alphanumeric validation fails</exception>
        /// </summary>
        public string CacheEntryPrefix
        {
            get => _cacheEntryPrefix ??= "ETag";
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !Regex.IsMatch(value, CacheEntryPrefixRegex))
                {
                    throw new ArgumentException($"Value of {nameof(CacheEntryPrefix)} must be alphanumeric");
                }
                _cacheEntryPrefix = value;
            } 
        }

        /// <summary>
        /// Use this to set header that should be ignored when generating the cache key
        /// </summary>
        /// <remarks>Cache key generation includes headers to be unique</remarks>
        public string[] CacheKeyHeaderNameExclusions = Array.Empty<string>();
    }
}