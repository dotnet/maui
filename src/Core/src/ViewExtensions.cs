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

#if !TIZEN
		internal static bool NeedsContainer(this IView? view, IViewHandler handler)
		{
			if (view is null)
				return false;

			if (view.Clip is not null || view.Shadow is not null)
				return true;

#if ANDROID
			if (TouchEventInterceptor.NeedsContainer(view, handler))
				return true;
#endif

#if ANDROID || IOS
			if (view is IBorder border && border.Border != null)
				return true;
#elif WINDOWS
			if (view is IBorderView border)
				return border.Shape is not null || border.Stroke is not null;
#endif
			return false;
		}
#endif
	}
}
