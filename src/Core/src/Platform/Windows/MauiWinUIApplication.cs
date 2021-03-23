using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public class MauiWinUIApplication<TStartup> : MauiWinUIApplication
		where TStartup : IStartup, new()
	{
		//MauiWinUIWindow? _window;

		protected override void OnLaunched(UI.Xaml.LaunchActivatedEventArgs args)
		{
			var startup = new TStartup();

			var host = startup
				.CreateAppHostBuilder()
				.ConfigureServices(ConfigureNativeServices)
				.ConfigureUsing(startup)
				.Build();

			Services = host.Services;
			Application = Services.GetRequiredService<IApplication>();

			//var mauiContext = new MauiContext(Services);

			//_window = new MauiWinUIWindow();

			//var activationState = new ActivationState(args, _window, mauiContext);
			//var window = Application.CreateWindow(activationState);
			//window.MauiContext = mauiContext;

			//var content = (window.Page as IView) ?? window.Page.View;

			//_window.Content = content.ToNative(window.MauiContext);

			//_window.Activate();
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
		}
	}

	public class MauiWinUIApplication : UI.Xaml.Application
	{
		protected MauiWinUIApplication()
		{
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}