using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Tests
{
	public class StartupStub : IStartup
	{
		public void Configure(IAppHostBuilder appBuilder)
		{
			appBuilder.ConfigureServices(ConfigureServices);
		}

		public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddSingleton<IMauiContext>(provider => new HandlersContextStub(provider));
			services.AddTransient<IButton, ButtonStub>();
		}
	}
}