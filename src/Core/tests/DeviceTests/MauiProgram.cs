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
#endif

		/// <summary>
		/// Records platform lifecycle event names in the order they fire during app startup.
		/// Used by LifecycleEventOrderTests to validate event ordering.
		/// </summary>
		public static List<string> LifecycleEventLog { get; } = new();

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp()
		{
			LifecycleEventLog.Clear();

			return MauiProgramDefaults.CreateMauiApp((sp) =>
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
				builder.ConfigureLifecycleEvents(life =>
				{
#if ANDROID
					life.AddAndroid(android =>
					{
						android.OnCreate((a, b) =>
							LifecycleEventLog.Add(nameof(AndroidLifecycle.OnCreate)));
						android.OnStart(a =>
							LifecycleEventLog.Add(nameof(AndroidLifecycle.OnStart)));
						android.OnResume(a =>
							LifecycleEventLog.Add(nameof(AndroidLifecycle.OnResume)));
						android.OnPostResume(a =>
							LifecycleEventLog.Add(nameof(AndroidLifecycle.OnPostResume)));
					});
#elif IOS || MACCATALYST
					life.AddiOS(ios =>
					{
						ios.FinishedLaunching((app, options) =>
						{
							LifecycleEventLog.Add(nameof(iOSLifecycle.FinishedLaunching));
							return true;
						});
						ios.OnActivated(app =>
							LifecycleEventLog.Add(nameof(iOSLifecycle.OnActivated)));
					});
#elif WINDOWS
					life.AddWindows(windows =>
					{
						windows.OnAppInstanceActivated((app, args) =>
						{
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnAppInstanceActivated));
							return false;
						});
						windows.OnLaunching((app, args) =>
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnLaunching)));
						windows.OnLaunched((app, args) =>
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnLaunched)));
						windows.OnWindowCreated(w =>
							LifecycleEventLog.Add(nameof(WindowsLifecycle.OnWindowCreated)));
					});
#endif
				});
			});
		}
	}
}