using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Provides shared domain-matching logic for URL allowlisting across all web view types.
	/// </summary>
	internal static class WebViewDomainAllowlist
	{
		static readonly IdnMapping s_idnMapping = new IdnMapping();

		/// <summary>
		/// Determines whether a URL should be allowed based on the allowlist of the supplied web view.
		/// </summary>
		/// <param name="url">The URL being navigated to or loaded.</param>
		/// <param name="webView">The web view. If it does not implement <see cref="IAllowedDomainsWebView"/> (or its allowlist is null/empty), all URLs are allowed.</param>
		/// <param name="appOriginUri">The app-internal origin URI (always allowed). May be null for standard WebView.</param>
		/// <returns><see langword="true"/> if the URL is allowed; otherwise, <see langword="false"/>.</returns>
		internal static bool IsUrlAllowed(string? url, IView? webView, Uri? appOriginUri = null)
			=> IsUrlAllowed(url, (webView as IAllowedDomainsWebView)?.AllowedDomains, appOriginUri);

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

			var scheme = uri.Scheme;

			// 'javascript:' navigations execute arbitrary script in the current document and are a
			// well-known allowlist-bypass / XSS vector, so they are NOT allowed once an allowlist is active.
			if (string.Equals(scheme, "javascript", StringComparison.OrdinalIgnoreCase))
				return false;

			// 'data:', 'about:' and 'blob:' have no network host to match against. They reference
			// in-page / in-memory content (e.g. data: images, blob: object URLs, about:blank) that is
			// produced by the already-loaded (allowed) document, so blocking them would break legitimate
			// rendering without adding a meaningful security boundary. They remain allowed.
			if (string.Equals(scheme, "data", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(scheme, "about", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(scheme, "blob", StringComparison.OrdinalIgnoreCase))
				return true;

			// Compare using the ASCII/punycode host so internationalized domain names (IDN) match
			// regardless of whether the allowlist entry or the URL is expressed in Unicode or punycode.
			var host = uri.IdnHost;
			if (string.IsNullOrEmpty(host))
				return false;

			for (int i = 0; i < allowedDomains.Count; i++)
			{
				var domain = allowedDomains[i];
				if (string.IsNullOrEmpty(domain))
					continue;

				var asciiDomain = ToAsciiHost(domain);
				if (string.IsNullOrEmpty(asciiDomain))
					continue;

				if (host.Equals(asciiDomain, StringComparison.OrdinalIgnoreCase))
					return true;

				if (host.EndsWith("." + asciiDomain, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		static string ToAsciiHost(string domain)
		{
			try
			{
				return s_idnMapping.GetAscii(domain);
			}
			catch (ArgumentException)
			{
				// Not a valid IDN label (e.g. malformed entry); fall back to the raw value so plain
				// ASCII hosts still match by ordinal comparison.
				return domain;
			}
		}
	}
}
