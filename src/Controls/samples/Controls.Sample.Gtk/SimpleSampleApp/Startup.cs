using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Window = Microsoft.Maui.Controls.Window;

namespace Maui.SimpleSampleApp
{

	public class Startup
	{

		public readonly static bool UseSemanticsPage = false;
		public readonly static bool UseXamlPage = false;
		public readonly static bool UseXamlApp = true;

		public static MauiApp CreateMauiApp()
		{
			var appBuilder = MauiApp.CreateBuilder();

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
				   .UseMauiApp<SimpleSampleMauiApp>();
			}

			var services = appBuilder.Services;

			appBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
			{
				{ "MyKey", "Dictionary MyKey Value" },
				{ ":Title", "Dictionary_Title" },
				{ "Position:Name", "Dictionary_Name" },
				{ "Logging:LogLevel:Default", "Warning" }
			});

			services.AddSingleton<ITextService, TextService>();
			services.AddTransient<MainPageViewModel>();

			// if (UseXamlPage)
			// 	services.AddTransient<IPage, XamlPage>();
			// else if (UseSemanticsPage)
			// 	services.AddTransient<IPage, SemanticsPage>();
			// else
			services.AddTransient<Page, ExamplePage>();

			services.AddTransient<IWindow, Window>();

			appBuilder
				//.UseServiceProviderFactory(new DIExtensionsServiceProviderFactory())
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
					events.AddGtk(gtk => gtk
					   .OnActivated((a, b) => LogEvent(nameof(GtkLifecycle.OnApplicationActivated)))
					   .OnClosed((a, b) => LogEvent(nameof(GtkLifecycle.OnHidden)))
					   .OnLaunched((a, b) => LogEvent(nameof(GtkLifecycle.OnLaunched)))
					   .OnShown((a, b) =>
						{
							LogEvent(nameof(GtkLifecycle.OnShown));
							a.Maximize();
						})
					);

					static bool LogEvent(string eventName, string type = null)
					{
						Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? "" : $" ({type})")}");

						return true;
					}
				});

			return appBuilder.Build();

		}

	}

}