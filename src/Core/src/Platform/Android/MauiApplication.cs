using System;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public class MauiApplication<TStartup, TApplication> : Android.App.Application
		where TStartup : IStartup
		where TApplication : MauiApp
	{
		public MauiApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
		{

		}

		public override void OnCreate()
		{
			if (!(Activator.CreateInstance(typeof(TStartup)) is TStartup startup))
				throw new InvalidOperationException($"We weren't able to create the Startup {typeof(TStartup)}");

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.ConfigureServices(ConfigureNativeServices);

			startup.Configure(appBuilder);

			appBuilder.Build();

			if (!(Activator.CreateInstance(typeof(TApplication)) is TApplication app))
				throw new InvalidOperationException($"We weren't able to create the App {typeof(TApplication)}");

			appBuilder.SetServiceProvider(app);

			base.OnCreate();
		}

		// Configure native services like HandlersContext, ImageSourceHandlers etc.. 
		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{

		}
	}
}