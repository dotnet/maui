using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgramDefaults
	{
#if ANDROID
		public static global::Android.Content.Context DefaultContext { get; private set; }
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow { get; private set; }
#endif

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp(List<Assembly> testAssemblies)
		{
			return CreateMauiApp((_) => new TestOptions
			{
				Assemblies = testAssemblies
			});
		}

		public static MauiApp CreateMauiApp(Func<IServiceProvider, TestOptions> options)
		{
			var appBuilder = MauiApp.CreateBuilder();

#if DEBUG
			appBuilder.Services.AddHybridWebViewDeveloperTools();
#endif

			appBuilder
				.ConfigureLifecycleEvents(life =>
				{
#if ANDROID
					life.AddAndroid(android =>
					{
						android.OnCreate((a, b) =>
						{
							DefaultContext = a;
						});
					});
#elif WINDOWS
					life.AddWindows(windows =>
					{
						windows.OnWindowCreated((w) => DefaultWindow = w);
					});
#endif
				})
				.ConfigureTests(options);

#if WINDOWS
			if (options(null).Assemblies.Any(a => a.FullName.Contains("Controls.DeviceTests",
				StringComparison.OrdinalIgnoreCase)))
			{
				appBuilder.UseControlsHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				});
			}
			else
			{
				appBuilder.UseHeadlessRunner(new HeadlessRunnerOptions
				{
					RequiresUIContext = true,
				});
			}
#else
			appBuilder.UseHeadlessRunner(new HeadlessRunnerOptions
			{
				RequiresUIContext = true,
			});
#endif

#if IOS || MACCATALYST

			appBuilder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
				handlers.AddHandler<Microsoft.Maui.Controls.CarouselView, Microsoft.Maui.Controls.Handlers.Items2.CarouselViewHandler2>();
			});

#endif
			appBuilder.UseVisualRunner();

			appBuilder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
			{
				ValidateOnBuild = true,
				ValidateScopes = true,
			}));

			var mauiApp = appBuilder.Build();

			DefaultTestApp = mauiApp.Services.GetRequiredService<IApplication>();

			return mauiApp;
		}
	}
}