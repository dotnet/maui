using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Window = Microsoft.Maui.Controls.Window;

namespace Maui.SimpleSampleApp
{
	public class Startup : IStartup
	{
		public readonly static bool UseSemanticsPage = false;
		public readonly static bool UseXamlPage = false;
		public readonly static bool UseXamlApp = true;

		public void Configure(IAppHostBuilder appBuilder)
		{
			// if (UseXamlApp)
			// {
			// 	// Use all the Forms features
			// 	appBuilder = appBuilder
			// 		.UseFormsCompatibility()
			// 		.UseMauiApp<XamlApp>();
			// }
			// else
			{
				// Use just the Forms renderers
				appBuilder = appBuilder
					.UseCompatibilityRenderers()
					.UseMauiApp<SimpleSampleMauiApp>();
			}

			appBuilder
				.ConfigureAppConfiguration(config =>
				{
					config.AddInMemoryCollection(new Dictionary<string, string>
					{
						{"MyKey", "Dictionary MyKey Value"},
						{":Title", "Dictionary_Title"},
						{"Position:Name", "Dictionary_Name" },
						{"Logging:LogLevel:Default", "Warning"}
					});
				})
				.UseMauiServiceProviderFactory(true)
				//.UseServiceProviderFactory(new DIExtensionsServiceProviderFactory())
				.ConfigureServices(services =>
				{
					services.AddSingleton<ITextService, TextService>();
					services.AddTransient<MainPageViewModel>();

					// if (UseXamlPage)
					// 	services.AddTransient<IPage, XamlPage>();
					// else if (UseSemanticsPage)
					// 	services.AddTransient<IPage, SemanticsPage>();
					// else
						services.AddTransient<IPage, ExamplePage>();

					services.AddTransient<IWindow, Window>();
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("Dokdo-Regular.ttf", "Dokdo");
				})
				// .ConfigureEssentials(essentials =>
				// {
				// 	essentials
				// 		.UseVersionTracking()
				// 		.UseMapServiceToken("YOUR-KEY-HERE")
				// 		.AddAppAction("test_action", "Test App Action")
				// 		.AddAppAction("second_action", "Second App Action")
				// 		.OnAppAction(appAction =>
				// 		{
				// 			Debug.WriteLine($"You seem to have arrived from a special place: {appAction.Title} ({appAction.Id})");
				// 		});
				// })
				.ConfigureLifecycleEvents(events =>
				{
					events.AddEvent<Action<string>>("CustomEventName", value => LogEvent("CustomEventName"));

					// Log everything in this one
					events.AddWindows(windows => windows
						.OnActivated((a, b) => LogEvent(nameof(LinuxLifecycle.OnApplicationActivated)))
						.OnClosed((a, b) => LogEvent(nameof(LinuxLifecycle.OnHidden)))
						.OnLaunched((a, b) => LogEvent(nameof(LinuxLifecycle.OnLaunched)))
						.OnVisibilityChanged((a, b) =>
						{
							LogEvent(nameof(LinuxLifecycle.OnVisibilityChanged));

							if (b.Event.State == VisibilityState.Unobscured)
							{
								if (a.AllocatedWidth < 2)
									a.WidthRequest = 200;
							}
						}
						));

					static bool LogEvent(string eventName, string type = null)
					{
						Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? "" : $" ({type})")}");
						return true;
					}
				});
		}

		// To use the Microsoft.Extensions.DependencyInjection ServiceCollection and not the MAUI one
		class DIExtensionsServiceProviderFactory : IServiceProviderFactory<ServiceCollection>
		{
			public ServiceCollection CreateBuilder(IServiceCollection services)
				=> new ServiceCollection { services };

			public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder)
				=> containerBuilder.BuildServiceProvider();
		}
	}
}
