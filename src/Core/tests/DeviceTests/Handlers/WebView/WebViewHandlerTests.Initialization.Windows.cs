using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WebViewHandlerTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task FailedCoreWebView2InitializationLogsErrorWithoutThrowing(bool hasException)
		{
			var logger = new WebViewInitializationLogger();
			EnsureHandlerCreated(builder =>
				builder.Services.AddSingleton<ILogger<WebViewHandler>>(logger));

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(new WebViewStub());
				var platformView = handler.PlatformView;
				var exception = hasException ? new InvalidOperationException("WebView2 initialization failed.") : null;

				try
				{
					Assert.Null(platformView.CoreWebView2);
					logger.Entries.Clear();

					handler.ProcessCoreWebView2Initialized(platformView, exception);

					Assert.Null(platformView.CoreWebView2);
					var entry = Assert.Single(logger.Entries);
					Assert.Equal(LogLevel.Error, entry.Level);
					Assert.Same(exception, entry.Exception);
					Assert.Equal("Failed to initialize WebView2.", entry.Message);
				}
				finally
				{
					((IElementHandler)handler).DisconnectHandler();
					platformView.Close();
				}
			});
		}

		sealed class WebViewInitializationLogger : ILogger<WebViewHandler>
		{
			public List<(LogLevel Level, Exception Exception, string Message)> Entries { get; } = new();

			public IDisposable BeginScope<TState>(TState state) => null;

			public bool IsEnabled(LogLevel logLevel) => true;

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
				Entries.Add((logLevel, exception, formatter(state, exception)));
		}
	}
}
