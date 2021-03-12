using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	class MockApp : App
	{
		public override IAppHostBuilder CreateBuilder()
		{
			return base.CreateBuilder()
				.ConfigureServices(ConfigureNativeServices);
		}

		public override IWindow CreateWindow(IActivationState state)
		{
			throw new System.NotImplementedException();
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddSingleton<IMauiContext>(provider => new HandlersContextStub(provider));
		}
	}
}
