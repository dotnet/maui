using System;
using System.Collections.Generic;
using System.Linq;

#if !NETSTANDARD
namespace Microsoft.Maui
{
	internal class HybridWebViewQueryStringHelper
	{
		public static string RemovePossibleQueryString(string? url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return string.Empty;
			}
			var indexOfQueryString = url.IndexOf('?', StringComparison.Ordinal);
			return (indexOfQueryString == -1)
				? url
				: url.Substring(0, indexOfQueryString);
		}

		// TODO: Replace this

		/// <summary>
		/// A simple utility that takes a URL, extracts the query string and returns a dictionary of key-value pairs.
		/// Note that values are unescaped. Manually created URLs in JavaScript should use encodeURIComponent to escape values.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetKeyValuePairs(string? url)
		{
			var result = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(url))
			{
				var query = new Uri(url).Query;
				if (query != null && query.Length > 1)
				{
					result = query
						.Substring(1)
						.Split('&')
						.Select(p => p.Split('='))
						.ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));
				}
			}

			return result;
		}
	}
}
#endif
