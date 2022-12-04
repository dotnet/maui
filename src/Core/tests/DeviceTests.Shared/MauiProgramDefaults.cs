using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgramDefaults
	{
#if ANDROID
		public static Android.Content.Context DefaultContext { get; private set; }
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow { get; private set; }
#endif

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp(List<Assembly> testAssemblies)
		{
			var appBuilder = MauiApp.CreateBuilder();
			appBuilder
				.ConfigureLifecycleEvents(life =>
				{
#if ANDROID
					life.AddAndroid(android =>
					{
						android.OnCreate((a, b) =>
						{
							DefaultContext = a;
						});
					});
#elif WINDOWS
					life.AddWindows(windows =>
					{
						windows.OnWindowCreated((w) => DefaultWindow = w);
					});
#endif
				})
				.ConfigureTests(new TestOptions
				{
					Assemblies = testAssemblies,
				})
				.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				})
				.UseVisualRunner();

			var mauiApp = appBuilder.Build();

			DefaultTestApp = mauiApp.Services.GetRequiredService<IApplication>();

			return mauiApp;
		}
	}
}