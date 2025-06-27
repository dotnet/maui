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
			await Retry(
				async () =>
				{
					var blazorObject = await ExecuteScriptAsync(webview, "(window.Blazor !== null) && (window.__BlazorStarted === true)");
					return blazorObject == "true";
				},
				timeoutInMS =>
				{
					return Task.FromResult(new Exception($"Waited {timeoutInMS}ms but couldn't get window.Blazor to be non-null *and* have window.__BlazorStarted to be true."));
				});
		}

		public static Task<string> ExecuteScriptAsync(AWebView webview, string script)
		{
			var jsResult = new JavascriptResult();
			webview.EvaluateJavascript(script, jsResult);
			return jsResult.JsResult;
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
