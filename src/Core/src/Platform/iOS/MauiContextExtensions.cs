using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using UIKit;

namespace Microsoft.Maui
{
	internal static partial class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			var window = mauiContext.GetNativeWindow();
			if (window == null)
				return FlowDirection.LeftToRight;

			return window.EffectiveUserInterfaceLayoutDirection.ToFlowDirection();
		}

		public static UIWindow GetNativeWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<UIWindow>();

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UIApplicationDelegate nativeApplication)
		{
			var scopedContext = new MauiContext(mauiContext);

			scopedContext.AddSpecific(nativeApplication);

			scopedContext.InitializeScopedServices();

			return scopedContext;
		}

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UIWindow nativeWindow, out IServiceScope scope)
		{
			scope = mauiContext.Services.CreateScope();

			var scopedContext = new MauiContext(scope.ServiceProvider, mauiContext);

			scopedContext.AddSpecific(nativeWindow);
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IAnimationManager>());

			scopedContext.InitializeScopedServices();

			return scopedContext;
		}
	}
}