using Microsoft.Maui.Graphics;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using System.IO;
#if (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using IPlatformViewHandler = Microsoft.Maui.IViewHandler;
#endif
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
using ParentView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
using ParentView = Android.Views.IViewParent;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using ParentView = Microsoft.UI.Xaml.DependencyObject;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using ParentView = Tizen.NUI.BaseComponents.View;
#else
using PlatformView = System.Object;
using ParentView = System.Object;
using System;
#endif

namespace Microsoft.Maui
{
	public static partial class ViewExtensions
	{
		public static Task<IScreenshotResult?> CaptureAsync(this IView view)
		{
#if PLATFORM
			if (view?.ToPlatform() is not PlatformView platformView)
				return Task.FromResult<IScreenshotResult?>(null);

			if (!Screenshot.Default.IsCaptureSupported)
				return Task.FromResult<IScreenshotResult?>(null);

			return CaptureAsync(platformView);
#else
			return Task.FromResult<IScreenshotResult?>(null);
#endif
		}


#if PLATFORM
		async static Task<IScreenshotResult?> CaptureAsync(PlatformView window) =>
			await Screenshot.Default.CaptureAsync(window);
#endif

		internal static bool NeedsContainer(this IView? view, PlatformView? platformView)
		{
#if !TIZEN
			if (view?.Clip != null || view?.Shadow != null)
				return true;
#endif

#if ANDROID
			// This is only here for Android because almost all Android views will require
			// a wrapper when the view is InputTransparent. This is because Android does not
			// have a concept of "not hit testable" so we have to emulate it intercepting the
			// the touch events with a parent layout.
			if (view?.InputTransparent == true && platformView is not IInputTransparentManagingView)
				return true;
#endif

#if ANDROID || IOS
			if (view is IBorder border && border.Border != null)
				return true;
#endif

#if WINDOWS || TIZEN
			if (view is IBorderView border)
				return border?.Shape != null || border?.Stroke != null;
#endif

			return false;
		}

	}
}
