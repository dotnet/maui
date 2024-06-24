using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.Content;
using Android.Webkit;
using Java.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;
using AUri = Android.Net.Uri;

namespace Microsoft.Maui.Handlers
{
	public partial class HybridWebViewHandler : ViewHandler<IHybridWebView, AWebView>
	{
		private HybridWebViewJavaScriptInterface? _javaScriptInterface;

		protected override AWebView CreatePlatformView()
		{
#if DEBUG
			var logger = MauiContext!.Services!.GetService<ILogger<HybridWebViewHandler>>() ?? NullLogger<HybridWebViewHandler>.Instance;
			logger.LogInformation("HybridWebViewHandler: CreatePlatformView Android WebView");
#endif
			var platformView = new HybridPlatformWebView(this, Context!)
			{
				LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
			};

			platformView.Settings.DomStorageEnabled = true;
			platformView.Settings.SetSupportMultipleWindows(true);

			// Note that this is a per-app setting and not per-control, so if you enable
			// this, it is enabled for all Android WebViews in the app.
			AWebView.SetWebContentsDebuggingEnabled(enabled: true); // TODO: Get from setting

			platformView.Settings.JavaScriptEnabled = true;

			_javaScriptInterface = new HybridWebViewJavaScriptInterface(this);
			platformView.AddJavascriptInterface(_javaScriptInterface, "hybridWebViewHost");

			return platformView;
		}

		private sealed class HybridWebViewJavaScriptInterface : Java.Lang.Object
		{
			private readonly HybridWebViewHandler _hybridWebViewHandler;

			public HybridWebViewJavaScriptInterface(HybridWebViewHandler hybridWebViewHandler)
			{
				_hybridWebViewHandler = hybridWebViewHandler;
			}

			[JavascriptInterface]
			[Export("sendMessage")]
			public void SendMessage(string message)
			{
				_hybridWebViewHandler.VirtualView?.RawMessageReceived(message);
			}
		}

		protected override void ConnectHandler(AWebView platformView)
		{
#if DEBUG
			var logger = MauiContext!.Services!.GetService<ILogger<HybridWebViewHandler>>() ?? NullLogger<HybridWebViewHandler>.Instance;
			logger.LogInformation("HybridWebViewHandler: Connecting WebView2");
#endif

			base.ConnectHandler(platformView);

			var webViewClient = new AndroidHybridWebViewClient(this);
			PlatformView.SetWebViewClient(webViewClient);

			platformView.LoadUrl(new Uri(AppOriginUri, "/").ToString());
		}

		protected override void DisconnectHandler(AWebView platformView)
		{
#if DEBUG
			var logger = MauiContext!.Services!.GetService<ILogger<HybridWebViewHandler>>() ?? NullLogger<HybridWebViewHandler>.Instance;
			logger.LogInformation("HybridWebViewHandler: Disconnecting WebView2");
#endif

			// TODO: Consider this code from WebView
			//if (OperatingSystem.IsAndroidVersionAtLeast(26))
			//{
			//	if (platformView.WebViewClient is MauiWebViewClient webViewClient)
			//		webViewClient.Disconnect();

			//	if (platformView.WebChromeClient is MauiWebChromeClient webChromeClient)
			//		webChromeClient.Disconnect();
			//}

			//platformView.SetWebViewClient(null!);
			//platformView.SetWebChromeClient(null);

			//platformView.StopLoading();


			base.DisconnectHandler(platformView);
		}

		protected class HybridPlatformWebView : AWebView, IHybridPlatformWebView
		{
			private readonly WeakReference<HybridWebViewHandler> _handler;
			private static readonly AUri AndroidAppOriginUri = AUri.Parse(AppOrigin)!;

			public HybridPlatformWebView(HybridWebViewHandler handler, Context context) : base(context)
			{
				ArgumentNullException.ThrowIfNull(handler, nameof(handler));
				_handler = new WeakReference<HybridWebViewHandler>(handler);
			}

			public void SendRawMessage(string rawMessage)
			{
#pragma warning disable CA1416 // Validate platform compatibility
				PostWebMessage(new WebMessage(rawMessage), AndroidAppOriginUri);
#pragma warning restore CA1416 // Validate platform compatibility
			}
		}

		public static void MapSendRawMessage(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
			if (arg is not string rawMessage || handler.PlatformView is not IHybridPlatformWebView hybridPlatformWebView)
			{
				return;
			}

			hybridPlatformWebView.SendRawMessage(rawMessage);
		}
	}

	public class AndroidHybridWebViewClient : WebViewClient
	{
		private readonly HybridWebViewHandler _handler;

		public AndroidHybridWebViewClient(HybridWebViewHandler handler)
		{
			_handler = handler;
		}

		public override WebResourceResponse? ShouldInterceptRequest(AWebView? view, IWebResourceRequest? request)
		{
			var fullUrl = request?.Url?.ToString();
			var requestUri = HybridWebViewQueryStringHelper.RemovePossibleQueryString(fullUrl);

			if (new Uri(requestUri) is Uri uri && HybridWebViewHandler.AppOriginUri.IsBaseOf(uri))
			{
				var relativePath = HybridWebViewHandler.AppOriginUri.MakeRelativeUri(uri).ToString().Replace('/', '\\');

				string contentType;
				if (string.IsNullOrEmpty(relativePath))
				{
					relativePath = _handler.DefaultFile;
					contentType = "text/html";
				}
				else
				{
					var requestExtension = Path.GetExtension(relativePath);
					contentType = requestExtension switch
					{
						".htm" or ".html" => "text/html",
						".js" => "application/javascript",
						".css" => "text/css",
						_ => "text/plain",
					};
				}

				Stream? contentStream = null;

				var assetPath = Path.Combine(_handler.HybridRoot!, relativePath!);
				contentStream = PlatformOpenAppPackageFile(assetPath);

				if (contentStream is null)
				{
					var notFoundContent = "Resource not found (404)";

					var notFoundByteArray = Encoding.UTF8.GetBytes(notFoundContent);
					var notFoundContentStream = new MemoryStream(notFoundByteArray);

					return new WebResourceResponse("text/plain", "UTF-8", 404, "Not Found", GetHeaders("text/plain"), notFoundContentStream);
				}
				else
				{
					// TODO: We don't know the content length because Android doesn't tell us. Seems to work without it!
					return new WebResourceResponse(contentType, "UTF-8", 200, "OK", GetHeaders(contentType), contentStream);
				}
			}
			else
			{
				return base.ShouldInterceptRequest(view, request);
			}
		}

		private Stream? PlatformOpenAppPackageFile(string filename)
		{
			filename = PathUtils.NormalizePath(filename);

			try
			{
				return _handler.Context.Assets?.Open(filename);
			}
			catch (Java.IO.FileNotFoundException)
			{
				return null;
			}
		}

		internal static class PathUtils
		{
			public static string NormalizePath(string filename) =>
				filename
					.Replace('\\', Path.DirectorySeparatorChar)
					.Replace('/', Path.DirectorySeparatorChar);
		}

		private protected static IDictionary<string, string> GetHeaders(string contentType) =>
			new Dictionary<string, string> {
				{ "Content-Type", contentType },
			};
	}
}
