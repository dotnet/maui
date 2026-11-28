using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.TestUtils.DeviceTests.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();
			appBuilder
				.ConfigureTests(new TestOptions
				{
					TestAssembly = typeof(MauiProgram).Assembly,
				})
				.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				});

			return appBuilder.Build();
		}
	}
}