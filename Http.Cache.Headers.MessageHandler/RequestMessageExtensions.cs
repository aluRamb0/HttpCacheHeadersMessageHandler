using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Http.Cache.Headers.MessageHandler
{
    public static class RequestMessageExtensions
    {
        public static string GetRequestKey(this HttpRequestMessage request, params string[] headerNameExclusions)
        {
            var requestHeaderValues = request
                .Headers
                    //remove conditional request headers https://developer.mozilla.org/en-US/docs/Web/HTTP/Conditional_requests
                .Where(header=> !header.Key.StartsWith("If-", StringComparison.InvariantCultureIgnoreCase))
                    //any other headers you want to exclude
                .Where(header=> !headerNameExclusions.Contains(header.Key, StringComparer.InvariantCultureIgnoreCase))
                .SelectMany(header => header.Value)
                .ToList();
            var path = request.RequestUri.AbsoluteUri;

            return string.Join("-", requestHeaderValues.Union(new[] { path }));
        }
        
        public static string ToSha256(this string value)
        {
            var stringBuilder = new StringBuilder();

            using (var hash = SHA256.Create())            
            {
                var enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(value));

                foreach (var b in result)
                    stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
        
    }
}