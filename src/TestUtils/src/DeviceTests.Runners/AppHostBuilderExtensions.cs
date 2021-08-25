#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureTests(this IAppHostBuilder appHostBuilder, TestOptions options)
		{
			appHostBuilder.ConfigureServices(services => services.AddSingleton(options));

			return appHostBuilder;
		}

		public static IAppHostBuilder UseVisualRunner(this IAppHostBuilder appHostBuilder)
		{
			appHostBuilder.UseMauiApp(svc => new MauiVisualRunnerApp(
				svc.GetRequiredService<TestOptions>(),
				svc.GetRequiredService<ILoggerFactory>().CreateLogger("TestRun")));

			return appHostBuilder;
		}

		public static IAppHostBuilder UseHeadlessRunner(this IAppHostBuilder appHostBuilder, HeadlessRunnerOptions options)
		{
			appHostBuilder.ConfigureServices(services =>
			{
				services.AddSingleton(options);

#if __ANDROID__ || __IOS__ || MACCATALYST
				services.AddTransient(svc => new HeadlessTestRunner(
					svc.GetRequiredService<HeadlessRunnerOptions>(),
					svc.GetRequiredService<TestOptions>()));
#endif
			});

			return appHostBuilder;
		}
	}
}