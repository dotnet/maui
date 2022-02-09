using System;
using System.Threading;
using System.Threading.Tasks;
using WebKit;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static class WebViewHelpers
	{
		const int MaxWaitTimes = 30;
		const int WaitTimeInMS = 250;

		public static async Task WaitForWebViewReady(WKWebView webview)
		{
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				var blazorObject = await ExecuteScriptAsync(webview, "(window.Blazor !== null).toString()");
				if (blazorObject == "true")
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get window.Blazor to be non-null.");
		}

		public static async Task<string> ExecuteScriptAsync(WKWebView webview, string script)
		{
			var nsStringResult = await webview.EvaluateJavaScriptAsync(script);
			return nsStringResult?.ToString();
		}

		public static async Task WaitForControlDiv(WKWebView webView, string controlValueToWaitFor)
		{
			var latestControlValue = "<no value yet>";
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				latestControlValue = await ExecuteScriptAsync(webView, "(document.getElementById('controlDiv') === null ? null : document.getElementById('controlDiv').innerText)");
				if (latestControlValue == controlValueToWaitFor)
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'. Most recent value was '{latestControlValue}'.");
		}
	}
}
