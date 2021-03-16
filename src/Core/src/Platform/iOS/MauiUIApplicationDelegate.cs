using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TStartup> : MauiUIApplicationDelegate
		where TStartup : IStartup, new()
	{
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var startup = new TStartup();

			var appBuilder = AppHostBuilder
				.CreateDefaultAppBuilder()
				.ConfigureServices(ConfigureNativeServices);

			startup.Configure(appBuilder);

			var host = appBuilder.Build();
			if (host.Services == null)
				throw new InvalidOperationException("App was not intialized");

			var services = host.Services;

			CurrentApp = services.GetRequiredService<MauiApp>();
			CurrentApp.SetServiceProvider(services);

			var mauiContext = new MauiContext(services);
			var window = CurrentApp.CreateWindow(new ActivationState(mauiContext));

			window.MauiContext = mauiContext;

			// Hack for now we set this on the App Static but this should be on IFrameworkElement
			CurrentApp.SetHandlerContext(window.MauiContext);

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

	public abstract class MauiUIApplicationDelegate : UIApplicationDelegate, IUIApplicationDelegate
	{
		public override UIWindow? Window { get; set; }

		public static MauiApp CurrentApp { get; internal set; } = null!;
	}
}