using Android.Webkit;
using AWebView = Android.Webkit.WebView;
using System;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// An implementation of <see cref="WebViewManager"/> that uses the Android WebKit WebView browser control
	/// to render web content.
	/// </summary>
	public class AndroidWebKitWebViewManager : WebViewManager
	{
		// Using an IP address means that WebView doesn't wait for any DNS resolution,
		// making it substantially faster. Note that this isn't real HTTP traffic, since
		// we intercept all the requests within this origin.
		private const string AppOrigin = "https://0.0.0.0/";
		private static readonly Android.Net.Uri AndroidAppOriginUri = Android.Net.Uri.Parse(AppOrigin)!;
		private readonly BlazorWebViewHandler _blazorMauiWebViewHandler;
		private readonly AWebView _webview;

		/// <summary>
		/// Constructs an instance of <see cref="AndroidWebKitWebViewManager"/>.
		/// </summary>
		/// <param name="webview">A wrapper to access platform-specific WebView2 APIs.</param>
		/// <param name="services">A service provider containing services to be used by this class and also by application code.</param>
		/// <param name="dispatcher">A <see cref="Dispatcher"/> instance that can marshal calls to the required thread or sync context.</param>
		/// <param name="fileProvider">Provides static content to the webview.</param>
		/// <param name="hostPageRelativePath">Path to the host page within the <paramref name="fileProvider"/>.</param>
		public AndroidWebKitWebViewManager(BlazorWebViewHandler blazorMauiWebViewHandler, AWebView webview, IServiceProvider services, Dispatcher dispatcher, IFileProvider fileProvider, string hostPageRelativePath)
			: base(services, dispatcher, new Uri(AppOrigin), fileProvider, hostPageRelativePath)
		{
			_blazorMauiWebViewHandler = blazorMauiWebViewHandler ?? throw new ArgumentNullException(nameof(blazorMauiWebViewHandler));
			_webview = webview ?? throw new ArgumentNullException(nameof(webview));

			InitializeWebView();
		}

		/// <inheritdoc />
		protected override void NavigateCore(Uri absoluteUri)
		{
			_webview.LoadUrl(absoluteUri.AbsoluteUri);
		}

		/// <inheritdoc />
		protected override void SendMessage(string message)
		{
			_webview.PostWebMessage(new WebMessage(message), AndroidAppOriginUri);
		}

		internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out string headers) =>
			TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);
		//internal bool TryGetResponseContentInternal(string uri, bool allowFallbackOnHostPage, out int statusCode, out string statusMessage, out Stream content, out IDictionary<string, string> headers) =>
		//	TryGetResponseContent(uri, allowFallbackOnHostPage, out statusCode, out statusMessage, out content, out headers);

		internal void SetUpMessageChannel()
		{
			var nativeToJsPorts = _webview.CreateWebMessageChannel();

			var nativeToJs = new BlazorWebMessageCallback(message =>
			{
				MessageReceived(new Uri(AppOrigin), message!);
			});

			var destPort = new[] { nativeToJsPorts[1] };

			nativeToJsPorts[0].SetWebMessageCallback(nativeToJs);

			_webview.PostWebMessage(new WebMessage("capturePort", destPort), AndroidAppOriginUri);
		}

		private void InitializeWebView()
		{
			//ApplyDefaultWebViewSettings();
		}

		private class BlazorWebMessageCallback : WebMessagePort.WebMessageCallback
		{
			private readonly Action<string?> _onMessageReceived;

			public BlazorWebMessageCallback(Action<string?> onMessageReceived)
			{
				_onMessageReceived = onMessageReceived ?? throw new ArgumentNullException(nameof(onMessageReceived));
			}

			public override void OnMessage(WebMessagePort? port, WebMessage? message)
			{
				if (message is null)
				{
					throw new ArgumentNullException(nameof(message));
				}

				_onMessageReceived(message.Data);
			}
		}

		private void ApplyDefaultWebViewSettings()
		{
			//// Desktop applications typically don't want the default web browser context menu
			//_webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

			//// Desktop applications almost never want to show a URL preview when hovering over a link
			//_webview.CoreWebView2.Settings.IsStatusBarEnabled = false;

			//// Desktop applications don't normally want to enable things like "alt-left to go back"
			//// or "ctrl+f to find". Developers should explicitly opt into allowing these.
			//// Issues #30511 and #30624 track making an option to control this.
			//_webview.AcceleratorKeyPressed += (sender, eventArgs) =>
			//{
			//    if (eventArgs.VirtualKey != 0x49) // Allow ctrl+shift+i to open dev tools, at least for now
			//    {
			//        // Note: due to what seems like a bug (https://github.com/MicrosoftEdge/WebView2Feedback/issues/549),
			//        // setting eventArgs.Handled doesn't actually have any effect in WPF, even though it works fine in
			//        // WinForms. Leaving the code here because it's supposedly fixed in a newer version.
			//        eventArgs.Handled = true;
			//    }
			//};
		}
	}
}
