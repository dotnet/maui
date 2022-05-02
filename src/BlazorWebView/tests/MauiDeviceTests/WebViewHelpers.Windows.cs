using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static class WebViewHelpers
	{
		const int MaxWaitTimes = 30;
		const int WaitTimeInMS = 250;

		public static async Task WaitForWebViewReady(WebView2 wv2)
		{
			CoreWebView2 coreWebView2 = null;
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				coreWebView2 = wv2.CoreWebView2;
				if (coreWebView2 != null)
				{
					break;
				}
				await Task.Delay(WaitTimeInMS);
			}

			if (coreWebView2 == null)
			{
				throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get CoreWebView2 to be available.");
			}

			var domLoaded = false;
			var sem = new SemaphoreSlim(1);
			await sem.WaitAsync();
			wv2.CoreWebView2.DOMContentLoaded += (s, e) =>
			{
				domLoaded = true;
				sem.Release();
			};

			await Task.WhenAny(Task.Delay(1000), sem.WaitAsync());

			if (!domLoaded)
			{
				throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get CoreWebView2.DOMContentLoaded to complete.");
			}
			return;
		}

		public static async Task<string> ExecuteScriptAsync(WebView2 webView2, string script)
		{
			return await webView2.CoreWebView2.ExecuteScriptAsync(javaScript: script);
		}

		public static async Task WaitForControlDiv(WebView2 webView2, string controlValueToWaitFor)
		{
			var quotedExpectedValue = "\"" + controlValueToWaitFor + "\"";
			for (int i = 0; i < MaxWaitTimes; i++)
			{
				var controlValue = await ExecuteScriptAsync(webView2, "document.getElementById('controlDiv').innerText");
				if (controlValue == quotedExpectedValue)
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			throw new Exception($"Waited {MaxWaitTimes * WaitTimeInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'.");
		}
	}
}
