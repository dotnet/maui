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
		/// implementation, capture is routed through a keyed DI hook. Third-party
		/// platform backends can opt in by registering a
		/// <see cref="Func{T, TResult}"/> of <see cref="object"/> to
		/// <c>Task&lt;IScreenshotResult?&gt;</c> under the service key
		/// <c>"Microsoft.Maui.WindowCapture"</c>:
		/// <code>
		/// builder.Services.AddKeyedSingleton&lt;Func&lt;object, Task&lt;IScreenshotResult?&gt;&gt;&gt;(
		///     "Microsoft.Maui.WindowCapture",
		///     (_, _) =&gt; platformWindow =&gt; ((AppKit.NSWindow)platformWindow).CaptureAsync());
		/// </code>
		/// If no hook is registered (or the <see cref="IElementHandler.PlatformView"/>
		/// is <see langword="null"/>), the returned task resolves to
		/// <see langword="null"/>.
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
			return ScreenshotDispatch.CaptureAsync(window?.Handler, ScreenshotDispatch.WindowCaptureKey);
#endif
		}


#if PLATFORM
		async static Task<IScreenshotResult?> CaptureAsync(PlatformView window) =>
			await Screenshot.Default.CaptureAsync(window);
#endif
	}
}
