using Microsoft.Maui.Hosting;
using Xunit;
using Xunit.Runners;

namespace Microsoft.Maui.Core.DeviceTests
{
	public class Startup : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.ConfigureTestRunner(new TestRunnerOptions
				{
					Assemblies = { typeof(Startup).Assembly }
				});
		}
	}

	public class TheTest
	{
		[Fact]
		public void SuccessTest()
		{
		}

		[Fact(Skip = "skipping!")]
		public void SkipTest()
		{
		}

		[Fact]
		public void FailTest()
		{
			throw new System.Exception("Failed!");
		}
	}
}