using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace Microsoft.Maui.Platform
{
	public partial class MauiWebView : WebView2, IWebViewDelegate
	{
		readonly WeakReference<WebViewHandler> _handler;

		[Obsolete("Constructor is no longer used, please use an overloaded version.")]
#pragma warning disable CS8618
		public MauiWebView()
		{
			SetupPlatformEvents();
		}
#pragma warning restore CS8618

		public MauiWebView(WebViewHandler handler)
		{
			ArgumentNullException.ThrowIfNull(handler, nameof(handler));
			_handler = new WeakReference<WebViewHandler>(handler);

			SetupPlatformEvents();
		}

		// Arbitrary local host name for virtual folder mapping
		const string LocalHostName = "appdir";
		const string LocalScheme = $"https://{LocalHostName}/";

		// Script to insert a <base> tag into an HTML document
		const string BaseInsertionScript = @"
			var head = document.getElementsByTagName('head')[0];
			var bases = head.getElementsByTagName('base');
			if(bases.length == 0) {
				head.innerHTML = 'baseTag' + head.innerHTML;
			}";

		// Allow for packaged/unpackaged app support
		static string ApplicationPath => AppInfoUtils.IsPackagedApp
			? Package.Current.InstalledLocation.Path
			: AppContext.BaseDirectory;

		public async void LoadHtml(string? html, string? baseUrl)
		{
			var mapBaseDirectory = false;
			if (string.IsNullOrEmpty(baseUrl))
			{
				baseUrl = LocalScheme;
				mapBaseDirectory = true;
			}

			await EnsureCoreWebView2Async();

			if (mapBaseDirectory)
			{
				CoreWebView2.SetVirtualHostNameToFolderMapping(
					LocalHostName,
					ApplicationPath,
					Web.WebView2.Core.CoreWebView2HostResourceAccessKind.DenyCors);
			}

			// Insert script to set the base tag
			var script = GetBaseTagInsertionScript(baseUrl);
			var htmlWithScript = $"{script}\n{html}";

			NavigateToString(htmlWithScript);
		}

		public async void LoadUrl(string? url)
		{
			Uri uri = new Uri(url ?? string.Empty, UriKind.RelativeOrAbsolute);

			if (IsLocalAppDirUrl(url ?? string.Empty))
			{
				await EnsureCoreWebView2Async();

				CoreWebView2.SetVirtualHostNameToFolderMapping(
					LocalHostName,
					ApplicationPath,
					Web.WebView2.Core.CoreWebView2HostResourceAccessKind.DenyCors);

				if (!uri.IsAbsoluteUri)
					uri = new Uri(LocalScheme + url, UriKind.RelativeOrAbsolute);
			}

			if (_handler?.TryGetTarget(out var handler) ?? false)
				await handler.SyncPlatformCookies(uri.AbsoluteUri);

			try
			{
				Source = uri;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(nameof(MauiWebView), $"Failed to load: {uri} {exc}");
			}
		}

		void SetupPlatformEvents()
		{
			NavigationStarting += (sender, args) =>
			{
				// Auto map local virtual app dir host, e.g. if navigating back to local site from a link to an external site
				if (IsUriWithLocalScheme(args?.Uri) ||
					IsWebView2DataUriWithBaseUrl(args?.Uri))
				{
					CoreWebView2.SetVirtualHostNameToFolderMapping(
						LocalHostName,
						ApplicationPath,
						Web.WebView2.Core.CoreWebView2HostResourceAccessKind.DenyCors);
				}
				// Auto unmap local virtual app dir host if navigating to any other potentially unsafe domain
				else
				{
					CoreWebView2.ClearVirtualHostNameToFolderMapping(LocalHostName);
				}
			};
		}

		// Determines whether a URL should be served from the local app directory,
		// which is mapped to the "appdir" virtual host. Relative URLs are always
		// treated as local; absolute URLs are only local when their parsed host is
		// exactly "appdir" over https. This mirrors the decision made in LoadUrl and
		// is exposed as an internal helper so the classification can be unit tested
		// without spinning up a live WebView2.
		internal static bool IsLocalAppDirUrl(string? url)
		{
			if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
				return false;

			// Relative URLs stay local and are later rebased onto the appdir host.
			// Absolute URLs reuse the already-parsed Uri (no second parse).
			return !uri.IsAbsoluteUri || IsUriWithLocalScheme(uri);
		}

		// String entry point used by the NavigationStarting handler (args.Uri is a
		// string) and by tests. Parses once, then delegates to the Uri-based check.
		internal static bool IsUriWithLocalScheme(string? uri) =>
			Uri.TryCreate(uri, UriKind.Absolute, out var parsed) && IsUriWithLocalScheme(parsed);

		// Core check on an already-parsed Uri. "appdir" is an arbitrary placeholder
		// virtual host that is mapped to the app directory via
		// SetVirtualHostNameToFolderMapping, so ONLY the exact host "appdir" serves
		// local content. Returns true only when the URI is absolute, its scheme is
		// https, and its host is exactly "appdir". Matching the parsed host - rather
		// than a raw string prefix - prevents unrelated hosts such as
		// "appdir.example.com" (subdomain) or "appdir@host.example.com" (userinfo)
		// from being treated as local content. The comparison is case-insensitive to
		// mirror WebView2, which canonicalizes host names case-insensitively (e.g.
		// "APPDIR" == "appdir"); System.Uri already lower-cases Host, so this is
		// belt-and-suspenders. A trailing dot ("appdir.") is intentionally a
		// different host and is not treated as local, which also matches WebView2.
		static bool IsUriWithLocalScheme(Uri uri) =>
			uri.IsAbsoluteUri &&
			uri.Scheme == Uri.UriSchemeHttps &&
			string.Equals(uri.Host, LocalHostName, StringComparison.OrdinalIgnoreCase);

		static bool IsWebView2DataUriWithBaseUrl(string? uri)
		{
			// WebView2 sends the web page with inserted base tag as data URI
			const string dataUriBase64 = "data:text/html;charset=utf-8;base64,";
			if (uri == null ||
				uri.StartsWith(
					dataUriBase64,
					StringComparison.OrdinalIgnoreCase) == false)
				return false;

			string decodedHtml = Encoding.UTF8.GetString(
				Convert.FromBase64String(
					uri.Substring(dataUriBase64.Length)));

			var localSchemeScript = GetBaseTagInsertionScript(LocalScheme);
			return decodedHtml.Contains(
				localSchemeScript,
				StringComparison.OrdinalIgnoreCase);
		}

		static string GetBaseTagInsertionScript(string baseUrl)
		{
			var baseTag = $"<base href=\"{baseUrl}\"></base>";
			return $"<script>{BaseInsertionScript.Replace("baseTag", baseTag, StringComparison.Ordinal)}</script>";
		}
	}
}
