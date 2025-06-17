using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIWindow;
using Microsoft.Maui.Platform;
#elif MONOANDROID
using PlatformView = Android.App.Activity;
using Microsoft.Maui.Platform;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Window;
using Microsoft.Maui.Platform;
#elif TIZEN
using PlatformView =  Tizen.NUI.Window;
#endif

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		public static Task<IScreenshotResult?> CaptureAsync(this IWindow window)
		{
#if PLATFORM
			if (window?.Handler?.PlatformView is not PlatformView platformView)
				return Task.FromResult<IScreenshotResult?>(null);

			if (!Screenshot.Default.IsCaptureSupported)
				return Task.FromResult<IScreenshotResult?>(null);

			return CaptureAsync(platformView);
#else
			return Task.FromResult<IScreenshotResult?>(null);
#endif
		}

		public static float GetDisplayDensity(this IWindow window)
		{
#if PLATFORM
			if (window?.Handler?.PlatformView is not PlatformView platformView)
				return 1.0f;

			return GetPlatformDisplayDensity(platformView);
#else
			return 1.0f;
#endif
		}


#if PLATFORM
		async static Task<IScreenshotResult?> CaptureAsync(PlatformView window) =>
			await Screenshot.Default.CaptureAsync(window);

		static float GetPlatformDisplayDensity(PlatformView platformView)
		{
#if __IOS__ || MACCATALYST
			// Use the existing UIWindow extension method
			return platformView.GetDisplayDensity();
#elif MONOANDROID
			// Get Context from Activity and use existing extension method
			return platformView.GetDisplayDensity();
#elif WINDOWS
			// Use the existing UI.Xaml.Window extension method  
			return platformView.GetDisplayDensity();
#elif TIZEN
			// For Tizen, return default density for now
			return 1.0f;
#else
			return 1.0f;
#endif
		}
#endif
	}
}
