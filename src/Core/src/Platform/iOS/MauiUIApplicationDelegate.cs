using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TStartup> : UIApplicationDelegate, IUIApplicationDelegate
		where TStartup : IStartup, new()
	{
		public override UIWindow? Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
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

			var mauiContext = new MauiContext(services);
			var window = app.CreateWindow(new ActivationState(mauiContext));

			window.MauiContext = mauiContext;

			var content = (window.Page as IView) ?? window.Page.View;

			Window = new UIWindow
			{
				RootViewController = new UIViewController
				{
					View = content.ToNative(window.MauiContext)
				}
			};

			Window.MakeKeyAndVisible();

			foreach (var iOSApplicationDelegateHandler in GetIosLifecycleHandler())
				iOSApplicationDelegateHandler.FinishedLaunching(application, launchOptions);

			return true;
		}

		public override void OnActivated(UIApplication application)
		{
			foreach (var iOSApplicationDelegateHandler in GetIosLifecycleHandler())
				iOSApplicationDelegateHandler.OnActivated(application);
		}

		public override void OnResignActivation(UIApplication application)
		{
			foreach (var iOSApplicationDelegateHandler in GetIosLifecycleHandler())
				iOSApplicationDelegateHandler.OnResignActivation(application);
		}

		public override void WillTerminate(UIApplication application)
		{
			foreach (var iOSApplicationDelegateHandler in GetIosLifecycleHandler())
				iOSApplicationDelegateHandler.WillTerminate(application);
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddTransient<IIosApplicationDelegateHandler, IosApplicationDelegateHandler>();
		}

		IEnumerable<IIosApplicationDelegateHandler> GetIosLifecycleHandler() =>
			App.Current?.Services?.GetServices<IIosApplicationDelegateHandler>() ?? Enumerable.Empty<IIosApplicationDelegateHandler>();
	}
}