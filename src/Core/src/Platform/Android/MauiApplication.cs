using System;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui
{
	public class MauiApplication<TApplication> : global::Android.App.Application where TApplication : MauiApp
	{
		public MauiApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
		{

		}
		IHost? _host;
		public override void OnCreate()
		{
			if (!(Activator.CreateInstance(typeof(TApplication)) is TApplication app))
				throw new InvalidOperationException($"We weren't able to create the App {typeof(TApplication)}");

			_host = app.CreateBuilder().ConfigureServices(ConfigureNativeServices).Build(app);

			//_host.Start();
			base.OnCreate();
		}

		// Configure native services like HandlersContext, ImageSourceHandlers etc.. 
		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddTransient<IAndroidLifecycleHandler, AndroidLifecycleHandler>();
		}
	}
}
