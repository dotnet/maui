using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TApplication> : UIApplicationDelegate, IUIApplicationDelegate
		where TApplication : Application
	{
		bool _isSuspended;
		Application? _application;

		public override UIWindow? Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication nativeApplication, NSDictionary launchOptions)
		{
			if (!(Activator.CreateInstance(typeof(TApplication)) is TApplication application))
				throw new InvalidOperationException($"We weren't able to create the App {typeof(TApplication)}");

			var host = application.CreateBuilder().ConfigureServices(ConfigureNativeServices).Build(application);

			if (Application.Current == null)
				throw new InvalidOperationException($"App is not {nameof(Application)}");

			_application = Application.Current;

			if (_application.Services == null)
				throw new InvalidOperationException("App was not initialized");

			_application.OnCreated();

			var mauiContext = new MauiContext(_application.Services);
			var window = _application.CreateWindow(new ActivationState(mauiContext));

			window.MauiContext = mauiContext;

			// Hack for now we set this on the App Static but this should be on IFrameworkElement
			Application.Current.SetHandlerContext(window.MauiContext);

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
				_application?.OnResumed();
			}
		}

		public override void OnResignActivation(UIApplication application)
		{
			_isSuspended = true;
			_application?.OnPaused();
		}

		public override void WillTerminate(UIApplication application)
		{
			_application?.OnStopped();
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{

		}
	}
}
