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
		public static async Task WaitForWebViewReady(AWebView webview)
		{
			const int MaxWaitTimes = 10;
			const int WaitTimeInMS = 200;
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				var blazorObject = await ExecuteScriptAsync(webview, "window.Blazor !== null");
				if (blazorObject == "true")
				{
					Log.Warn("eilon", $"FOUND BLAZOBJ: {blazorObject}");
					return;
				}
				Log.Warn("blazorwebview", $"window.Blazor not found, waiting {WaitTimeInMS}ms...");
				await Task.Delay(WaitTimeInMS);
			}

			throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get window.Blazor to be non-null.");
		}

		public static Task<string> ExecuteScriptAsync(AWebView webview, string script)
		{
			var jsResult = new JavascriptResult();
			webview.EvaluateJavascript(script, jsResult);
			return jsResult.JsResult;
		}

		public static async Task WaitForControlDiv(AWebView webView, string controlValueToWaitFor)
		{
			const int MaxWaitTimes = 10;
			const int WaitTimeInMS = 200;
			var quotedExpectedValue = "\"" + controlValueToWaitFor + "\"";
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				var controlValue = await ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText");
				if (controlValue == quotedExpectedValue)
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'.");
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
