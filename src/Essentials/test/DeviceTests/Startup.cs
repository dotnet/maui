using System;
using System.Linq;
using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using DeviceRunners.XHarness;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Essentials.DeviceTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
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
			});

		builder
			.ConfigureUITesting()
			.UseXHarnessTestRunner(conf =>
			{
				conf.AddTestAssembly(typeof(MauiProgram).Assembly)
					.AddXunit();

				foreach (var skip in Traits.GetSkipTraits())
				{
					var pair = skip.Split("=");
					conf.SkipCategory(pair[0], pair[1]);
				}
			})
			.UseVisualTestRunner(conf => conf
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddXunit());

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
