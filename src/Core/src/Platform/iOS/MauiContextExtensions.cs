using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
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
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IDispatcherProvider>().GetDispatcher(nativeApplication));

			return scopedContext;
		}

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UIWindow nativeWindow)
		{
			var scopedContext = new MauiContext(mauiContext);

			scopedContext.AddSpecific(nativeWindow);
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IAnimationManager>());
			scopedContext.AddSpecific(svc => svc.GetRequiredService<IDispatcherProvider>().GetDispatcher(nativeWindow));

			return scopedContext;
		}
	}
}