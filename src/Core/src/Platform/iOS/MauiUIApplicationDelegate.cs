using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TStartup> : MauiUIApplicationDelegate
		where TStartup : IStartup, new()
	{
		public IWindow? CurrentWindow { get; private set; }

		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var startup = new TStartup();

			var host = startup
				.CreateAppHostBuilder()
				.ConfigureServices(ConfigureNativeServices)
				.ConfigureUsing(startup)
				.Build();

			Services = host.Services;
			Application = Services.GetRequiredService<IApplication>();

			var mauiContext = new MauiContext(Services);

			var activationState = new ActivationState(mauiContext);
			CurrentWindow = Application.CreateWindow(activationState);
			CurrentWindow.MauiContext = mauiContext;

			var content = (CurrentWindow.Page as IView) ?? CurrentWindow.Page.View;

			var adornerService = Services.GetService<IAdornerService>() as AdornerService;
			Window = new MauiUIWindow()
			{
				RootViewController = new UIViewController
				{
					View = content.ToNative(CurrentWindow.MauiContext)
				}
			};

			//We set the window because we could have multiwindows, this should when active(focus) windows change
			adornerService?.SetUIWindow(Window);

			Window.MakeKeyAndVisible();

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.FinishedLaunching>(del => del(application, launchOptions));

			return true;
		}

		public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.PerformActionForShortcutItem>(del => del(application, shortcutItem, completionHandler));
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
		{
			var wasHandled = false;

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.OpenUrl>(del =>
			{
				wasHandled = del(application, url, options) || wasHandled;
			});

			return wasHandled || base.OpenUrl(application, url, options);
		}

		public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			var wasHandled = false;

			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.ContinueUserActivity>(del =>
			{
				wasHandled = del(application, userActivity, completionHandler) || wasHandled;
			});

			return wasHandled || base.ContinueUserActivity(application, userActivity, completionHandler);
		}

		public override void OnActivated(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.OnActivated>(del => del(application));
		}

		public override void OnResignActivation(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.OnResignActivation>(del => del(application));
		}

		public override void WillTerminate(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.WillTerminate>(del => del(application));
		}

		public override void DidEnterBackground(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.DidEnterBackground>(del => del(application));
		}

		public override void WillEnterForeground(UIApplication application)
		{
			Current.Services?.InvokeLifecycleEvents<iOSLifecycle.WillEnterForeground>(del => del(application));
		}

		// Configure native services like HandlersContext, ImageSourceHandlers etc.. 
		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
			services.AddSingleton<IAdornerService, AdornerService>();
		}
	}

	public abstract class MauiUIApplicationDelegate : UIApplicationDelegate, IUIApplicationDelegate
	{
		protected MauiUIApplicationDelegate()
		{
			Current = this;
		}

		public static MauiUIApplicationDelegate Current { get; private set; } = null!;

		public override UIWindow? Window { get; set; }

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}

	class MauiUIWindow : UIWindow
	{
		AdornerService? _adornerService;

		public MauiUIWindow()
		{
			_adornerService = MauiUIApplicationDelegate.Current.Services.GetService<IAdornerService>() as AdornerService;
		}

		public override void SendEvent(UIEvent evt)
		{
			if (_adornerService != null && evt.Type == UIEventType.Touches && evt.AllTouches.Count == 1)
			{
				UITouch touch = evt.AllTouches.ToArray<UITouch>()[0];
				if (touch.Phase == UIKit.UITouchPhase.Ended)
				{
					var statusBarHeight = UIKit.UIApplication.SharedApplication.StatusBarFrame.Size.Height;
					var nativePoint = touch.LocationInView(this);
					_adornerService.ExecuteTouchEventDelegate(new Point(nativePoint.X, nativePoint.Y - statusBarHeight));
				}
			}
			base.SendEvent(evt);
		}
	}
}