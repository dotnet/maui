using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static partial class WebViewHelpers
	{
		public static async Task WaitForWebViewReady(WebView2 wv2)
		{
			CoreWebView2 coreWebView2 = null;

			await Retry(() =>
			{
				coreWebView2 = wv2.CoreWebView2;
				return Task.FromResult(coreWebView2 != null);
			}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get CoreWebView2 to be available.")));

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
				// It's possible that the DOMContentLoaded event won't fire because it's already loaded by the time we got here. To check
				// for that, we inspect an arbitrary custom HTML element attribute to see if we can find it. If we can find it, then surely
				// the DOM content is loaded, so we can continue with the test.

				await Retry(async () =>
				{
					var testHtmlLoadedAttributeValue = await wv2.CoreWebView2.ExecuteScriptAsync("(document.head.attributes['testhtmlloaded']?.value === 'true')");

					// If the event didn't fire, AND we couldn't find the custom HTML element attribute, then the test content didn't load
					return testHtmlLoadedAttributeValue == "true";
				}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get CoreWebView2.DOMContentLoaded to complete.")));
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

			await Retry(async () =>
			{
				var controlValue = await ExecuteScriptAsync(webView2, "document.getElementById('controlDiv').innerText");
				return controlValue == quotedExpectedValue;
			}, createExceptionWithTimeoutMS: (int timeoutInMS) => Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'.")));
		}
	}
}
