using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Util;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static class WebViewHelpers
	{
		const int MaxWaitTimes = 30;
		const int WaitTimeInMS = 250;

		public static async Task WaitForWebViewReady(AWebView webview)
		{
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				var blazorObject = await ExecuteScriptAsync(webview, "(window.Blazor !== null) && (window.__BlazorStarted === true)");
				if (blazorObject == "true")
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get window.Blazor to be non-null *and* have window.__BlazorStarted to be true.");
		}

		public static Task<string> ExecuteScriptAsync(AWebView webview, string script)
		{
			var jsResult = new JavascriptResult();
			webview.EvaluateJavascript(script, jsResult);
			return jsResult.JsResult;
		}

		public static async Task WaitForControlDiv(AWebView webView, string controlValueToWaitFor)
		{
			var quotedExpectedValue = "\"" + controlValueToWaitFor + "\"";
			var latestControlValue = "<no value yet>";
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				latestControlValue = await ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText");
				if (latestControlValue == quotedExpectedValue)
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			var documentHtmlJavaScriptEncoded = await ExecuteScriptAsync(webView, "document.body.innerHTML");
			var documentHtmlString = System.Text.Json.JsonSerializer.Deserialize<string>(documentHtmlJavaScriptEncoded);

			throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'. Most recent value was '{latestControlValue}'. document.body.innerHTML = {documentHtmlString}");
		}

		class JavascriptResult : Java.Lang.Object, IValueCallback
		{
			TaskCompletionSource<string> source;
			public Task<string> JsResult { get { return source.Task; } }

			public JavascriptResult()
			{
				source = new TaskCompletionSource<string>();
			}

			public void OnReceiveValue(Java.Lang.Object result)
			{
				string json = ((Java.Lang.String)result).ToString();
				source.SetResult(json);
			}
		}
	}
}
