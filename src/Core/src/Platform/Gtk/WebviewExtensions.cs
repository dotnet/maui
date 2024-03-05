using System;
using WebKit;

namespace Microsoft.Maui.Platform;

public static partial class WebviewExtensions
{

	public static void UpdateSource(this MauiWebView platformWebView, IWebView webView)
	{
		platformWebView.UpdateSource(webView, null);
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

	public static void UpdateSettings(this MauiWebView platformWebView, IWebView webView, bool javaScriptEnabled, bool domStorageEnabled)
	{
		if (platformWebView.Settings == null)
			return;

		// platformWebView.Settings.JavaScriptEnabled = javaScriptEnabled;
		// platformWebView.Settings.DomStorageEnabled = domStorageEnabled;
	}

	public static void UpdateUserAgent(this MauiWebView platformWebView, IWebView webView)
	{
		if (platformWebView.Settings == null)
			return;

		// if (webView.UserAgent != null)
		// 	platformWebView.Settings.UserAgentString = webView.UserAgent;
		// else
		// 	webView.UserAgent = platformWebView.Settings.UserAgentString;
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
			// var javaScriptResult = new JavascriptResult();
			// webView.EvaluateJavascript(request.Script, javaScriptResult);
			// request.RunAndReport(javaScriptResult.JsValue);
		}
		catch (Exception ex)
		{
			request.SetException(ex);
		}
	}

}