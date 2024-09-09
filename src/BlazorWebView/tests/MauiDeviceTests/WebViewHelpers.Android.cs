using System;
using System.Threading.Tasks;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static partial class WebViewHelpers
	{
		public static async Task WaitForWebViewReady(AWebView webview)
		{
			await Retry(async () =>
			{
				var blazorObject = await ExecuteScriptAsync(webview, "(window.Blazor !== null) && (window.__BlazorStarted === true)");
				return blazorObject == "true";
			}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get window.Blazor to be non-null *and* have window.__BlazorStarted to be true.")));
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

			await Retry(async () =>
			{
				latestControlValue = await ExecuteScriptAsync(webView, "document.getElementById('controlDiv').innerText");
				return latestControlValue == quotedExpectedValue;
			}, createExceptionWithTimeoutMS: async (int timeoutInMS) =>
			{
				var documentHtmlJavaScriptEncoded = await ExecuteScriptAsync(webView, "document.body.innerHTML");
				var documentHtmlString = System.Text.Json.JsonSerializer.Deserialize<string>(documentHtmlJavaScriptEncoded);

				return new Exception($"Waited {timeoutInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'. Most recent value was '{latestControlValue}'. document.body.innerHTML = {documentHtmlString}");
			});
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
