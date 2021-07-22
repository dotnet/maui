using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.TestUtils.DeviceTests.Sample
{
	public static class MauiProgram
	{
		public static MauiAppBuilder CreateAppBuilder()
		{
			var appBuilder = MauiApp.CreateBuilder();
			appBuilder
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

			return appBuilder;
		}
	}
}