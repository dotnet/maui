using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	public class StartupStub : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder
				.ConfigureServices(ConfigureNativeServices);
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddSingleton<IMauiContext>(provider => new HandlersContextStub(provider));
		}
	}
}