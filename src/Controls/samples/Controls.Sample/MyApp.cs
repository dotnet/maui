using System;
using System.Collections.Generic;
using System.Diagnostics;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Application = Microsoft.Maui.Application;

namespace Maui.Controls.Sample
{
	public class MyApp : Application
	{
		public readonly static bool UseXamlPage = false;

		public override IAppHostBuilder CreateBuilder() =>
			base.CreateBuilder()
				.RegisterCompatibilityRenderers()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "MyKey", "Dictionary MyKey Value" },
						{ ":Title", "Dictionary_Title" },
						{ "Position:Name", "Dictionary_Name" },
						{ "Logging:LogLevel:Default", "Warning" }
					});
				})
				.ConfigureServices((hostingContext, services) =>
				{
					services.AddSingleton<ITextService, TextService>();
					services.AddTransient<MainPageViewModel>();
					if (UseXamlPage)
						services.AddTransient<IPage, XamlPage>();
					else
						services.AddTransient<IPage, MainPage>();
					services.AddTransient<IWindow, MainWindow>();
				})
				.ConfigureFonts((hostingContext, fonts) =>
				{
					fonts.AddFont("dokdo_regular.ttf", "Dokdo");
				});

		// IAppState state
		public override IWindow CreateWindow(IActivationState state)
		{
			Forms.Init(state);
			return Services.GetRequiredService<IWindow>();
		}

		public override void OnCreated()
		{
			Debug.WriteLine("Application Created.");
		}

		public override void OnPaused()
		{
			Debug.WriteLine("Application Paused.");
		}

		public override void OnResumed()
		{
			Debug.WriteLine("Application Resumed.");
		}

		public override void OnStopped()
		{
			Debug.WriteLine("Application Stopped.");
		}
	}

	// To use DI ServiceCollection and not the MAUI one
	public class DIExtensionsServiceProviderFactory : IServiceProviderFactory<ServiceCollection>
	{
		public ServiceCollection CreateBuilder(IServiceCollection services)
			=> new ServiceCollection { services };

		public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder)
			=> containerBuilder.BuildServiceProvider();
	}
}