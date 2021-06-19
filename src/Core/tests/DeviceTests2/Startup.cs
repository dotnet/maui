using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using NewNamespace;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner;

namespace Microsoft.Maui.Core.DeviceTests
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
						android.OnCreate((a, b) => Maui.DeviceTests.Platform.Init(a));
					});
#endif
				})
				.UseVisualRunner()
				.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					ActivityType = typeof(TestActivity)
				})
				.ConfigureTestRunner(new TestRunnerOptions
				{
					Assemblies = { typeof(Startup).Assembly }
				});
		}
	}
}