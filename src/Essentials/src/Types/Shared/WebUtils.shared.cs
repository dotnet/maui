#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Maui
{
	static class WebUtils
	{
#if !NETSTANDARD
		internal static string RemovePossibleQueryString(string? url)
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
#endif

		internal static Dictionary<string, string> ParseQueryString(Uri uri, bool includeFragment = true)
		{
			var parameters = new Dictionary<string, string>(StringComparer.Ordinal);

			if (uri == null)
				return parameters;

			// Note: Uri.Query starts with a '?'
			if (!string.IsNullOrEmpty(uri.Query))
				UnpackParameters(uri.Query.AsSpan(1), parameters);

			if (includeFragment)
			{
				// Note: Uri.Fragment starts with a '#'
				if (!string.IsNullOrEmpty(uri.Fragment))
					UnpackParameters(uri.Fragment.AsSpan(1), parameters);
			}

			return parameters;
		}

		// The following method is a port of the logic found in https://source.dot.net/#Microsoft.AspNetCore.WebUtilities/src/Shared/QueryStringEnumerable.cs
		// but refactored such that it:
		//
		// 1. avoids the IEnumerable overhead that isn't needed (the ASP.NET logic was clearly designed that way to offer a public API whereas we don't need that)
		// 2. avoids the use of unsafe code
		internal static void UnpackParameters(ReadOnlySpan<char> query, Dictionary<string, string> parameters)
		{
			while (!query.IsEmpty)
			{
				int delimeterIndex = query.IndexOf('&');
				ReadOnlySpan<char> segment;

				if (delimeterIndex >= 0)
				{
					segment = query.Slice(0, delimeterIndex);
					query = query.Slice(delimeterIndex + 1);
				}
				else
				{
					segment = query;
					query = default;
				}

				// If it's nonempty, emit it
				if (!segment.IsEmpty)
				{
					var equalIndex = segment.IndexOf('=');
					string name, value;

					if (equalIndex >= 0)
					{
						name = segment.Slice(0, equalIndex).ToString();

						var span = segment.Slice(equalIndex + 1);
						var chars = new char[span.Length];

						for (int i = 0; i < span.Length; i++)
							chars[i] = span[i] == '+' ? ' ' : span[i];

						value = new string(chars);
					}
					else
					{
						name = segment.ToString();
						value = string.Empty;
					}

					name = Uri.UnescapeDataString(name);

					parameters[name] = Uri.UnescapeDataString(value);
				}
			}
		}

		internal static Uri EscapeUri(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			var idn = new global::System.Globalization.IdnMapping();
			return new Uri(uri.Scheme + "://" + idn.GetAscii(uri.Authority) + uri.PathAndQuery + uri.Fragment);
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

#if __IOS__ || __TVOS__ || __MACOS__
        internal static Foundation.NSUrl GetNativeUrl(Uri uri)
        {
            try
            {
                return new Foundation.NSUrl(uri.OriginalString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to create NSUrl from Original string, trying Absolute URI: {ex.Message}");
                return new Foundation.NSUrl(uri.AbsoluteUri);
            }
        }
#endif
	}
}
