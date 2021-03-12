using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	class ApplicationStub : Application
	{
		public override IAppHostBuilder CreateBuilder()
		{
			return base.CreateBuilder()
				.ConfigureServices(ConfigureNativeServices);
		}

		public override IWindow CreateWindow(IActivationState state)
		{
			return new WindowStub();
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddSingleton<IMauiContext>(provider => new HandlersContextStub(provider));
		}
	}
}
