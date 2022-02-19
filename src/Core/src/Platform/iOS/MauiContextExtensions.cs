using Microsoft.Extensions.DependencyInjection;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			var window = mauiContext.GetPlatformWindow();
			if (window == null)
				return FlowDirection.LeftToRight;

			return window.EffectiveUserInterfaceLayoutDirection.ToFlowDirection();
		}

		public static UIWindow GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<UIWindow>();
	}
}