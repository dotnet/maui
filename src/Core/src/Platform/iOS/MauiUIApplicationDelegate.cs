using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiUIApplicationDelegate<TStartup> : MauiUIApplicationDelegate
		where TStartup : IStartup, new()
	{
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			var startup = new TStartup();

			var host = startup
				.CreateAppHostBuilder()
				//.ConfigureServices(ConfigureNativeServices)
				.ConfigureUsing(startup)
				.Build();

			Services = host.Services;
			Application = Services.GetRequiredService<IApplication>();

			var mauiContext = new MauiContext(Services);

			var activationState = new ActivationState(mauiContext);
			var window = Application.CreateWindow(activationState);
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

			//foreach (var iOSApplicationDelegateHandler in GetIosApplicationLifetime())
			//	iOSApplicationDelegateHandler.FinishedLaunching(application, launchOptions);

			Current.Services?.InvokeLifecycleEvents<IosLifecycle.FinishedLaunching>(del => del(application, launchOptions));

			return true;
		}

		public override void OnActivated(UIApplication application)
		{
			//foreach (var iOSApplicationDelegateHandler in GetIosApplicationLifetime())
			//	iOSApplicationDelegateHandler.OnActivated(application);

			Current.Services?.InvokeLifecycleEvents<IosLifecycle.OnActivated>(del => del(application));
		}

		public override void OnResignActivation(UIApplication application)
		{
			//foreach (var iOSApplicationDelegateHandler in GetIosApplicationLifetime())
			//	iOSApplicationDelegateHandler.OnResignActivation(application);

			Current.Services?.InvokeLifecycleEvents<IosLifecycle.OnResignActivation>(del => del(application));
		}

		public override void WillTerminate(UIApplication application)
		{
			//foreach (var iOSApplicationDelegateHandler in GetIosApplicationLifetime())
			//	iOSApplicationDelegateHandler.WillTerminate(application);

			Current.Services?.InvokeLifecycleEvents<IosLifecycle.WillTerminate>(del => del(application));
		}

		public override void DidEnterBackground(UIApplication application)
		{
			//foreach (var iOSApplicationDelegateHandler in GetIosApplicationLifetime())
			//	iOSApplicationDelegateHandler.DidEnterBackground(application);

			Current.Services?.InvokeLifecycleEvents<IosLifecycle.DidEnterBackground>(del => del(application));
		}

		public override void WillEnterForeground(UIApplication application)
		{
			//foreach (var iOSApplicationDelegateHandler in GetIosApplicationLifetime())
			//	iOSApplicationDelegateHandler.WillEnterForeground(application);

			Current.Services?.InvokeLifecycleEvents<IosLifecycle.WillEnterForeground>(del => del(application));
		}

		//void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		//{
		//	services.AddTransient<IIosApplicationLifetime, IosApplicationLifetime>();
		//}

		//IEnumerable<IIosApplicationLifetime> GetIosApplicationLifetime() =>
		//	Services?.GetServices<IIosApplicationLifetime>() ?? Enumerable.Empty<IIosApplicationLifetime>();

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
}