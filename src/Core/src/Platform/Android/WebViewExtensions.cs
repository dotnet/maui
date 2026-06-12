using System;
using System.Threading.Tasks;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this AWebView platformWebView, IWebView webView)
		{
			platformWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this AWebView platformWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			IWebViewSource? source = webView.Source;

			if (source != null)
			{
				if (webViewDelegate != null)
				{
					source.Load(webViewDelegate);

					platformWebView.UpdateCanGoBackForward(webView);
				}
			}
			else
			{
				// Load about:blank to constrain the WebView within its layout bounds when source
				// is null, preventing overflow in grid cells (#32030). Flag IsLoadingForLayout so
				// MauiWebViewClient can remove this synthetic entry from the native back stack
				// once the real URL loads, fixing CanGoBack() returning true unexpectedly (#35788).
				// Guard: skip when real history exists so ClearHistory() does not destroy back entries.
				if (platformWebView is MauiWebView mauiWebView && !platformWebView.CanGoBack() && !platformWebView.CanGoForward())
				{
					mauiWebView.IsLoadingForLayout = true;
					platformWebView.LoadUrl("about:blank");
				}
			}
		}

		public static void UpdateSettings(this AWebView platformWebView, IWebView webView, bool javaScriptEnabled, bool domStorageEnabled)
		{
			if (platformWebView.Settings == null)
				return;

			platformWebView.Settings.JavaScriptEnabled = javaScriptEnabled;
			platformWebView.Settings.DomStorageEnabled = domStorageEnabled;
		}

		public static void UpdateUserAgent(this AWebView platformWebView, IWebView webView)
		{
			if (platformWebView.Settings == null)
				return;

			if (webView.UserAgent != null)
				platformWebView.Settings.UserAgentString = webView.UserAgent;
			else
				webView.UserAgent = platformWebView.Settings.UserAgentString;
		}

		public static void Eval(this AWebView platformWebView, IWebView webView, string script)
		{
			string source = "javascript:" + script;

			platformWebView.LoadUrl(source);
		}

		public static void UpdateGoBack(this AWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CanGoBack())
				platformWebView.GoBack();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this AWebView platformWebView, IWebView webView)
		{
			if (platformWebView == null)
				return;

			if (platformWebView.CanGoForward())
				platformWebView.GoForward();

			platformWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this AWebView platformWebView, IWebView webView)
		{
			platformWebView.Reload();
		}

		internal static void UpdateCanGoBackForward(this AWebView platformWebView, IWebView webView)
		{
			if (webView == null || platformWebView == null)
				return;

			webView.CanGoBack = platformWebView.CanGoBack();
			webView.CanGoForward = platformWebView.CanGoForward();
		}

		public static void EvaluateJavaScript(this AWebView webView, EvaluateJavaScriptAsyncRequest request)
		{
			try
			{
				var javaScriptResult = new JavascriptResult();
				webView.EvaluateJavascript(request.Script, javaScriptResult);
				request.RunAndReport(javaScriptResult.JsResult);
			}
			catch (Exception ex)
			{
				request.SetException(ex);
			}
		}

		class JavascriptResult : Java.Lang.Object, IValueCallback
		{
			readonly TaskCompletionSource<string> _source;
			public Task<string> JsResult => _source.Task;

			public JavascriptResult()
			{
				_source = new TaskCompletionSource<string>();
			}

			public void OnReceiveValue(Java.Lang.Object? result)
			{
				if (result == null)
				{
					_source.SetResult("null");
					return;
				}

				string json = ((Java.Lang.String)result).ToString();
				_source.SetResult(json);
			}
		}
	}
}