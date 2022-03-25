using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public class MauiWebView : WebView2, IWebViewDelegate
	{
		WebView2? _internalWebView;

		const string LocalScheme = "ms-appx-web:///";

		// Script to insert a <base> tag into an HTML document
		const string BaseInsertionScript = @"
			var head = document.getElementsByTagName('head')[0];
			var bases = head.getElementsByTagName('base');
			if(bases.length == 0) {
				head.innerHTML = 'baseTag' + head.innerHTML;
			}";

		public async void LoadHtml(string? html, string? baseUrl)
		{
			if (string.IsNullOrEmpty(baseUrl))
			{
				baseUrl = LocalScheme;
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

				// Set the HTML for the 'real' WebView to the updated HTML
				NavigateToString(!string.IsNullOrEmpty(htmlWithBaseTag) ? htmlWithBaseTag : html);

				// Free up memory after we're done with _internalWebView
				_internalWebView = null;
			};

			// Kick off the initial navigation
			_internalWebView.NavigateToString(html);
		}

		public void LoadUrl(string? url)
		{
			Uri uri = new Uri(url ?? string.Empty, UriKind.RelativeOrAbsolute);

			if (!uri.IsAbsoluteUri)
			{
				uri = new Uri(LocalScheme + url, UriKind.RelativeOrAbsolute);
			}

			// TODO: Sync Cookies

			try
			{
				Source = uri;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(nameof(MauiWebView), $"Failed to load: {uri} {exc}");
			}
		}
	}
}
