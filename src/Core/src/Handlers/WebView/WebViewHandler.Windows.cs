using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	public partial class WebViewHandler : ViewHandler<IWebView, WebView2>
	{
		WebNavigationEvent _eventState;

		protected override WebView2 CreatePlatformView() => new MauiWebView();

		internal WebNavigationEvent CurrentWebNavigationEvent
		{
			get => _eventState;
			set => _eventState = value;
		}
    
		protected override void ConnectHandler(WebView2 platformView)
		{
      platformView.NavigationStarting += OnNavigationStarted;
			platformView.NavigationCompleted += OnNavigationCompleted;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(WebView2 platformView)
		{
			platformView.NavigationStarting -= OnNavigationStarted;
			platformView.NavigationCompleted -= OnNavigationCompleted;

			base.DisconnectHandler(platformView);
		}

		public static void MapSource(WebViewHandler handler, IWebView webView)
		{
			IWebViewDelegate? webViewDelegate = handler.PlatformView as IWebViewDelegate;

			handler.PlatformView?.UpdateSource(webView, webViewDelegate);
		}

		public static void MapGoBack(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.NativeView.CanGoBack)
				handler.CurrentWebNavigationEvent = WebNavigationEvent.Back;

			handler.PlatformView?.UpdateGoBack(webView);
		}

		public static void MapGoForward(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (handler.NativeView.CanGoForward)
				handler.CurrentWebNavigationEvent = WebNavigationEvent.Forward;

			handler.PlatformView?.UpdateGoForward(webView);
		}

		public static void MapReload(WebViewHandler handler, IWebView webView, object? arg)
		{
			handler.PlatformView?.UpdateReload(webView);
		}

		void OnNavigationStarted(WebView2 sender, CoreWebView2NavigationStartingEventArgs e)
		{
			if (Uri.TryCreate(e.Uri, UriKind.Absolute, out Uri? uri) && uri != null)
			{
				bool cancel = VirtualView.Navigating(CurrentWebNavigationEvent, uri.AbsoluteUri);
				
				e.Cancel = cancel;      
				
				// Reset in this case because this is the last event we will get
				if (cancel)
					_eventState = WebNavigationEvent.NewPage;
			}
		}

		void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
		{
			if (args.IsSuccess)
				NavigationSucceeded(sender, args);
			else
				NavigationFailed(sender, args);

			if (VirtualView == null)
				return;

			sender.UpdateCanGoBackForward(VirtualView);
		}

		public static void MapEval(WebViewHandler handler, IWebView webView, object? arg)
		{
			if (arg is not string script)
				return;

			handler.PlatformView?.Eval(webView, script);
		}

		void NavigationSucceeded(WebView2 sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			Uri uri = sender.Source;

			if (uri != null)
				SendNavigated(uri.AbsoluteUri, CurrentWebNavigationEvent, WebNavigationResult.Success);

			if (VirtualView == null)
				return;

			sender.UpdateCanGoBackForward(VirtualView);
		}

		void NavigationFailed(WebView2 sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			Uri uri = sender.Source;

			if (uri != null)
				SendNavigated(uri.AbsoluteUri, CurrentWebNavigationEvent, WebNavigationResult.Failure);
		}

		void SendNavigated(string url, WebNavigationEvent evnt, WebNavigationResult result)
		{
			if (VirtualView != null)
			{
				VirtualView.Navigated(evnt, url, result);

				NativeView?.UpdateGoForward(VirtualView);
			}

			CurrentWebNavigationEvent = WebNavigationEvent.NewPage;
		}

		public static void MapEvaluateJavaScriptAsync(WebViewHandler handler, IWebView webView, object? arg) 
		{
			if (arg is EvaluateJavaScriptAsyncRequest request)
			{
				if (handler.PlatformView == null)
				{ 
					request.SetCanceled();
					return;
				}

				handler.PlatformView.EvaluateJavaScript(request);
			}
		}
	}
}