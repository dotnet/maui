using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.Essentials
{
    static class WebUtils
    {
        internal static IDictionary<string, string> ParseQueryString(string url)
        {
            var d = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(url) || (!url.Contains("?") && !url.Contains("#")))
                return d;

            var qsStartIndex = url.IndexOf('?');
            if (qsStartIndex < 0)
                qsStartIndex = url.IndexOf('#');

            if (url.Length - 1 < qsStartIndex + 1)
                return d;

            var qs = url.Substring(qsStartIndex + 1);

            var kvps = qs.Split('&');

            if (kvps == null || !kvps.Any())
                return d;

            foreach (var kvp in kvps)
            {
                var pair = kvp.Split(new char[] { '=' }, 2);

                if (pair == null || pair.Length != 2)
                    continue;

                d[pair[0]] = pair[1];
            }

            return d;
        }

        internal static bool CanHandleCallback(Uri expectedUrl, Uri callbackUrl)
        {
            if (!callbackUrl.Scheme.Equals(expectedUrl.Scheme, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrEmpty(expectedUrl.Host))
            {
                if (!callbackUrl.Host.Equals(expectedUrl.Host, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
    }
}
