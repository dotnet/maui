using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Provides shared domain-matching logic for URL allowlisting across all web view types.
	/// </summary>
	internal static class WebViewDomainAllowlist
	{
		/// <summary>
		/// Determines whether a URL should be allowed based on the provided domain allowlist.
		/// </summary>
		/// <param name="url">The URL being navigated to or loaded.</param>
		/// <param name="allowedDomains">The list of allowed domains. If null or empty, all URLs are allowed.</param>
		/// <param name="appOriginUri">The app-internal origin URI (always allowed). May be null for standard WebView.</param>
		/// <returns><see langword="true"/> if the URL is allowed; otherwise, <see langword="false"/>.</returns>
		internal static bool IsUrlAllowed(string? url, IList<string>? allowedDomains, Uri? appOriginUri = null)
		{
			if (allowedDomains is null || allowedDomains.Count == 0)
				return true;

			if (string.IsNullOrWhiteSpace(url))
				return false;

			if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
				return false;

			// Always allow app-internal origin
			if (appOriginUri is not null && appOriginUri.IsBaseOf(uri))
				return true;

			// Always allow special schemes
			var scheme = uri.Scheme;
			if (string.Equals(scheme, "data", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(scheme, "about", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(scheme, "javascript", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(scheme, "blob", StringComparison.OrdinalIgnoreCase))
				return true;

			var host = uri.Host;
			if (string.IsNullOrEmpty(host))
				return false;

			for (int i = 0; i < allowedDomains.Count; i++)
			{
				var domain = allowedDomains[i];
				if (string.IsNullOrEmpty(domain))
					continue;

				if (host.Equals(domain, StringComparison.OrdinalIgnoreCase))
					return true;

				if (host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}
	}
}
