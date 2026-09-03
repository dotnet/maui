using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Media;

namespace Microsoft.Maui
{
	/// <summary>
	/// Internal helper that routes <see cref="ViewExtensions.CaptureAsync(IView)"/>
	/// and <see cref="WindowExtensions.CaptureAsync(IWindow)"/> through a keyed DI
	/// hook when MAUI is built for a non-built-in platform TFM and therefore has no
	/// compile-time screenshot implementation.
	/// </summary>
	/// <remarks>
	/// Third-party platform backends (e.g. macOS AppKit, Linux/GTK) register a
	/// <see cref="Func{T, TResult}"/> of <see cref="object"/> to
	/// <see cref="Task{TResult}"/> of nullable <see cref="IScreenshotResult"/>
	/// (i.e. <c>Func&lt;object, Task&lt;IScreenshotResult?&gt;&gt;</c>) under one of
	/// the well-known keys defined on this type. The lambda receives the handler's
	/// <see cref="IElementHandler.PlatformView"/> object and returns a task whose
	/// result is the screenshot (or <see langword="null"/> if capture is not
	/// supported for that view). A hook that returns a <see langword="null"/>
	/// task is treated as unsupported and produces a <see langword="null"/> result.
	/// This contract intentionally uses only BCL types so it can ship without any
	/// MAUI public API addition.
	/// </remarks>
	static class ScreenshotDispatch
	{
		/// <summary>
		/// DI service key for the <see cref="IView"/> screenshot hook.
		/// </summary>
		public const string ViewCaptureKey = "Microsoft.Maui.ViewCapture";

		/// <summary>
		/// DI service key for the <see cref="IWindow"/> screenshot hook.
		/// </summary>
		public const string WindowCaptureKey = "Microsoft.Maui.WindowCapture";

		public static Task<IScreenshotResult?> CaptureAsync(IElementHandler? handler, string serviceKey)
		{
			var platformView = handler?.PlatformView;
			if (platformView is null)
				return Task.FromResult<IScreenshotResult?>(null);

			if (handler!.MauiContext?.Services is not IKeyedServiceProvider keyedProvider)
				return Task.FromResult<IScreenshotResult?>(null);

			var capture = keyedProvider.GetKeyedService<Func<object, Task<IScreenshotResult?>>>(serviceKey);

			if (capture is null)
				return Task.FromResult<IScreenshotResult?>(null);

			return capture(platformView) ?? Task.FromResult<IScreenshotResult?>(null);
		}
	}
}
