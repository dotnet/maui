using System;
using System.Linq;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();
			appBuilder
				.ConfigureLifecycleEvents(life =>
				{
#if __ANDROID__
					life.AddAndroid(android =>
					{
						android.OnCreate((activity, bundle) =>
							ApplicationModel.Platform.Init(activity, bundle));
						android.OnRequestPermissionsResult((activity, requestCode, permissions, grantResults) =>
							ApplicationModel.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults));
					});
#endif
				})
				.ConfigureTests(new TestOptions
				{
					Assemblies =
					{
						typeof(MauiProgram).Assembly
					},
					SkipCategories = Traits
						.GetSkipTraits()
						.ToList(),
				})
				.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				})
				.UseVisualRunner();

			return appBuilder.Build();
		}
	}
}