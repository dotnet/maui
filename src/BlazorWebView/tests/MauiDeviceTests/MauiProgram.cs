using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static class MauiProgram
	{
#if ANDROID
		public static Android.Content.Context CurrentContext { get; private set; }
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow { get; private set; }
#endif
		public static MauiApp CreateMauiApp()
		{
			var x = MauiApp.CreateBuilder();
			x.ConfigureLifecycleEvents(life =>
			{
#if ANDROID
				life.AddAndroid(android =>
				{
					android.OnCreate((a, b) => CurrentContext = a);
				});
#elif WINDOWS
				life.AddWindows(windows =>
				{
					windows.OnWindowCreated((w) => DefaultWindow = w);
				});
#endif
			});

			x.ConfigureTests(new TestOptions
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
			return x.Build();
		}
	}
}