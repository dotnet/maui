using System;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources.Core;

namespace Microsoft.Maui.Platform
{
	internal static partial class MauiContextExtensions
	{
		public static FlowDirection GetFlowDirection(this IMauiContext mauiContext)
		{
			// TODO MAUI NEed to test what the right way to do this is
			//var WindowHandle = PlatformView.GetWindowHandle();

			//// Retrieve current extended style
			//var extended_style = PlatformMethods.GetWindowLongPtr(WindowHandle, PlatformMethods.WindowLongFlags.GWL_EXSTYLE);

			//// https://docs.microsoft.com/en-us/windows/win32/directwrite/how-to-ensure-text-is-displayed-with-the-correct-reading-direction
			//var layoutRTL = extended_style & (long)PlatformMethods.ExtendedWindowStyles.WS_EX_LAYOUTRTL;
			//var readingRTL = extended_style & (long)PlatformMethods.ExtendedWindowStyles.WS_EX_RTLREADING;

			//if (layoutRTL == (long)PlatformMethods.ExtendedWindowStyles.WS_EX_LAYOUTRTL ||
			//	readingRTL == (long)PlatformMethods.ExtendedWindowStyles.WS_EX_RTLREADING)
			//{
			//	VirtualView.SetDeviceFlowDirection(FlowDirection.RightToLeft);
			//}
			//else
			//{
			//	VirtualView.SetDeviceFlowDirection(FlowDirection.LeftToRight);
			//}

			string resourceFlowDirection = ResourceManager.Current.DefaultContext.QualifierValues["LayoutDirection"];
			if (resourceFlowDirection == "LTR")
				return FlowDirection.LeftToRight;
			else if (resourceFlowDirection == "RTL")
				return FlowDirection.RightToLeft;

			return FlowDirection.MatchParent;
		}

		public static NavigationRootManager GetNavigationRootManager(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<NavigationRootManager>();

		public static UI.Xaml.Window GetPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetRequiredService<UI.Xaml.Window>();

		public static UI.Xaml.Window? GetOptionalPlatformWindow(this IMauiContext mauiContext) =>
			mauiContext.Services.GetService<UI.Xaml.Window>();

		public static IServiceProvider GetApplicationServices(this IMauiContext mauiContext)
		{
			return MauiWinUIApplication.Current.Services
				?? throw new InvalidOperationException("Unable to find Application Services");
		}


		public static IMauiContext MakeScoped(this IMauiContext mauiContext, bool registerNewNavigationRoot)
		{
			var scopedContext = new MauiContext(mauiContext.Services);

			if (registerNewNavigationRoot)
			{
				scopedContext.AddWeakSpecific(new NavigationRootManager(scopedContext));
			}

			return scopedContext;
		}
	}
}