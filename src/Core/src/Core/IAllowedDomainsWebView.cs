using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides a mechanism to restrict which domains a web view can navigate to and load resources from.
	/// </summary>
	/// <remarks>
	/// When <see cref="AllowedDomains"/> is <see langword="null"/> or empty, all URLs are allowed (default behavior).
	/// When set, only URLs whose host matches one of the listed domains (or is a subdomain of one) are permitted.
	/// The app's internal origin (e.g., <c>app://</c> for HybridWebView/BlazorWebView) is always implicitly allowed.
	/// <para>
	/// On iOS and Mac Catalyst, for sub-resource blocking (images, scripts, CSS, fetch), the domains must also be declared
	/// in the app's <c>Info.plist</c> under <c>WKAppBoundDomains</c>. Apple enforces a maximum of 10 domains and requires HTTPS.
	/// </para>
	/// </remarks>
	public interface IAllowedDomainsWebView
	{
		/// <summary>
		/// Gets the list of domains that this web view is allowed to navigate to and load resources from.
		/// </summary>
		/// <remarks>
		/// Each entry should be a plain domain string (e.g., <c>"example.com"</c>, <c>"cdn.example.com"</c>).
		/// Do not include a scheme or path. Subdomains of listed domains are implicitly allowed
		/// (e.g., listing <c>"example.com"</c> also allows <c>"sub.example.com"</c>).
		/// <para>
		/// When <see langword="null"/> or empty, all domains are allowed. When populated, only the listed domains
		/// (and their subdomains) are permitted.
		/// </para>
		/// <para>
		/// On iOS and Mac Catalyst, OS-level sub-resource enforcement (<c>WKAppBoundDomains</c> /
		/// <c>LimitsNavigationsToAppBoundDomains</c>) is configured when the underlying <c>WKWebView</c> is
		/// created, so set <see cref="AllowedDomains"/> before the handler is created. Changing it afterward
		/// still updates navigation-level checks, but the OS-level app-bound configuration is not re-applied.
		/// </para>
		/// </remarks>
		IList<string>? AllowedDomains { get; }
	}
}
