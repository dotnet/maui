#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using WebKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_EvaluateJavaScriptAsync
{
	[Fact]
	public async Task FailedEvaluationCanBeCaughtByTheCaller()
	{
		using var error = new NSError(
			new NSString("WKErrorDomain"),
			5,
			new NSDictionary(
				NSError.LocalizedDescriptionKey,
				new NSString("JavaScript execution returned a result of an unsupported type")));

		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<HybridWebView>(_ => new FailingHybridWebViewHandler(error));
			});
		});

		var hybridWebView = new HybridWebView
		{
			WidthRequest = 100,
			HeightRequest = 100,
		};

		await AttachAndRun(hybridWebView, async _ =>
		{
			var exception = await Assert.ThrowsAsync<NSErrorException>(
				() => hybridWebView.EvaluateJavaScriptAsync("1+1").WaitAsync(TimeSpan.FromSeconds(5)));

			Assert.Equal("WKErrorDomain", exception.Error.Domain);
			Assert.Equal((nint)5, exception.Error.Code);
		});
	}

	sealed class FailingHybridWebViewHandler : HybridWebViewHandler
	{
		readonly NSError _error;

		public FailingHybridWebViewHandler(NSError error)
		{
			_error = error;
		}

		protected override WKWebView CreatePlatformView() =>
			new FailingMauiHybridWebView(this, CGRect.Empty, new WKWebViewConfiguration(), _error);
	}

	sealed class FailingMauiHybridWebView : MauiHybridWebView
	{
		readonly NSError _error;

		public FailingMauiHybridWebView(HybridWebViewHandler handler, CGRect frame, WKWebViewConfiguration configuration, NSError error) : base(handler, frame, configuration)
		{
			_error = error;
		}

		public override void EvaluateJavaScript(NSString javaScriptString, [AllowNull] WKJavascriptEvaluationResult completionHandler)
		{
			completionHandler?.Invoke(null, _error);
		}
	}
}
