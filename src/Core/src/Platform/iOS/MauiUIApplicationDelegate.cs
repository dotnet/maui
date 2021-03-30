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

			var host = startup
				.CreateAppHostBuilder()
				.ConfigureServices(ConfigureNativeServices)
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

			return true;
		}

		void ConfigureNativeServices(HostBuilderContext ctx, IServiceCollection services)
		{
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
}