using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WebView2, IWebViewDelegate
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

		WebView2? _internalWebView;

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
		string ApplicationPath => AppInfoUtils.IsPackagedApp
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

			// Generate a base tag for the document
			var baseTag = $"<base href=\"{baseUrl}\"></base>";

			string htmlWithBaseTag;

			// Set up an internal WebView we can use to load and parse the original HTML string
			// Make _internalWebView a field instead of local variable to avoid garbage collection
			_internalWebView = new WebView2();

			// TODO: For now, the CoreWebView2 won't be created without either setting Source or
			// calling EnsureCoreWebView2Async().
			await _internalWebView.EnsureCoreWebView2Async();

			// When the 'navigation' to the original HTML string is done, we can modify it to include our <base> tag
			_internalWebView.NavigationCompleted += async (sender, args) =>
			{
				// Generate a version of the <base> script with the correct <base> tag
				var script = BaseInsertionScript.Replace("baseTag", baseTag, StringComparison.Ordinal);

				// Run it and retrieve the updated HTML from our WebView
				await sender.ExecuteScriptAsync(script);
				htmlWithBaseTag = await sender.ExecuteScriptAsync("document.documentElement.outerHTML;");

				htmlWithBaseTag = Regex.Unescape(htmlWithBaseTag);
				htmlWithBaseTag = htmlWithBaseTag.Remove(0, 1);
				htmlWithBaseTag = htmlWithBaseTag.Remove(htmlWithBaseTag.Length - 1, 1);

				await EnsureCoreWebView2Async();

				if (mapBaseDirectory)
				{
					CoreWebView2.SetVirtualHostNameToFolderMapping(
						LocalHostName,
						ApplicationPath,
						Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
				}

				// Set the HTML for the 'real' WebView to the updated HTML
				NavigateToString(!string.IsNullOrEmpty(htmlWithBaseTag) ? htmlWithBaseTag : html);

				// Free up memory after we're done with _internalWebView
				if (_internalWebView.IsValid())
				{
					_internalWebView.Close();
					_internalWebView = null;
				}
			};

			// Kick off the initial navigation
			if (_internalWebView.IsValid())
				_internalWebView.NavigateToString(html);
		}

		public async void LoadUrl(string? url)
		{
			Uri uri = new Uri(url ?? string.Empty, UriKind.RelativeOrAbsolute);

			if (!uri.IsAbsoluteUri ||
				IsUriWithLocalScheme(uri.AbsoluteUri))
			{
				await EnsureCoreWebView2Async();

				CoreWebView2.SetVirtualHostNameToFolderMapping(
					LocalHostName,
					ApplicationPath,
					Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

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
						Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
				}
				// Auto unmap local virtual app dir host if navigating to any other potentially unsafe domain
				else
				{
					CoreWebView2.ClearVirtualHostNameToFolderMapping(LocalHostName);
				}
			};
		}

		static bool IsUriWithLocalScheme(string? uri)
		{
			return uri?
				.StartsWith(
					LocalScheme.TrimEnd('/'),
					StringComparison.OrdinalIgnoreCase) == true;
		}

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

			return decodedHtml.Contains(
				$"<base href=\"{LocalScheme}",
				StringComparison.OrdinalIgnoreCase);
		}
	}
}
