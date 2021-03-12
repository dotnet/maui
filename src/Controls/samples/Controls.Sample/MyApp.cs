using System;
using System.Collections.Generic;
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

namespace Maui.Controls.Sample
{
	public class MyApp : MauiApp
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
				.UseServiceProviderFactory(new DIExtensionsServiceProviderFactory())
				.ConfigureServices((hostingContext, services) =>
				{
					services.AddSingleton<ITextService, TextService>();
					services.AddTransient<MainPageViewModel>();
					if (UseXamlPage)
						services.AddTransient<IPage, XamlPage>();
					else
						services.AddTransient<IPage, MainPage>();
					services.AddTransient<IWindow, MainWindow>();
#if __ANDROID__
					services.AddTransient<IAndroidLifecycleHandler, CustomAndroidLifecycleHandler>();
#endif

#if __IOS__
					services.AddTransient<IIosApplicationDelegateHandler, CustomIosLifecycleHandler>();
#endif
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