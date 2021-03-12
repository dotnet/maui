using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TApplication> : UIApplicationDelegate, IUIApplicationDelegate where TApplication : MauiApp
	{
		public override UIWindow? Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			if (!(Activator.CreateInstance(typeof(TApplication)) is TApplication app))
				throw new InvalidOperationException($"We weren't able to create the App {typeof(TApplication)}");

			var host = app.CreateBuilder().ConfigureServices(ConfigureNativeServices).Build(app);

			if (App.Current == null || App.Current.Services == null)
				throw new InvalidOperationException("App was not intialized");


			var mauiContext = new MauiContext(App.Current.Services);
			var window = app.CreateWindow(new ActivationState(mauiContext));

			window.MauiContext = mauiContext;

			//Hack for now we set this on the App Static but this should be on IFrameworkElement
			App.Current.SetHandlerContext(window.MauiContext);

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
			base.OnActivated(application);

			foreach (var iOSApplicationDelegateHandler in GetIosLifecycleHandler())
				iOSApplicationDelegateHandler.OnActivated(application);
		}

		public override void OnResignActivation(UIApplication application)
		{
			base.OnResignActivation(application);

			foreach (var iOSApplicationDelegateHandler in GetIosLifecycleHandler())
				iOSApplicationDelegateHandler.OnResignActivation(application);
		}

		public override void WillTerminate(UIApplication application)
		{
			base.WillTerminate(application);

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