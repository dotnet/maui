using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgram
	{
#if ANDROID
		public static Android.Content.Context DefaultContext { get; private set; }
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow { get; private set; }
#endif

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();
			appBuilder
				.ConfigureLifecycleEvents(life =>
				{
#if ANDROID
					life.AddAndroid(android =>
					{
						android.OnCreate((a, b) => DefaultContext = a);
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
					Assemblies =
					{
						typeof(MauiProgram).Assembly
					},
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