using System.Threading.Tasks;
using AppKit;
using Foundation;
using WebKit;

namespace System.Maui.Platform
{
	public partial class WebViewRenderer : AbstractViewRenderer<IWebView, WebView>, IWebViewDelegate
	{
		bool _ignoreSourceChanges;
		WebNavigationEvent _lastBackForwardEvent;
		WebNavigationEvent _lastEvent;

		protected override WebView CreateView()
		{
			var webView = new WebView
			{
				AutoresizingMask = NSViewResizingMask.WidthSizable,
				AutoresizesSubviews = true
			};

			webView.FrameLoadDelegate = new MauiWebFrameDelegate(this);

			return webView;
		}

		protected override void DisposeView(WebView webView)
		{
			VirtualView.EvalRequested -= OnEvalRequested;
			VirtualView.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
			VirtualView.GoBackRequested -= OnGoBackRequested;
			VirtualView.GoForwardRequested -= OnGoForwardRequested;
			VirtualView.ReloadRequested -= OnReloadRequested;

			base.DisposeView(webView);
		}

		public static void MapPropertySource(IViewRenderer renderer, IWebView webView)
		{
			(renderer as WebViewRenderer)?.Load();
		}

		public void LoadHtml(string html, string baseUrl)
		{
			if (html != null)
				TypedNativeView.MainFrame.LoadHtmlString(html,
					baseUrl == null ? new NSUrl(NSBundle.MainBundle.BundlePath, true) : new NSUrl(baseUrl, true));
		}

		public void LoadUrl(string url)
		{
			TypedNativeView.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(url)));
		}

		protected internal void UpdateCanGoBackForward()
		{
			VirtualView.CanGoBack = TypedNativeView.CanGoBack();
			VirtualView.CanGoForward = TypedNativeView.CanGoForward();
		}

		void Load()
		{
			if (_ignoreSourceChanges)
				return;

			VirtualView?.Source?.Load(this);

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			TypedNativeView?.StringByEvaluatingJavaScriptFromString(eventArg?.Script);
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			var tcr = new TaskCompletionSource<string>();

			var task = tcr.Task;

			tcr.SetResult(TypedNativeView?.StringByEvaluatingJavaScriptFromString(script));

			return await task.ConfigureAwait(false);
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoBack())
			{
				_lastBackForwardEvent = WebNavigationEvent.Back;
				TypedNativeView.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoForward())
			{
				_lastBackForwardEvent = WebNavigationEvent.Forward;
				TypedNativeView.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			TypedNativeView.Reload(TypedNativeView);
		}

		internal class MauiWebFrameDelegate : WebFrameLoadDelegate
		{
			readonly WebViewRenderer _webViewRenderer;

			internal MauiWebFrameDelegate(WebViewRenderer webViewRenderer)
			{
				_webViewRenderer = webViewRenderer;
			}

			public override void FinishedLoad(WebView sender, WebFrame forFrame)
			{
				if (_webViewRenderer.TypedNativeView.IsLoading)
					return;

				if (_webViewRenderer.TypedNativeView.MainFrameUrl == $"file://{NSBundle.MainBundle.BundlePath}/")
					return;

				_webViewRenderer._ignoreSourceChanges = true;
				_webViewRenderer.VirtualView.Source = new UrlWebViewSource { Url = _webViewRenderer.TypedNativeView.MainFrameUrl };
				_webViewRenderer._ignoreSourceChanges = false;

				_webViewRenderer._lastEvent = _webViewRenderer._lastBackForwardEvent;
				_webViewRenderer.VirtualView?.Navigated(new WebNavigatedEventArgs(_webViewRenderer._lastEvent, _webViewRenderer.VirtualView?.Source, _webViewRenderer.TypedNativeView.MainFrameUrl, WebNavigationResult.Success));

				_webViewRenderer.UpdateCanGoBackForward();
			}

			public override void FailedLoadWithError(WebView sender, NSError error, WebFrame forFrame)
			{
				_webViewRenderer._lastEvent = _webViewRenderer._lastBackForwardEvent;

				_webViewRenderer.VirtualView?.Navigated(new WebNavigatedEventArgs(_webViewRenderer._lastEvent, new UrlWebViewSource { Url = _webViewRenderer.TypedNativeView.MainFrameUrl }, _webViewRenderer.TypedNativeView.MainFrameUrl, WebNavigationResult.Failure));

				_webViewRenderer.UpdateCanGoBackForward();
			}
		}
	}
}