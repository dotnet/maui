using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TApplication> : UIApplicationDelegate, IUIApplicationDelegate where TApplication : App
	{
		bool _isSuspended;
		App? _app;

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

			if (App.Current == null)
				throw new InvalidOperationException($"App is not {nameof(App)}");

			_app = App.Current;

			if (_app.Services == null)
				throw new InvalidOperationException("App was not initialized");

			_app.OnCreated();

			var mauiContext = new MauiContext(_app.Services);
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

			return true;
		}
		public override void OnActivated(UIApplication application)
		{
			if (_isSuspended)
			{
				_isSuspended = false;
				_app?.OnResumed();
			}
		}

		public override void OnResignActivation(UIApplication application)
		{
			_isSuspended = true;
			_app?.OnPaused();
		}

		public override void WillTerminate(UIApplication application)
		{
			_app?.OnStopped();
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{

		}
	}
}
