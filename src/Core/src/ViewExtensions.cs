using Microsoft.Maui.Graphics;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using System.IO;
#if NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
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
#else
using PlatformView = System.Object;
using ParentView = System.Object;
#endif

namespace Microsoft.Maui
{
	public static partial class ViewExtensions
	{
		public static Task<Stream?> CaptureAsync(this IView view, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
		{
#if PLATFORM
			if (view?.ToPlatform() is not PlatformView platformView)
				return Task.FromResult<Stream?>(null);

			if (!Screenshot.Current.IsCaptureSupported)
				return Task.FromResult<Stream?>(null);

			return CaptureAsync(platformView, format, quality);
#else
			return Task.FromResult<Stream?>(null);
#endif
		}

#if PLATFORM
		static async Task<Stream?> CaptureAsync(PlatformView window, ScreenshotFormat format, int quality)
		{
			var result = await Screenshot.Current.CaptureAsync(window);

			return await result.OpenReadAsync(format, quality);
		}
#endif
	}
}
