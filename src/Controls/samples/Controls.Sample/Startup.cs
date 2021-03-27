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
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Maui.Controls.Sample
{
	public class Startup : IStartup
	{
		public readonly static bool UseXamlPage = false;
		public readonly static bool UseXamlApp = true;

		public void Configure(IAppHostBuilder appBuilder)
		{
			if (UseXamlApp)
			{
				appBuilder = appBuilder
					.RegisterCompatibilityForms()
					.UseMauiApp<XamlApp>();
			}
			else
			{
				appBuilder = appBuilder
					.UseMauiApp<MyApp>();
			}

			appBuilder
				.RegisterCompatibilityRenderers()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddInMemoryCollection(new Dictionary<string, string>
					{
						{"MyKey", "Dictionary MyKey Value"},
						{":Title", "Dictionary_Title"},
						{"Position:Name", "Dictionary_Name" },
						{"Logging:LogLevel:Default", "Warning"}
					});
				})
				//.UseMauiServiceProviderFactory(true)
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
					//services.AddTransient<IAndroidApplicationLifetime, CustomAndroidLifecycleHandler>();
#elif __IOS__
					//services.AddTransient<IIosApplicationLifetime, CustomIosLifecycleHandler>();
#endif
				})
				.ConfigureLifecycleEvents((ctx, events) =>
				{
#if __ANDROID__
					events.AddAndroid(lifecycleBuilder => lifecycleBuilder
						.OnActivityResult((a, b, c, d) => LogEvent(nameof(AndroidLifecycle.OnActivityResult), b.ToString()))
						.OnBackPressed((a) => LogEvent(nameof(AndroidLifecycle.OnBackPressed)))
						.OnConfigurationChanged((a, b) => LogEvent(nameof(AndroidLifecycle.OnConfigurationChanged)))
						.OnCreate((a, b) => LogEvent(nameof(AndroidLifecycle.OnCreate)))
						.OnDestroy((a) => LogEvent(nameof(AndroidLifecycle.OnDestroy)))
						.OnPause((a) => LogEvent(nameof(AndroidLifecycle.OnPause)))
						.OnPostCreate((a, b) => LogEvent(nameof(AndroidLifecycle.OnPostCreate)))
						.OnPostResume((a) => LogEvent(nameof(AndroidLifecycle.OnPostResume)))
						.OnRestart((a) => LogEvent(nameof(AndroidLifecycle.OnRestart)))
						.OnRestoreInstanceState((a, b) => LogEvent(nameof(AndroidLifecycle.OnRestoreInstanceState)))
						.OnResume((a) => LogEvent(nameof(AndroidLifecycle.OnResume)))
						.OnSaveInstanceState((a, b) => LogEvent(nameof(AndroidLifecycle.OnSaveInstanceState)))
						.OnStart((a) => LogEvent(nameof(AndroidLifecycle.OnStart)))
						.OnStop((a) => LogEvent(nameof(AndroidLifecycle.OnStop))));
#endif
				})
#if __ANDROID__
				.ConfigureAndroidLifecycleEvents((ctx, life) =>
				{
					life.OnResume(a =>
						{
							LogEvent(nameof(AndroidLifecycle.OnResume), "shortcut");
							CurrentActivity = a;
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
						});
				})
#endif
				.ConfigureFonts((hostingContext, fonts) =>
				{
					fonts.AddFont("Dokdo-Regular.ttf", "Dokdo");
				});
		}

#if __ANDROID__
		public static Android.App.Activity CurrentActivity;

		[Android.App.Activity]
		public class TestActivity : Android.App.Activity
		{

		}
#endif

		void LogEvent(string eventName, string type = null)
		{
			Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? "" : $" ({type})")}");
		}

		// To use the Microsoft.Extensions.DependencyInjection ServiceCollection and not the MAUI one
		public class DIExtensionsServiceProviderFactory : IServiceProviderFactory<ServiceCollection>
		{
			public ServiceCollection CreateBuilder(IServiceCollection services)
				=> new ServiceCollection { services };

			public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder)
				=> containerBuilder.BuildServiceProvider();
		}
	}
}