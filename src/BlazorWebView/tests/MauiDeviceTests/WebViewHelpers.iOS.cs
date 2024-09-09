using System;
using System.Threading.Tasks;
using WebKit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static partial class WebViewHelpers
	{
		public static async Task WaitForWebViewReady(WKWebView webview)
		{
			await Retry(async () =>
			{
				var blazorObject = await ExecuteScriptAsync(webview, "(window.Blazor !== null).toString()");
				return blazorObject == "true";
			}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get window.Blazor to be non-null.")));
		}

		public static async Task<string> ExecuteScriptAsync(WKWebView webview, string script)
		{
			var nsStringResult = await webview.EvaluateJavaScriptAsync(script);
			return nsStringResult?.ToString();
		}

		public static async Task WaitForControlDiv(WKWebView webView, string controlValueToWaitFor)
		{
			var latestControlValue = "<no value yet>";

			await Retry(async () =>
			{
				latestControlValue = await ExecuteScriptAsync(webView, "(document.getElementById('controlDiv') === null ? null : document.getElementById('controlDiv').innerText)");
				return latestControlValue == controlValueToWaitFor;
			}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'. Most recent value was '{latestControlValue}'.")));
		}
	}
}
