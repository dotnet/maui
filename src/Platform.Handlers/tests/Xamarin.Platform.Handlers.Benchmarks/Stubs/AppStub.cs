using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xamarin.Platform.Hosting;

namespace Xamarin.Platform.Handlers.Benchmarks
{
	class MockApp : App
	{
		public override IAppHostBuilder CreateBuilder()
		{
			return base.CreateBuilder()
				.ConfigureServices(ConfigureNativeServices);
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddSingleton<IMauiContext>(provider => new HandlersContextStub(provider));
		}
	}
}
