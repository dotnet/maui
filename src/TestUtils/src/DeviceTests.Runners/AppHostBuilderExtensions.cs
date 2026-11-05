#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureTests(this MauiAppBuilder builder, TestOptions options) =>
			builder.ConfigureTests(_ => options);

		public static MauiAppBuilder ConfigureTests(this MauiAppBuilder builder, Func<IServiceProvider, TestOptions> options)
		{
			builder.Services.AddSingleton(options);
			builder.Logging.AddConsole();
			return builder;
		}

		public static MauiAppBuilder UseHeadlessRunner(this MauiAppBuilder builder, HeadlessRunnerOptions options)
		{
			builder.Services.AddSingleton(options);
			builder.Services.AddSingleton<HeadlessTestRunner>();
			builder.UseMauiApp<MauiTestApplication>();
			return builder;
		}
	}
}
