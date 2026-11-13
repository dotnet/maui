#nullable enable
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Web.WebView2.Core;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_MessageOrigins
{
	private static partial async Task LoadOtherOriginDocumentAsync(
		HybridWebViewHandler handler,
		HybridWebView hybridWebView,
		string html)
	{
		var responseProvided = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var navigationCompleted = new TaskCompletionSource<CoreWebView2NavigationCompletedEventArgs>(
			TaskCreationOptions.RunContinuationsAsynchronously);

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

		void OnNavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args) =>
			navigationCompleted.TrySetResult(args);

		hybridWebView.WebResourceRequested += OnWebResourceRequested;
		handler.PlatformView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

		try
		{
			handler.PlatformView.CoreWebView2.Navigate(OtherOrigin);

			await responseProvided.Task.WaitAsync(OperationWaitTimeout);
			var navigation = await navigationCompleted.Task.WaitAsync(OperationWaitTimeout);

			Assert.True(navigation.IsSuccess, $"Navigation failed with {navigation.WebErrorStatus}.");
		}
		finally
		{
			hybridWebView.WebResourceRequested -= OnWebResourceRequested;
			handler.PlatformView.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;
		}
	}
}
