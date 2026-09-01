#nullable enable
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_MessageOrigins
{
	private static partial async Task LoadOtherOriginDocumentAsync(
		HybridWebViewHandler handler,
		HybridWebView hybridWebView,
		string html)
	{
		var responseProvided = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		void OnWebResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs args)
		{
			if (!new Uri(OtherOrigin).IsBaseOf(args.Uri))
			{
				return;
			}

			if (args.Uri == new Uri(OtherOrigin))
			{
				args.SetResponse(200, "OK", "text/html", new MemoryStream(Encoding.UTF8.GetBytes(html)));
				responseProvided.TrySetResult();
			}
			else
			{
				args.SetResponse(404, "Not Found");
			}

			args.Handled = true;
		}

		hybridWebView.WebResourceRequested += OnWebResourceRequested;

		try
		{
			handler.PlatformView.LoadUrl(OtherOrigin);
			await responseProvided.Task.WaitAsync(TimeSpan.FromSeconds(5));

			for (var attempt = 0; attempt < 20; attempt++)
			{
				var loaded = await hybridWebView.EvaluateJavaScriptAsync(
					"document.getElementById('otherOriginDocument') !== null");
				if (loaded == "true")
				{
					return;
				}

				await Task.Delay(100);
			}

			throw new TimeoutException("The other-origin document did not finish loading.");
		}
		finally
		{
			hybridWebView.WebResourceRequested -= OnWebResourceRequested;
		}
	}

	private static partial async Task WaitForBridgeAttemptAsync(HybridWebView hybridWebView)
	{
		var bridgeAttempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

		void OnWebResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs args)
		{
			if (args.Uri.Host == HybridWebViewHandler.AppOriginUri.Host &&
				args.Uri.AbsolutePath == $"/{HybridWebViewHandler.SendMessagePath}")
			{
				bridgeAttempted.TrySetResult();
			}
		}

		hybridWebView.WebResourceRequested += OnWebResourceRequested;

		try
		{
			await bridgeAttempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}
		finally
		{
			hybridWebView.WebResourceRequested -= OnWebResourceRequested;
		}
	}
}
