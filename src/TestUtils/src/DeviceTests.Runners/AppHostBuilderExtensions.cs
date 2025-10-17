#nullable enable
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Services;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureTests(this MauiAppBuilder appHostBuilder, TestOptions options)
		{
			appHostBuilder.Services.AddSingleton(options);

			appHostBuilder.Logging.AddConsole();
			// appHostBuilder.Logging.SetMinimumLevel(LogLevel.Debug);
			return appHostBuilder;
		}

		public static MauiAppBuilder ConfigureTests(this MauiAppBuilder appHostBuilder, Func<IServiceProvider, TestOptions> options)
		{
			appHostBuilder.Services.AddSingleton(options);

			appHostBuilder.Logging.AddConsole();
			// appHostBuilder.Logging.SetMinimumLevel(LogLevel.Debug);
			return appHostBuilder;
		}

		public static MauiAppBuilder UseVisualRunner(this MauiAppBuilder appHostBuilder)
		{
			appHostBuilder.UseMauiApp(svc => new MauiVisualRunnerApp(
				svc.GetRequiredService<TestOptions>(),
				svc.GetRequiredService<ILoggerFactory>().CreateLogger("TestRun")));

			return appHostBuilder;
		}

		public static MauiAppBuilder UseHeadlessRunner(this MauiAppBuilder appHostBuilder, HeadlessRunnerOptions options)
		{
			appHostBuilder.Services.AddSingleton(options);

#if __ANDROID__ || __IOS__ || MACCATALYST || WINDOWS
			appHostBuilder.Services.AddTransient(svc => new HeadlessTestRunner(
					svc.GetRequiredService<HeadlessRunnerOptions>(),
					svc.GetRequiredService<TestOptions>()));
#endif

			appHostBuilder.Logging.AddConsole();

			appHostBuilder.Services.AddTransient(svc => new MicrosoftPlatformEntryPoint());

			return appHostBuilder;
		}

#if WINDOWS
		public static MauiAppBuilder UseControlsHeadlessRunner(this MauiAppBuilder appHostBuilder, HeadlessRunnerOptions options)
		{
			appHostBuilder.Services.AddSingleton(options);

			appHostBuilder.Services.AddTransient(svc => new ControlsHeadlessTestRunner(
					svc.GetRequiredService<HeadlessRunnerOptions>(),
					svc.GetRequiredService<TestOptions>()));

			return appHostBuilder;
		}
#endif
	}


	public static class TestingFrameworkExtensions
	{
	
	}
}