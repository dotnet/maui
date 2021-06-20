using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.TestUtils.DeviceTests.Sample
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.ConfigureTests(new TestOptions
				{
					Assemblies =
					{
						typeof(Startup).Assembly
					},
				})
				.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				})
				.UseVisualRunner();
		}
	}
}