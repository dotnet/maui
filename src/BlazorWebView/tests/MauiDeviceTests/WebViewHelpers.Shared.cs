#nullable enable
using System;
using System.Threading.Tasks;
#if ANDROID
using PlatformWebView = Android.Webkit.WebView;
#elif IOS || MACCATALYST
using PlatformWebView = WebKit.WKWebView;
#else
using PlatformWebView = Microsoft.Maui.Controls.WebView;
#endif

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static partial class WebViewHelpers
	{
		const int MaxWaitTimes = 30;
		const int WaitTimeInMS = 1000;

		private static async Task Retry(Func<Task<bool>> tryAction, Func<int, Task<Exception>> createExceptionWithTimeoutMS)
		{
			for (var i = 0; i < MaxWaitTimes; i++)
			{
				if (await tryAction())
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			throw await createExceptionWithTimeoutMS(MaxWaitTimes * WaitTimeInMS);
		}

		/// <summary>
		/// Executes an async JavaScript function body and waits for the result to be stored in controlDiv.
		/// This method handles all the boilerplate for script injection and Promise avoidance.
		/// </summary>
		/// <param name="webView">The WKWebView instance</param>
		/// <param name="asyncFunctionBody">The body of the async function (without function wrapper)</param>
		/// <returns>The result stored in controlDiv after the async operation completes</returns>
		public static async Task<string?> ExecuteAsyncScriptAndWaitForResult(PlatformWebView webView, string asyncFunctionBody)
		{
			// Inject script that executes the async function and stores result in controlDiv
			await ExecuteScriptAsync(webView,
				$$"""
				(function() {
					var script = document.createElement('script');
					script.textContent = `
						(async function() {
							try {
								const result = await (async function() {
									{{asyncFunctionBody}}
								})();
								document.getElementById('controlDiv').innerText = result;
							} catch (error) {
								document.getElementById('controlDiv').innerText = JSON.stringify({error: error.message});
							}
						})();
					`;
					document.head.appendChild(script);
					return true;
				})()
				""");

			// Wait for the async operation to complete and get the result
			return await WaitForControlDivToChangeFrom(webView, "Static");
		}

		public static async Task WaitForControlDiv(PlatformWebView webView, string controlValueToWaitFor)
		{
			await Retry(
				async () =>
				{
					var controlValue = await GetControlDivValue(webView);

					return
						controlValue == controlValueToWaitFor ||
						controlValue == $"\"{controlValueToWaitFor}\"";
				},
				timeoutInMS =>
				{
					return Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get controlDiv to have value '{controlValueToWaitFor}'."));
				});
		}

		public static async Task<string?> WaitForControlDivToChangeFrom(PlatformWebView webView, string valueToChangeFrom)
		{
			string? latestControlValue = null;

			await Retry(
				async () =>
				{
					var controlValue = await GetControlDivValue(webView);
					latestControlValue = controlValue;

					return
						!string.IsNullOrEmpty(controlValue) &&
						controlValue != "null" &&
						controlValue != valueToChangeFrom &&
						controlValue != $"\"{valueToChangeFrom}\"";
				},
				timeoutInMS =>
				{
					return Task.FromResult(new Exception($"Waited {timeoutInMS}ms but controlDiv never changed from value '{valueToChangeFrom}'."));
				});

			// Remove quotes that some platforms add
			return latestControlValue?.Trim('\"');
		}

		private static Task<string?> GetControlDivValue(PlatformWebView webView)
		{
			return ExecuteScriptAsync(webView,
				"""
				(document.getElementById('controlDiv') === null
					? null
					: document.getElementById('controlDiv').innerText)
				""");
		}
	}
}
