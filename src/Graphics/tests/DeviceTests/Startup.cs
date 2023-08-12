using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using DeviceRunners.XHarness;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Graphics.DeviceTests;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.ConfigureUITesting()
			.UseXHarnessTestRunner(conf => conf
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddXunit())
			.UseVisualTestRunner(conf => conf
				.AddTestAssembly(typeof(MauiProgram).Assembly)
				.AddXunit());

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
