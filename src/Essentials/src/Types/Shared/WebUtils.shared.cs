using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.ApplicationModel
{
	static class WebUtils
	{
		internal static IDictionary<string, string> ParseQueryString(string url)
		{
			var d = new Dictionary<string, string>();

			if (string.IsNullOrWhiteSpace(url) || (url.IndexOf("?", StringComparison.Ordinal) == -1 && url.IndexOf("#", StringComparison.Ordinal) == -1))
				return d;

			var qsStartIndex = url.IndexOf("?", StringComparison.Ordinal);
			if (qsStartIndex < 0)
				qsStartIndex = url.IndexOf("#", StringComparison.Ordinal);

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
