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

/* Unmerged change from project 'Core(net8.0-maccatalyst)'
Before:
				return Task.FromResult<IScreenshotResult?>(null);
After:
			{
				return Task.FromResult<IScreenshotResult?>(null);
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.19041.0)'
Before:
				return Task.FromResult<IScreenshotResult?>(null);
After:
			{
				return Task.FromResult<IScreenshotResult?>(null);
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.20348.0)'
Before:
				return Task.FromResult<IScreenshotResult?>(null);
After:
			{
				return Task.FromResult<IScreenshotResult?>(null);
			}
*/
			{
			{
				return Task.FromResult<IScreenshotResult?>(null);
			}
			}

			if (!Screenshot.Default.IsCaptureSupported)
			{
				return Task.FromResult<IScreenshotResult?>(null);
			}

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
		internal static bool NeedsContainer(this IView? view)
		{
			if (view?.Clip != null || view?.Shadow != null)

/* Unmerged change from project 'Core(net8.0-ios)'
Before:
				return true;

#if ANDROID
			if (view?.InputTransparent == true)
				return true;
#endif

#if ANDROID || IOS
			if (view is IBorder border && border.Border != null)
After:
			{
*/

/* Unmerged change from project 'Core(net8.0-android)'
Before:
				return true;

#if ANDROID
			if (view?.InputTransparent == true)
After:
			{
*/
			{
			{
				return true;
			}
			}

#if ANDROID
			if (view?.InputTransparent == true)
				return true;
			}

#if ANDROID
			if (view?.InputTransparent == true)
			{
				return true;
			}
#endif

#if ANDROID || IOS
			if (view is IBorder border && border.Border != null)
			{
			{
				return true;
			}

/* Unmerged change from project 'Core(net8.0-android)'
Before:
#elif WINDOWS
After:
			}
#elif WINDOWS
*/
			}

#if ANDROID
			if (view?.InputTransparent == true)
				return true;
#endif

#if ANDROID || IOS
			if (view is IBorder border && border.Border != null)
			{
				return true;
			}
#elif WINDOWS
			if (view is IBorderView border)
			{
				return border?.Shape != null || border?.Stroke != null;
			}
#endif
			return false;
		}
#endif

	}
}
