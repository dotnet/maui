using System;
using System.Threading.Tasks;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.Platform
{
	public static class WebViewExtensions
	{
		public static void UpdateSource(this AWebView nativeWebView, IWebView webView)
		{
			nativeWebView.UpdateSource(webView, null);
		}

		public static void UpdateSource(this AWebView nativeWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
		{
			if (webViewDelegate != null)
			{
				webView.Source?.Load(webViewDelegate);

				nativeWebView.UpdateCanGoBackForward(webView);
			}
		}

		public static void UpdateSettings(this AWebView nativeWebView, IWebView webView, bool javaScriptEnabled, bool domStorageEnabled)
		{
			if (nativeWebView.Settings == null)
				return;

			nativeWebView.Settings.JavaScriptEnabled = javaScriptEnabled;
			nativeWebView.Settings.DomStorageEnabled = domStorageEnabled;
		}

		public static void Eval(this AWebView nativeWebView, IWebView webView, string script)
		{
			string source = "javascript:" + script;

			nativeWebView.LoadUrl(source);
		}

		public static void UpdateGoBack(this AWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoBack())
				nativeWebView.GoBack();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateGoForward(this AWebView nativeWebView, IWebView webView)
		{
			if (nativeWebView == null)
				return;

			if (nativeWebView.CanGoForward())
				nativeWebView.GoForward();

			nativeWebView.UpdateCanGoBackForward(webView);
		}

		public static void UpdateReload(this AWebView nativeWebView, IWebView webView)
		{
			// TODO: Sync Cookies

			nativeWebView.Reload();
		}

		internal static void UpdateCanGoBackForward(this AWebView nativeWebView, IWebView webView)
		{
			if (webView == null || nativeWebView == null)
				return;

			webView.CanGoBack = nativeWebView.CanGoBack();
			webView.CanGoForward = nativeWebView.CanGoForward();
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