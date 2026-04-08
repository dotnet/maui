using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgram
	{
#if ANDROID
		public static global::Android.Content.Context DefaultContext => MauiProgramDefaults.DefaultContext;
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow => MauiProgramDefaults.DefaultWindow;

		/// <summary>
		/// Records Windows lifecycle event names in the order they fire during app startup.
		/// Used by LifecycleEventOrderTests to validate event ordering.
		/// </summary>
		public static List<string> LifecycleEventLog { get; } = new();
#endif

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp() =>
			MauiProgramDefaults.CreateMauiApp((sp) =>
			{
				var options = new TestOptions
				{
					Assemblies = new List<Assembly>()
					{
						typeof(MauiProgram).Assembly
					},
					SkipCategories = typeof(TestCategory).GetExcludedTestCategories()
				};

				return options;
			},
			configureBuilder: builder =>
			{
#if WINDOWS
				builder.ConfigureLifecycleEvents(life =>
				{
					life.AddWindows(windows =>
					{
						windows.OnAppActivation((app, args) =>
						{
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnAppActivation));
							return false;
						});
						windows.OnLaunching((app, args) =>
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnLaunching)));
						windows.OnLaunched((app, args) =>
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnLaunched)));
						windows.OnWindowCreated(w =>
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnWindowCreated)));
					});
				});
#endif
			});
	}
}