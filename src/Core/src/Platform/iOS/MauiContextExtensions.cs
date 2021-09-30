using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Microsoft.Maui
{
	internal static class MauiContextExtensions
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

		public static IMauiContext MakeScoped(this IMauiContext mauiContext, UIWindow nativeWindow)
		{
			var scopedContext = new MauiContext(mauiContext);
			scopedContext.AddSpecific(nativeWindow);
			return scopedContext;
		}
	}
}
