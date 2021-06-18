using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Xunit.Runners;

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
				.ConfigureTestRunner(new TestRunnerOptions
				{
					Assemblies = { typeof(Startup).Assembly }
				});
		}
	}
}