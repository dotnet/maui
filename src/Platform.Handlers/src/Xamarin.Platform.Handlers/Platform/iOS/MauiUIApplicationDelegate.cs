using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UIKit;
using Xamarin.Platform.Hosting;

namespace Xamarin.Platform
{
	public class MauiUIApplicationDelegate<TApplication> : UIApplicationDelegate, IUIApplicationDelegate where TApplication : MauiApp
	{
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			if (!(Activator.CreateInstance(typeof(TApplication)) is TApplication app))
				throw new InvalidOperationException($"We weren't able to create the App {typeof(TApplication)}");

			var host = app.CreateBuilder().ConfigureServices(ConfigureNativeServices).Build(app);

			if (MauiApp.Current == null || MauiApp.Current.Services == null)
				throw new InvalidOperationException("App was not intialized");

			var window = app.GetWindowFor(null!);

			window.MauiContext = new MauiContext(MauiApp.Current.Services);

			//Hack for now we set this on the App Static but this should be on IFrameworkElement
			App.Current.SetHandlerContext(window.MauiContext);

			var content = window.Page.View;

			var uiWindow = new UIWindow
			{
				RootViewController = new UIViewController
				{
					View = content.ToNative(window.MauiContext)
				}
			};

			uiWindow.MakeKeyAndVisible();

			return true;
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			
		}
	}
}
