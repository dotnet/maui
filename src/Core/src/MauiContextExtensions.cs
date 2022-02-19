using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;

#if WINDOWS
using NativeApplication = Microsoft.UI.Xaml.Application;
using NativeWindow = Microsoft.UI.Xaml.Window;
#elif __IOS__ || __MACCATALYST__
using NativeApplication = UIKit.IUIApplicationDelegate;
using NativeWindow = UIKit.UIWindow;
#elif __ANDROID__
using NativeApplication = Android.App.Application;
using NativeWindow = Android.App.Activity;
#else
using NativeApplication = System.Object;
using NativeWindow = System.Object;
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

		public static IMauiContext MakeApplicationScope(this IMauiContext mauiContext, NativeApplication platformApplication)
		{
			var scopedContext = new MauiContext(mauiContext.Services);

			scopedContext.AddSpecific(platformApplication);

			scopedContext.InitializeScopedServices();

			return scopedContext;
		}

		public static IMauiContext MakeWindowScope(this IMauiContext mauiContext, NativeWindow platformWindow, out IServiceScope scope)
		{
			scope = mauiContext.Services.CreateScope();

#if __ANDROID__
			var scopedContext = new MauiContext(scope.ServiceProvider, platformWindow);
#else
			var scopedContext = new MauiContext(scope.ServiceProvider);
#endif

			scopedContext.AddWeakSpecific(platformWindow);

#if WINDOWS || __ANDROID__
			scopedContext.AddSpecific(new NavigationRootManager(scopedContext));
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
