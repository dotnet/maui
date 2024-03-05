using System;
using WebKit;

namespace Microsoft.Maui.Platform;

public static partial class WebviewExtensions
{

	public static void UpdateSource(this MauiWebView platformWebView, IWebView webView)
	{
		platformWebView.UpdateSource(webView, null);
	}

	public static void MapWebViewSettings(IWebViewHandler handler, IWebView webView)
	{
		handler.PlatformView.UpdateSettings(webView, true, true);
	}

	public static void UpdateSource(this MauiWebView platformWebView, IWebView webView, IWebViewDelegate? webViewDelegate)
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
			platformWebView.LoadUri("about:blank");
		}
	}

	public static void UpdateSettings(this MauiWebView platformWebView, IWebView webView, bool javaScriptEnabled, bool html5DomStorageEnabled)
	{
		if (platformWebView.Settings == null)
			return;

		platformWebView.Settings.EnableJavascript = javaScriptEnabled;
		platformWebView.Settings.EnableHtml5LocalStorage = html5DomStorageEnabled;

	}

	public static void UpdateUserAgent(this MauiWebView platformWebView, IWebView webView)
	{
		if (platformWebView.Settings == null)
			return;

		if (webView.UserAgent != null)
			platformWebView.Settings.UserAgent = webView.UserAgent;
		else
			webView.UserAgent = platformWebView.Settings.UserAgent;
	}

	public static void Eval(this MauiWebView platformWebView, IWebView webView, string script)
	{
		string source = "javascript:" + script;

		platformWebView.LoadUri(source);
	}

	public static void UpdateGoBack(this MauiWebView platformWebView, IWebView webView)
	{
		if (platformWebView == null)
			return;

		if (platformWebView.CanGoBack())
			platformWebView.GoBack();

		platformWebView.UpdateCanGoBackForward(webView);
	}

	public static void UpdateGoForward(this MauiWebView platformWebView, IWebView webView)
	{
		if (platformWebView == null)
			return;

		if (platformWebView.CanGoForward())
			platformWebView.GoForward();

		platformWebView.UpdateCanGoBackForward(webView);
	}

	public static void UpdateReload(this MauiWebView platformWebView, IWebView webView)
	{
		platformWebView.Reload();
	}

	internal static void UpdateCanGoBackForward(this MauiWebView platformWebView, IWebView webView)
	{
		if (webView == null || platformWebView == null)
			return;

		webView.CanGoBack = platformWebView.CanGoBack();
		webView.CanGoForward = platformWebView.CanGoForward();
	}

	public static void EvaluateJavaScript(this MauiWebView webView, EvaluateJavaScriptAsyncRequest request)
	{
		try
		{
			webView.RunJavascript(request.Script, null, (sender, res) =>
			{
				if (sender is not WebView view)
				{
					request.SetException(new ArgumentException($"{nameof(WebView)} is null"));

					return;
				}

				try
				{
					var javascriptResult = view.RunJavascriptFinish(res);

					if (javascriptResult.JsValue is not { } jsValue)
						return;

					if (jsValue.IsString)
						request.SetResult(jsValue.ToString());
					else
						request.SetException(new ArgumentException($"script {request.Script} is not {nameof(jsValue.IsString)}"));

				}
				catch (Exception exception)
				{
					request.SetException(exception);
				}
			});

		}
		catch (Exception ex)
		{
			request.SetException(ex);
		}
	}

}