using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIWindow;
#elif MONOANDROID
using PlatformView = Android.App.Activity;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		public static Task<Stream?> CaptureAsync(this IWindow window, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
		{
#if PLATFORM
			if (window?.Handler?.PlatformView is not PlatformView platformWindow)
				return Task.FromResult<Stream?>(null);

			if (!Screenshot.Current.IsCaptureSupported)
				return Task.FromResult<Stream?>(null);

			return CaptureAsync(platformWindow, format, quality);
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
