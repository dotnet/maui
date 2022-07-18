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
using PlatformView = ElmSharp.EvasObject;
using ParentView = ElmSharp.EvasObject;
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
	}
}
