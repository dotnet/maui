using System;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui
{
	/// <summary>
	/// Defines the core behavior of a .NET MAUI application running on iOS and MacCatalyst.
	/// </summary>
	public abstract partial class MauiUIApplicationDelegate : UIResponder, IUIApplicationDelegate, IPlatformApplication
	{
		internal const string MauiSceneConfigurationKey = "__MAUI_DEFAULT_SCENE_CONFIGURATION__";
		internal const string GetConfigurationSelectorName = "application:configurationForConnectingSceneSession:options:";

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "IMauiContext is a non-NSObject in MAUI.")]
		IMauiContext _applicationContext = null!;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "IServiceProvider is a non-NSObject from Microsoft.Extensions.DependencyInjection.")]
		IServiceProvider? _services;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "IApplication is a non-NSObject in MAUI.")]
		IApplication? _application;

		protected MauiUIApplicationDelegate() : base()
		{
			Current = this;
			IPlatformApplication.Current = this;
			
			// Initialize CALayer autoresize to super layer support
			CALayerAutoresizeToSuperLayer.EnsureInitialized();
		}

		/// <summary>
		/// When overridden in a derived class, creates the <see cref="MauiApp"/> to be used in this application.
		/// Typically a <see cref="MauiApp"/> is created by calling <see cref="MauiApp.CreateBuilder(bool)"/>, configuring
		/// the returned <see cref="MauiAppBuilder"/>, and returning the built app by calling <see cref="MauiAppBuilder.Build"/>.
		/// </summary>
		/// <returns>The built <see cref="MauiApp"/>.</returns>
		protected abstract MauiApp CreateMauiApp();

		[Export("application:willFinishLaunchingWithOptions:")]
		public virtual bool WillFinishLaunching(UIApplication application, NSDictionary? launchOptions)
		{
			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			_applicationContext = rootContext.MakeApplicationScope(this);

			_services = _applicationContext.Services;

			_services?.InvokeLifecycleEvents<iOSLifecycle.WillFinishLaunching>(del => del(application, launchOptions));

			return true;
		}

		[Export("application:didFinishLaunchingWithOptions:")]
		public virtual bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
		{
			_application = _services!.GetRequiredService<IApplication>();

			this.SetApplicationHandler(_application, _applicationContext);

			// if there is no scene delegate or support for scene delegates, then we set up the window here
			if (!this.HasSceneManifest())
			{
				this.CreatePlatformWindow(_application, application, launchOptions);

				if (Window != null)
					_services?.InvokeLifecycleEvents<iOSLifecycle.OnPlatformWindowCreated>(del => del(Window));
			}

			_services?.InvokeLifecycleEvents<iOSLifecycle.FinishedLaunching>(del => del(application!, launchOptions!));

			return true;
		}

		public override bool RespondsToSelector(Selector? sel)
		{
			// if the app is not a multi-window app, then we cannot override the GetConfiguration method
			if (sel?.Name == GetConfigurationSelectorName && !this.HasSceneManifest())
				return false;

			return base.RespondsToSelector(sel);
		}

		[Export("application:configurationForConnectingSceneSession:options:")]
		[System.Runtime.Versioning.SupportedOSPlatform("ios13.1")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos13.1")]
		public virtual UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options)
			=> new(MauiUIApplicationDelegate.MauiSceneConfigurationKey, connectingSceneSession.Role);

		[Export("application:performActionForShortcutItem:completionHandler:")]
		public virtual void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.PerformActionForShortcutItem>(del => del(application, shortcutItem, completionHandler));
		}

		[Export("application:openURL:options:")]
		public virtual bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
		{
			var wasHandled = false;

			_services?.InvokeLifecycleEvents<iOSLifecycle.OpenUrl>(del =>
			{
				wasHandled = del(application, url, options) || wasHandled;
			});

			return wasHandled;
		}

		[Export("application:continueUserActivity:restorationHandler:")]
		public virtual bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			var wasHandled = false;

			_services?.InvokeLifecycleEvents<iOSLifecycle.ContinueUserActivity>(del =>
			{
				wasHandled = del(application, userActivity, completionHandler) || wasHandled;
			});

			return wasHandled;
		}

		[Export("applicationDidBecomeActive:")]
		public virtual void OnActivated(UIApplication application)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.OnActivated>(del => del(application));
		}

		[Export("applicationWillResignActive:")]
		public virtual void OnResignActivation(UIApplication application)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.OnResignActivation>(del => del(application));
		}

		[Export("applicationWillTerminate:")]
		public virtual void WillTerminate(UIApplication application)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.WillTerminate>(del => del(application));
		}

		[Export("applicationDidEnterBackground:")]
		public virtual void DidEnterBackground(UIApplication application)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.DidEnterBackground>(del => del(application));
		}

		[Export("applicationWillEnterForeground:")]
		public virtual void WillEnterForeground(UIApplication application)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.WillEnterForeground>(del => del(application));
		}

		[Export("applicationSignificantTimeChange:")]
		public virtual void ApplicationSignificantTimeChange(UIApplication application)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.ApplicationSignificantTimeChange>(del => del(application));
		}

		[Export("application:performFetchWithCompletionHandler:")]
		public virtual void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
		{
			_services?.InvokeLifecycleEvents<iOSLifecycle.PerformFetch>(del => del(application, completionHandler));
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "There can only be one MauiUIApplicationDelegate.")]
		public static MauiUIApplicationDelegate Current { get; private set; } = null!;

		[Export("window")]
		public virtual UIWindow? Window { get; set; }

		// TODO: we should investigate throwing an exception or changing the public API
		IServiceProvider IPlatformApplication.Services => _services!;

		IApplication IPlatformApplication.Application => _application!;

		[Obsolete("Use the IPlatformApplication.Current.Services instead.")]
		public IServiceProvider Services
		{
			get => _services!;
			protected set => _services = value;
		}

		[Obsolete("Use the IPlatformApplication.Current.Application instead.")]
		public IApplication Application
		{
			get => _application!;
			protected set => _application = value;
		}
	}
}
