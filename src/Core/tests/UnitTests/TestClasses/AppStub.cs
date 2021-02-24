using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.UnitTests.TestClasses;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Tests
{
	class AppStub : MauiApp
	{
		public void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddSingleton<IMauiContext>(provider => new HandlersContextStub(provider));
			services.AddTransient<IButton, ButtonStub>();
		}

		public override IAppHostBuilder CreateBuilder()
		{
			return base.CreateBuilder().ConfigureServices(ConfigureServices);
		}

		public override IWindow GetWindowFor(IActivationState state)
		{
			return new WindowStub();
		}

		internal void ClearApp()
		{
			Current = null;
		}
	}
}
