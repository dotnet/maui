using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.ConfigureLifecycleEvents(life =>
				{
#if __ANDROID__
					life.AddAndroid(android =>
					{
						android.OnCreate((a, b) => Platform.Init(a));
					});
#endif
				})
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