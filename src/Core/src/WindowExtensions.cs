using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIWindow;
#elif MONOANDROID
using PlatformView = Android.App.Activity;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Window;
#elif TIZEN
using PlatformView =  Tizen.NUI.Window;
#endif

namespace Microsoft.Maui
{
	public static partial class WindowExtensions
	{
		/// <summary>
		/// Captures a screenshot of the specified <paramref name="window"/>.
		/// </summary>
		/// <remarks>
		/// On non-built-in platform TFMs (e.g. <c>net10.0-macos</c> AppKit backends,
		/// <c>net10.0</c> Linux/GTK backends) where MAUI does not ship a screenshot
		/// implementation, capture is routed through the registered screenshot service.
		/// Third-party platform backends opt in by registering an <see cref="IScreenshot"/>
		/// implementation that also implements <see cref="IViewScreenshot"/> in the app's
		/// <see cref="System.IServiceProvider"/>:
		/// <code>
		/// builder.Services.AddSingleton&lt;IScreenshot, AppKitScreenshotImplementation&gt;();
		/// </code>
		/// The dispatch resolves that service from the handler's
		/// <see cref="IElementHandler.MauiContext"/> and forwards the window's platform view
		/// to <see cref="IViewScreenshot.CaptureViewAsync(object)"/>. When no capable service
		/// is registered (or the <see cref="IElementHandler.PlatformView"/> is
		/// <see langword="null"/>), the returned task resolves to <see langword="null"/>.
		/// </remarks>
		public static Task<IScreenshotResult?> CaptureAsync(this IWindow window)
		{
#if PLATFORM
			if (window?.Handler?.PlatformView is not PlatformView platformView)
				return Task.FromResult<IScreenshotResult?>(null);

			if (!Screenshot.Default.IsCaptureSupported)
				return Task.FromResult<IScreenshotResult?>(null);

			return CaptureAsync(platformView);
#else
			var handler = window?.Handler;
			return ScreenshotDispatch.CaptureAsync(handler, handler?.PlatformView);
#endif
		}


#if PLATFORM
		async static Task<IScreenshotResult?> CaptureAsync(PlatformView window) =>
			await Screenshot.Default.CaptureAsync(window);
#endif
	}
}
