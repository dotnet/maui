using System;
using System.Diagnostics;
using System.Text;
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

		// Arbitrary local host name for virtual folder mapping
		const string LocalHostName = "appdir";
		const string LocalScheme = $"https://{LocalHostName}/";

		// Script to insert a <base> tag into an HTML document
		const string BaseInsertionScript = @"
			var base = document.createElement('base');
			base.href = 'baseTag';
			document.getElementsByTagName('head')[0].appendChild(base);";

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
		    		Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
		    }

			// Insert script to set the base tag
			var script = $"<script>{BaseInsertionScript.Replace("baseTag", baseUrl, StringComparison.Ordinal)}</script>";
			var htmlWithScript = $"{script}\n{html}";

			NavigateToString(htmlWithScript);
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

			var localSchemeScript = $"<script>{BaseInsertionScript.Replace("baseTag", LocalScheme, StringComparison.Ordinal)}</script>";
			return decodedHtml.Contains(
				localSchemeScript,
				StringComparison.OrdinalIgnoreCase);
		}
	}
}
