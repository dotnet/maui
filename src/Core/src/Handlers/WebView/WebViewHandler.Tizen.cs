using System;
using Tizen.UIExtensions.ElmSharp;
using TChromium = Tizen.WebView.Chromium;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, MauiWebView>
	{
		protected virtual double MinimumSize => 44d;

		TWebView PlatformWebView => PlatformView.WebView;

		public static void MapSource(IWebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;
			handler.PlatformView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(IWebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(IWebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateGoForward(webView);
		}

		public static void MapReload(IWebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateReload(webView);
		}

		public static void MapEval(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		public static void MapEvaluateJavaScriptAsync(IWebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		protected override MauiWebView CreatePlatformView()
		{
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} should have been set by base class.");
			return new MauiWebView(NativeParent)
			{
				MinimumHeight = MinimumSize.ToScaledPixel(),
				MinimumWidth = MinimumSize.ToScaledPixel()
			};
		}

		protected override void ConnectHandler(MauiWebView platformView)
		{
			TChromium.Initialize();
			MauiApplication.Current.Terminated += (sender, arg) => TChromium.Shutdown();
			PlatformWebView.LoadFinished += OnLoadFinished;
		}

		protected override void DisconnectHandler(MauiWebView platformView)
		{
			PlatformWebView.StopLoading();
			PlatformWebView.LoadFinished -= OnLoadFinished;
			base.DisconnectHandler(platformView);
		}

		void OnLoadFinished(object? sender, EventArgs e)
		{
			PlatformWebView.SetFocus(true);
		}
	}
}