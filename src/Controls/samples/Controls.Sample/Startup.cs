#if NET6_0_OR_GREATER
#define BLAZOR_ENABLED
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Services;
using Maui.Controls.Sample.ViewModel;
#if BLAZOR_ENABLED
using Microsoft.AspNetCore.Components.WebView.Maui;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Controls;
using Maui.Controls.Sample.Controls;

namespace Maui.Controls.Sample
{

	public class CustomButton : Button { }

	public class Startup : IStartup
	{
		enum PageType { Xaml, Semantics, Main, Blazor, NavigationPage, Shell }
		private PageType _pageType = PageType.NavigationPage;

		public readonly static bool UseXamlApp = true;
		public readonly static bool UseFullDI = false;

		public void Configure(IAppHostBuilder appBuilder)
		{
			bool useFullDIAndBlazor = UseFullDI || _pageType == PageType.Blazor;

			appBuilder
				.UseFormsCompatibility()
				.UseMauiControlsHandlers()
				.ConfigureMauiHandlers(handlers =>
				{
#if __ANDROID__
					handlers.AddCompatibilityRenderer(typeof(CustomButton),
						typeof(Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat.ButtonRenderer));
#elif __IOS__
					handlers.AddCompatibilityRenderer(typeof(CustomButton),
						typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.ButtonRenderer));
#elif WINDOWS
					handlers.AddCompatibilityRenderer(typeof(CustomButton),
						typeof(Microsoft.Maui.Controls.Compatibility.Platform.UWP.ButtonRenderer));
#endif
				});

			if (UseXamlApp)
				appBuilder.UseMauiApp<XamlApp>();
			else
				appBuilder.UseMauiApp<MyApp>();

			// Use a "third party" library that brings in a massive amount of controls
			appBuilder.UseRed();

#if DEBUG && !WINDOWS
			appBuilder.EnableHotReload();
#endif
			appBuilder.UseMauiControlsHandlers();

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
				});

			if (useFullDIAndBlazor)
			{
#if BLAZOR_ENABLED
				appBuilder
					.RegisterBlazorMauiWebView(typeof(Startup).Assembly);
#endif
				appBuilder.UseMicrosoftExtensionsServiceProviderFactory();
			}
			else
			{
				appBuilder.UseMauiServiceProviderFactory(constructorInjection: true);
			}

			appBuilder
				.ConfigureServices(services =>
				{
					// The MAUI DI does not support generic argument resolution
					if (useFullDIAndBlazor)
					{
						services.AddLogging(logging =>
						{
#if WINDOWS
							logging.AddDebug();
#else
							logging.AddConsole();
#endif
						});
					}

					services.AddSingleton<ITextService, TextService>();
					services.AddTransient<MainPageViewModel>();
#if BLAZOR_ENABLED
					if (useFullDIAndBlazor)
						services.AddBlazorWebView();
#endif
					services.AddTransient(
						serviceType: _pageType == PageType.Blazor ? typeof(Page) : typeof(IPage),
						implementationType: _pageType switch
						{
							PageType.Shell => typeof(AppShell),
							PageType.NavigationPage => typeof(NavPage),
							PageType.Xaml => typeof(XamlPage),
							PageType.Semantics => typeof(SemanticsPage),
							PageType.Blazor =>
#if BLAZOR_ENABLED
								typeof(BlazorPage),
#else
								throw new NotSupportedException("Blazor requires .NET 6 or higher."),
#endif
							PageType.Main => typeof(MainPage),
							_ => throw new Exception(),
						});

					services.AddTransient<IWindow, Window>();
				})
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("Dokdo-Regular.ttf", "Dokdo");
					fonts.AddFont("LobsterTwo-Regular.ttf", "Lobster Two");
					fonts.AddFont("LobsterTwo-Bold.ttf", "Lobster Two Bold");
					fonts.AddFont("LobsterTwo-Italic.ttf", "Lobster Two Italic");
					fonts.AddFont("LobsterTwo-BoldItalic.ttf", "Lobster Two BoldItalic");
					fonts.AddFont("ionicons.ttf", "Ionicons");
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
	}
}
