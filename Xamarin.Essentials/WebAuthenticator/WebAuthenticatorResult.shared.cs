using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.Essentials
{
    public class WebAuthenticatorResult
    {
        public WebAuthenticatorResult()
        {
        }

        public WebAuthenticatorResult(Uri uri)
        {
            foreach (var kvp in WebUtils.ParseQueryString(uri.ToString()))
            {
                Properties[kvp.Key] = kvp.Value;
            }
        }

        public WebAuthenticatorResult(IDictionary<string, string> properties)
        {
            foreach (var kvp in properties)
                Properties[kvp.Key] = kvp.Value;
        }

        public DateTimeOffset Timestamp { get; set; } = new DateTimeOffset(DateTime.UtcNow);

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public void Put(string key, string value)
            => Properties[key] = value;

        public string Get(string key)
        {
            if (Properties.TryGetValue(key, out var v))
                return v;

            return default;
        }

        public string AccessToken
            => Get("access_token");

        public string TokenType
            => Get("token_type");

        public string RefreshToken
            => Get("refresh_token");

        public DateTimeOffset? RefreshTokenExpiresIn
        {
            get
            {
                if (Properties.TryGetValue("refresh_token_expires_in", out var v))
                {
                    if (int.TryParse(v, out var i))
                        return Timestamp.AddSeconds(i);
                }

                return null;
            }
        }

        public DateTimeOffset? ExpiresIn
        {
            get
            {
                if (Properties.TryGetValue("expires_in", out var v))
                {
                    if (int.TryParse(v, out var i))
                        return Timestamp.AddSeconds(i);
                }

                return null;
            }
        }
    }
}
