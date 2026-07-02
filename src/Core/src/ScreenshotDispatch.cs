using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Media;

namespace Microsoft.Maui
{
	/// <summary>
	/// Internal helper that routes <see cref="ViewExtensions.CaptureAsync(IView)"/>
	/// and <see cref="WindowExtensions.CaptureAsync(IWindow)"/> through the registered
	/// screenshot service when MAUI is built for a non-built-in platform TFM and
	/// therefore has no compile-time screenshot implementation.
	/// </summary>
	/// <remarks>
	/// Third-party platform backends (e.g. macOS AppKit, Linux/GTK) register an
	/// <see cref="IScreenshot"/> implementation that also implements
	/// <see cref="IViewScreenshot"/> in the app's <see cref="System.IServiceProvider"/>.
	/// The dispatch resolves that service from the handler's
	/// <see cref="IElementHandler.MauiContext"/> and forwards the handler's platform
	/// view (or, for views, its container) to
	/// <see cref="IViewScreenshot.CaptureViewAsync(object)"/>. When no capable
	/// service is registered (or capture is unsupported) the result is
	/// <see langword="null"/>, preserving the extension methods' graceful contract.
	/// </remarks>
	static class ScreenshotDispatch
	{
		public static Task<IScreenshotResult?> CaptureAsync(IElementHandler? handler, object? captureView)
		{
			if (captureView is null)
				return Task.FromResult<IScreenshotResult?>(null);

			if (handler?.MauiContext?.Services?.GetService(typeof(IScreenshot)) is not IScreenshot screenshot
				|| !screenshot.IsCaptureSupported
				|| screenshot is not IViewScreenshot viewScreenshot)
			{
				return Task.FromResult<IScreenshotResult?>(null);
			}

			return viewScreenshot.CaptureViewAsync(captureView) ?? Task.FromResult<IScreenshotResult?>(null);
		}
	}
}
