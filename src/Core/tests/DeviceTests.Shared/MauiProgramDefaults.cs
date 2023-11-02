using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgramDefaults
	{
#if ANDROID
		public static Android.Content.Context DefaultContext { get; private set; }
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow { get; private set; }
#endif

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp(List<Assembly> testAssemblies)
		{
			var appBuilder = MauiApp.CreateBuilder();
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
				.ConfigureTests(new TestOptions
				{
					Assemblies = testAssemblies,
				});

#if WINDOWS
			if (testAssemblies.Any(a => a.FullName.Contains("Controls.DeviceTests",
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
			appBuilder.UseVisualRunner();

			appBuilder.ConfigureContainer(new TestServiceCollection(appBuilder.Services));

			var mauiApp = appBuilder.Build();

			DefaultTestApp = mauiApp.Services.GetRequiredService<IApplication>();

			return mauiApp;
		}

		private class TestServiceCollection : IServiceProviderFactory<ServiceCollection>
		{
			private IServiceCollection _services;

			public TestServiceCollection(IServiceCollection services)
			{
				_services = services;
			}

			public ServiceCollection CreateBuilder(IServiceCollection services)
				=> new ServiceCollection { _services };

			public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder)
				=> containerBuilder.BuildServiceProvider(validateScopes: true);
		}
	}
}