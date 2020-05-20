using System.Threading.Tasks;
using Android.Webkit;
using Android.Widget;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;

namespace System.Maui.Platform
{
	public partial class WebViewRenderer : AbstractViewRenderer<IWebView, AWebView>, IWebViewDelegate
	{
		public const string AssetBaseUrl = "file:///android_asset/";

		// readonly bool _ignoreSourceChanges;
		WebNavigationEvent _eventState;
		WebViewClient _webViewClient;
		WebChromeClient _webChromeClient;

		protected internal string UrlCanceled { get; set; }

		protected override AWebView CreateView()
		{
			var aWebView = new AWebView(Context)
			{
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
				LayoutParameters = new AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0)
#pragma warning restore 618
			};

			aWebView.Settings.JavaScriptEnabled = true;
			aWebView.Settings.DomStorageEnabled = true;

			_webViewClient = GetWebViewClient();
			aWebView.SetWebViewClient(_webViewClient);

			_webChromeClient = GetWebChromeClient();
			aWebView.SetWebChromeClient(_webChromeClient);

			VirtualView.EvalRequested += OnEvalRequested;
			VirtualView.EvaluateJavaScriptRequested += OnEvaluateJavaScriptRequested;
			VirtualView.GoBackRequested += OnGoBackRequested;
			VirtualView.GoForwardRequested += OnGoForwardRequested;
			VirtualView.ReloadRequested += OnReloadRequested;

			return aWebView;
		}

		protected override void DisposeView(AWebView aWebView)
		{
			VirtualView.EvalRequested -= OnEvalRequested;
			VirtualView.EvaluateJavaScriptRequested -= OnEvaluateJavaScriptRequested;
			VirtualView.GoBackRequested -= OnGoBackRequested;
			VirtualView.GoForwardRequested -= OnGoForwardRequested;
			VirtualView.ReloadRequested -= OnReloadRequested;

			aWebView.StopLoading();

			_webViewClient?.Dispose();
			_webChromeClient?.Dispose();

			base.DisposeView(aWebView);
		}

		public static void MapPropertySource(IViewRenderer renderer, IWebView webView)
		{
			(renderer as WebViewRenderer)?.Load();
		}

		public void LoadHtml(string html, string baseUrl)
		{
			_eventState = WebNavigationEvent.NewPage;
			TypedNativeView.LoadDataWithBaseURL(baseUrl ?? AssetBaseUrl, html, "text/html", "UTF-8", null);
		}

		public void LoadUrl(string url)
		{
			if (!SendNavigatingCanceled(url))
			{
				_eventState = WebNavigationEvent.NewPage;
				TypedNativeView.LoadUrl(url);
			}
		}

		protected virtual WebViewClient GetWebViewClient()
		{
			return new WebViewClient();
		}

		protected virtual WebChromeClient GetWebChromeClient()
		{
			return new WebChromeClient();
		}

		protected internal void UpdateCanGoBackForward()
		{
			VirtualView.CanGoBack = TypedNativeView.CanGoBack();
			VirtualView.CanGoForward = TypedNativeView.CanGoForward();
		}

		protected internal bool SendNavigatingCanceled(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return true;

			if (url == AssetBaseUrl)
				return false;

			var args = new WebNavigatingEventArgs(_eventState, new UrlWebViewSource { Url = url }, url);
			VirtualView.Navigating(args);
			UpdateCanGoBackForward();
			UrlCanceled = args.Cancel ? null : url;
			return args.Cancel;
		}

		void Load()
		{
			//if (_ignoreSourceChanges)
			//	return;

			VirtualView.Source?.Load(this);

			UpdateCanGoBackForward();
		}

		void OnEvalRequested(object sender, EvalRequested eventArg)
		{
			LoadUrl("javascript:" + eventArg.Script);
		}

		async Task<string> OnEvaluateJavaScriptRequested(string script)
		{
			var jsr = new JavascriptResult();

			TypedNativeView.EvaluateJavascript(script, jsr);

			return await jsr.JsResult.ConfigureAwait(false);
		}

		void OnGoBackRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoBack())
			{
				_eventState = WebNavigationEvent.Back;
				TypedNativeView.GoBack();
			}

			UpdateCanGoBackForward();
		}

		void OnGoForwardRequested(object sender, EventArgs eventArgs)
		{
			if (TypedNativeView.CanGoForward())
			{
				_eventState = WebNavigationEvent.Forward;
				TypedNativeView.GoForward();
			}

			UpdateCanGoBackForward();
		}

		void OnReloadRequested(object sender, EventArgs eventArgs)
		{
			_eventState = WebNavigationEvent.Refresh;
			TypedNativeView.Reload();
		}

		class JavascriptResult : Java.Lang.Object, IValueCallback
		{
			readonly TaskCompletionSource<string> _source;
			public Task<string> JsResult { get { return _source.Task; } }

			public JavascriptResult()
			{
				_source = new TaskCompletionSource<string>();
			}

			public void OnReceiveValue(Java.Lang.Object result)
			{
				string json = ((Java.Lang.String)result).ToString();
				_source.SetResult(json);
			}
		}
	}
}