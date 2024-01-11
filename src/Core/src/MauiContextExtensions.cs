using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.ApplicationModel;

#if WINDOWS
using PlatformApplication = Microsoft.UI.Xaml.Application;
using PlatformWindow = Microsoft.UI.Xaml.Window;
#elif __IOS__ || __MACCATALYST__
using PlatformApplication = UIKit.IUIApplicationDelegate;
using PlatformWindow = UIKit.UIWindow;
#elif __ANDROID__
using PlatformApplication = Android.App.Application;
using PlatformWindow = Android.App.Activity;
#elif TIZEN
using PlatformApplication = Tizen.Applications.CoreApplication;
using PlatformWindow = Tizen.NUI.Window;
#else
using PlatformApplication = System.Object;
using PlatformWindow = System.Object;
#endif

namespace Microsoft.Maui
{
	internal static partial class MauiContextExtensions
	{
		public static IAnimationManager GetAnimationManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<IAnimationManager>();

		public static IDispatcher GetDispatcher(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<IDispatcher>();

		public static IDispatcher? GetOptionalDispatcher(this IMauiContext mauiContext) =>
			mauiContext.Services.GetService<IDispatcher>();

		public static IMauiContext MakeApplicationScope(this IMauiContext mauiContext, PlatformApplication platformApplication)
		{
			var scopedContext = new MauiContext(mauiContext.Services);

			scopedContext.AddSpecific(platformApplication);

			scopedContext.InitializeScopedServices();

			return scopedContext;
		}

		internal static void SetupDispatcher(this IMauiContext mauiContext)
		{
			// Capture the window level dispatcher.
			// If the user first retrieves the IDispatcher from the scoped service on a background thread
			// it'll just return null. This ensures that the user has access to the window dispatcher
			_ = mauiContext?.Services?.GetService<IDispatcher>();
		}

		public static IMauiContext MakeWindowScope(this IMauiContext mauiContext, PlatformWindow platformWindow, out IServiceScope scope)
		{
			scope = mauiContext.Services.CreateScope();

#if ANDROID
			var scopedContext = new MauiContext(scope.ServiceProvider, platformWindow);
#else
			var scopedContext = new MauiContext(scope.ServiceProvider);
#endif

			scopedContext.AddWeakSpecific(platformWindow);

#if ANDROID
			scopedContext.AddSpecific(new NavigationRootManager(scopedContext));
#endif
#if WINDOWS
			scopedContext.AddSpecific(new NavigationRootManager(platformWindow));
#endif

			// Windows creates its window from the application thread so we
			// leave it to the MauiWinUIWindow to call `SetupDispatcher`
			// Android/iOS create the window scope from their respective versions of "window"
#if !WINDOWS
			scopedContext.SetupDispatcher();
#endif

			return scopedContext;
		}

		public static void InitializeScopedServices(this IMauiContext scopedContext)
		{
			var scopedServices = scopedContext.Services.GetServices<IMauiInitializeScopedService>();

			foreach (var service in scopedServices)
				service.Initialize(scopedContext.Services);
		}
	}
}
