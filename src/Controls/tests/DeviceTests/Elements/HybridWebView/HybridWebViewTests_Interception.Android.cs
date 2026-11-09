#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Webkit;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AUri = Android.Net.Uri;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_Interception
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public Task InFlightLocalResourceSurvivesDisconnect(bool disconnectHandler) =>
		RunTest("index.html", async hybridWebView =>
		{
			var handler = Assert.IsType<HybridWebViewHandler>(hybridWebView.Handler);
			var platformView = handler.PlatformView;
			var client = Assert.IsType<MauiHybridWebViewClient>(platformView.WebViewClient);
			var logger = new DisconnectingRequestLogger(() =>
				InvokeOnMainThreadAsync(() =>
				{
					if (disconnectHandler)
						((IElementHandler)handler).DisconnectHandler();
					else
						client.Disconnect();
				}).GetAwaiter().GetResult());
			var context = new MauiContext(handler.MauiContext!.Services, handler.Context);
			context.AddSpecific<ILogger<MauiHybridWebViewClient>>(logger);
			handler.SetMauiContext(context);

			await Task.Run(() =>
			{
				using var request = new DisconnectingResourceRequest();
				using var response = client.ShouldInterceptRequest(platformView, request);
				Assert.True(logger.Disconnected);
				Assert.NotNull(response);
				Assert.Equal(200, response.StatusCode);
				using var reader = new StreamReader(Assert.IsAssignableFrom<Stream>(response.Data));
				Assert.Contains("htmlLoaded", reader.ReadToEnd(), StringComparison.Ordinal);
			});
		});

	sealed class DisconnectingResourceRequest : Java.Lang.Object, IWebResourceRequest
	{
		public bool HasGesture => false;
		public bool IsForMainFrame => true;
		public bool IsRedirect => false;
		public string Method => "GET";
		public IDictionary<string, string> RequestHeaders { get; } = new Dictionary<string, string>();
		public AUri Url { get; } = AUri.Parse("https://0.0.0.1/?disconnect-during-request")!;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Url.Dispose();

			base.Dispose(disposing);
		}
	}

	sealed class DisconnectingRequestLogger(Action disconnect) : ILogger<MauiHybridWebViewClient>
	{
		int _disconnected;

		public bool Disconnected => Volatile.Read(ref _disconnected) != 0;
		public bool IsEnabled(LogLevel logLevel) => true;
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			var message = formatter(state, exception);
			if (message.Contains("disconnect-during-request", StringComparison.Ordinal) &&
				message.Contains("will be handled by .NET MAUI.", StringComparison.Ordinal) &&
				Interlocked.Exchange(ref _disconnected, 1) == 0)
			{
				disconnect();
			}
		}
	}
}
