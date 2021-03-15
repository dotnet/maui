using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TStartup, TApplication> : UIApplicationDelegate, IUIApplicationDelegate
		where TStartup : IStartup
		where TApplication : MauiApp
	{
		public override UIWindow? Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			if (!(Activator.CreateInstance(typeof(TStartup)) is TStartup startup))
				throw new InvalidOperationException($"We weren't able to create the Startup {typeof(TStartup)}");

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.ConfigureServices(ConfigureNativeServices);

			startup.Configure(appBuilder);

			appBuilder.Build();

			if (!(Activator.CreateInstance(typeof(TApplication)) is TApplication app))
				throw new InvalidOperationException($"We weren't able to create the App {typeof(TApplication)}");

			appBuilder.SetServiceProvider(app);

			if (app == null || app.Services == null)
				throw new InvalidOperationException("App was not intialized");

			var mauiContext = new MauiContext(app.Services);
			var window = app.CreateWindow(new ActivationState(mauiContext));

			window.MauiContext = mauiContext;

			// Hack for now we set this on the App Static but this should be on IFrameworkElement
			app.SetHandlerContext(window.MauiContext);

			var content = (window.Page as IView) ?? window.Page.View;

			Window = new UIWindow
			{
				RootViewController = new UIViewController
				{
					View = content.ToNative(window.MauiContext)
				}
			};

			Window.MakeKeyAndVisible();

			return true;
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{

		}
	}
}