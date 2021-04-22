using System;
using System.Collections.Generic;
using System.Diagnostics;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModel;
#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Components.WebView.Maui;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Maui.Controls.Sample
{
	public class Startup : IStartup
	{
		enum PageType { Xaml, Semantics, Main, Blazor }
		private PageType _pageType = PageType.Main;

		public readonly static bool UseXamlApp = true;

		public void Configure(IAppHostBuilder appBuilder)
		{
			if (UseXamlApp)
			{
				// Use all the Forms features
				appBuilder = appBuilder
					.UseFormsCompatibility()
					.UseMauiApp<XamlApp>();
			}
			else
			{
				// Use just the Forms renderers
				appBuilder = appBuilder
					.UseCompatibilityRenderers()
					.UseMauiApp<MyApp>();
			}
#if DEBUG
			appBuilder.EnableHotReload();
#endif
			appBuilder
#if NET6_0_OR_GREATER
				.RegisterBlazorMauiWebView()
#endif
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

					services.AddTransient(
						serviceType: typeof(IPage),
						implementationType: _pageType switch
						{
							PageType.Xaml => typeof(XamlPage),
							PageType.Semantics => typeof(SemanticsPage),
							PageType.Blazor =>
#if NET6_0_OR_GREATER
								typeof(BlazorPage),
#else
								throw new NotSupportedException("Blazor requires .NET 6 or higher."),
#endif
							PageType.Main => typeof(MainPage),
							_ => throw new Exception(),
						});

					services.AddTransient<IWindow, Microsoft.Maui.Controls.Window>();
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("Dokdo-Regular.ttf", "Dokdo");
				})
				.ConfigureEssentials(essentials =>
				{
					essentials
						.UseVersionTracking()
						.UseMapServiceToken("YOUR-KEY-HERE")
						.AddAppAction("test_action", "Test App Action")
						.AddAppAction("second_action", "Second App Action")
						.OnAppAction(appAction =>
						{
							Debug.WriteLine($"You seem to have arrived from a special place: {appAction.Title} ({appAction.Id})");
						});
				})
				.ConfigureLifecycleEvents(events =>
				{
					events.AddEvent<Action<string>>("CustomEventName", value => LogEvent("CustomEventName"));

#if __ANDROID__
					// Log everything in this one
					events.AddAndroid(android => android
						.OnActivityResult((a, b, c, d) => LogEvent(nameof(AndroidLifecycle.OnActivityResult), b.ToString()))
						.OnBackPressed((a) => LogEvent(nameof(AndroidLifecycle.OnBackPressed)))
						.OnConfigurationChanged((a, b) => LogEvent(nameof(AndroidLifecycle.OnConfigurationChanged)))
						.OnCreate((a, b) => LogEvent(nameof(AndroidLifecycle.OnCreate)))
						.OnDestroy((a) => LogEvent(nameof(AndroidLifecycle.OnDestroy)))
						.OnNewIntent((a, b) => LogEvent(nameof(AndroidLifecycle.OnNewIntent)))
						.OnPause((a) => LogEvent(nameof(AndroidLifecycle.OnPause)))
						.OnPostCreate((a, b) => LogEvent(nameof(AndroidLifecycle.OnPostCreate)))
						.OnPostResume((a) => LogEvent(nameof(AndroidLifecycle.OnPostResume)))
						.OnPressingBack((a) => LogEvent(nameof(AndroidLifecycle.OnPressingBack)) && false)
						.OnRequestPermissionsResult((a, b, c, d) => LogEvent(nameof(AndroidLifecycle.OnRequestPermissionsResult)))
						.OnRestart((a) => LogEvent(nameof(AndroidLifecycle.OnRestart)))
						.OnRestoreInstanceState((a, b) => LogEvent(nameof(AndroidLifecycle.OnRestoreInstanceState)))
						.OnResume((a) => LogEvent(nameof(AndroidLifecycle.OnResume)))
						.OnSaveInstanceState((a, b) => LogEvent(nameof(AndroidLifecycle.OnSaveInstanceState)))
						.OnStart((a) => LogEvent(nameof(AndroidLifecycle.OnStart)))
						.OnStop((a) => LogEvent(nameof(AndroidLifecycle.OnStop))));

					// Add some cool features/things
					var shouldPreventBack = 1;
					events.AddAndroid(android => android
						.OnResume(a =>
						{
							LogEvent(nameof(AndroidLifecycle.OnResume), "shortcut");
						})
						.OnPressingBack(a =>
						{
							LogEvent(nameof(AndroidLifecycle.OnPressingBack), "shortcut");

							return shouldPreventBack-- > 0;
						})
						.OnBackPressed(a => LogEvent(nameof(AndroidLifecycle.OnBackPressed), "shortcut"))
						.OnRestoreInstanceState((a, b) =>
						{
							LogEvent(nameof(AndroidLifecycle.OnRestoreInstanceState), "shortcut");

							Debug.WriteLine($"{b.GetString("test2", "fail")} == {b.GetBoolean("test", false)}");
						})
						.OnSaveInstanceState((a, b) =>
						{
							LogEvent(nameof(AndroidLifecycle.OnSaveInstanceState), "shortcut");

							b.PutBoolean("test", true);
							b.PutString("test2", "yay");
						}));
#elif __IOS__
					// Log everything in this one
					events.AddiOS(ios => ios
						.ContinueUserActivity((a, b, c) => LogEvent(nameof(iOSLifecycle.ContinueUserActivity)) && false)
						.DidEnterBackground((a) => LogEvent(nameof(iOSLifecycle.DidEnterBackground)))
						.FinishedLaunching((a, b) => LogEvent(nameof(iOSLifecycle.FinishedLaunching)) && true)
						.OnActivated((a) => LogEvent(nameof(iOSLifecycle.OnActivated)))
						.OnResignActivation((a) => LogEvent(nameof(iOSLifecycle.OnResignActivation)))
						.OpenUrl((a, b, c) => LogEvent(nameof(iOSLifecycle.OpenUrl)) && false)
						.PerformActionForShortcutItem((a, b, c) => LogEvent(nameof(iOSLifecycle.PerformActionForShortcutItem)))
						.WillEnterForeground((a) => LogEvent(nameof(iOSLifecycle.WillEnterForeground)))
						.WillTerminate((a) => LogEvent(nameof(iOSLifecycle.WillTerminate))));
#elif WINDOWS
					// Log everything in this one
					events.AddWindows(windows => windows
						.OnActivated((a, b) => LogEvent(nameof(WindowsLifecycle.OnActivated)))
						.OnClosed((a, b) => LogEvent(nameof(WindowsLifecycle.OnClosed)))
						.OnLaunched((a, b) => LogEvent(nameof(WindowsLifecycle.OnLaunched)))
						.OnVisibilityChanged((a, b) => LogEvent(nameof(WindowsLifecycle.OnVisibilityChanged))));
#endif

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
