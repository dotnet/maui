using System;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public class MauiApplication<TStartup> : Android.App.Application
		where TStartup : IStartup, new()
	{
		public MauiApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
		{
		}

		public override void OnCreate()
		{
			var startup = new TStartup();

			IAppHostBuilder appBuilder;

			if (startup is IHostBuilderStartup hostBuilderStartup)
			{
				appBuilder = hostBuilderStartup
					.CreateHostBuilder();
			}
			else
			{
				appBuilder = AppHostBuilder
					.CreateDefaultAppBuilder();
			}

			appBuilder.
				ConfigureServices(ConfigureNativeServices);

			startup.Configure(appBuilder);

			var host = appBuilder.Build();
			if (host.Services == null)
				throw new InvalidOperationException("App was not intialized");

			var services = host.Services;

			var app = services.GetRequiredService<MauiApp>();
			host.SetServiceProvider(app);

			base.OnCreate();
		}

		// Configure native services like HandlersContext, ImageSourceHandlers etc.. 
		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddTransient<IAndroidLifecycleHandler, AndroidLifecycleHandler>();
		}
	}
}