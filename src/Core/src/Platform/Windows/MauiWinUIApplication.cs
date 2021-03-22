using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class MauiWinUIApplication<TStartup> : Microsoft.UI.Xaml.Application
		where TStartup : IStartup, new()
	{
		//MauiWinUIWindow? m_window;

		protected override void OnLaunched(LaunchActivatedEventArgs args)
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


			//m_window = new MauiWinUIWindow();

			//var content = window.Page.View;//(window.Page as IView) ?? window.Page.View;

			//m_window.Content = new ContentControl()
			//{
			//	Content = content.ToNative(window.MauiContext)
			//};

			//m_window.Activate();

		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{

		}
	}
}
