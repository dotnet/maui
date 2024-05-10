#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// The Screenshot API lets you take a capture of the current displayed screen of the app.
	/// </summary>
	public interface IScreenshot
	{
		/// <summary>
		/// Gets a value indicating whether capturing screenshots is supported on this device.
		/// </summary>
		bool IsCaptureSupported { get; }

		/// <summary>
		/// Captures a screenshot of the current screen of the running application.
		/// </summary>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult> CaptureAsync();
	}

	/// <summary>
	/// Provides abstractions for the platform screenshot methods when using Screenshot.
	/// </summary>
	public interface IPlatformScreenshot : IScreenshot
	{
#if ANDROID
		/// <summary>
		/// Captures a screenshot of the specified activity.
		/// </summary>
		/// <param name="activity">The activity to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult> CaptureAsync(Android.App.Activity activity);

		/// <summary>
		/// Captures a screenshot of the specified view.
		/// </summary>
		/// <param name="view">The view to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult?> CaptureAsync(Android.Views.View view);
#elif IOS || MACCATALYST
		/// <summary>
		/// Captures a screenshot of the specified window.
		/// </summary>
		/// <param name="window">The window to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult> CaptureAsync(UIKit.UIWindow window);

		/// <summary>
		/// Captures a screenshot of the specified view.
		/// </summary>
		/// <param name="view">The view to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult?> CaptureAsync(UIKit.UIView view);

		/// <summary>
		/// Captures a screenshot of the specified layer.
		/// </summary>
		/// <param name="layer">The layer to capture.</param>
		/// <param name="skipChildren">Specifies whether to include the child layers or not.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult?> CaptureAsync(CoreAnimation.CALayer layer, bool skipChildren);
#elif WINDOWS
		/// <summary>
		/// Captures a screenshot of the specified window.
		/// </summary>
		/// <param name="window">The window to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult> CaptureAsync(UI.Xaml.Window window);

		/// <summary>
		/// Captures a screenshot of the specified user-interface element.
		/// </summary>
		/// <param name="element">The user-interface element to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult?> CaptureAsync(UI.Xaml.UIElement element);
#elif TIZEN

		/// <summary>
		/// Captures a screenshot of the specified window.
		/// </summary>
		/// <param name="window">The window to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult> CaptureAsync(Tizen.NUI.Window window);

		/// <summary>
		/// Captures a screenshot of the specified view.
		/// </summary>
		/// <param name="view">The view to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		Task<IScreenshotResult?> CaptureAsync(Tizen.NUI.BaseComponents.View view);
#endif
	}

	/// <summary>
	/// A representation of a screenshot, as a result of a screenshot captured by the user.
	/// </summary>
	public interface IScreenshotResult
	{
		/// <summary>
		/// The width of this screenshot in pixels.
		/// </summary>
		int Width { get; }

		/// <summary>
		/// The height of this screenshot in pixels.
		/// </summary>
		int Height { get; }

		/// <summary>
		/// Opens a <see cref="Stream"/> to the corresponding screenshot file on the filesystem.
		/// </summary>
		/// <param name="format">The image format used to read this screenshot.</param>
		/// <param name="quality">The quality used to read this screenshot. Quality only applies when <see cref="ScreenshotFormat.Jpeg"/> is used.</param>
		/// <returns>A <see cref="Stream"/> containing the screenshot file data.</returns>
		Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="destination">The destination <see cref="Stream"/> to copy the screenshot file data to.</param>
		/// <param name="format">The image format used to copy this screenshot.</param>
		/// <param name="quality">The quality used to copy this screenshot. Quality only applies when <see cref="ScreenshotFormat.Jpeg"/> is used.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100);
	}

	/// <summary>
	/// The Screenshot API lets you take a capture of the current displayed screen of the app.
	/// </summary>
	public static partial class Screenshot
	{
		/// <summary>
		/// Gets a value indicating whether capturing screenshots is supported on this device.
		/// </summary>
		public static bool IsCaptureSupported
			=> Default.IsCaptureSupported;

		/// <summary>
		/// Captures a screenshot of the current screen of the running application.
		/// </summary>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		/// <exception cref="FeatureNotSupportedException">Thrown when <see cref="IsCaptureSupported"/> is <see langword="false"/>.</exception>
		public static Task<IScreenshotResult> CaptureAsync()
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return Default.CaptureAsync();
		}

		static IScreenshot? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IScreenshot Default =>
			defaultImplementation ??= new ScreenshotImplementation();

		internal static void SetDefault(IScreenshot? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// This class contains static extension methods for use with <see cref="IScreenshot"/>.
	/// </summary>
	public static class ScreenshotExtensions
	{
		static IPlatformScreenshot AsPlatform(this IScreenshot screenshot)
		{
			if (screenshot is not IPlatformScreenshot platform)
				throw new PlatformNotSupportedException("This implementation of IScreenshot does not implement IPlatformScreenshot.");

			return platform;
		}

#if ANDROID
		/// <summary>
		/// Captures a screenshot of the specified activity.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="activity">The activity to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, Android.App.Activity activity) =>
			screenshot.AsPlatform().CaptureAsync(activity);

		/// <summary>
		/// Captures a screenshot of the specified view.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="view">The view to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, Android.Views.View view) =>
			screenshot.AsPlatform().CaptureAsync(view);

#elif IOS || MACCATALYST
		/// <summary>
		/// Captures a screenshot of the specified window.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="window">The window to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, UIKit.UIWindow window) =>
			screenshot.AsPlatform().CaptureAsync(window);

		/// <summary>
		/// Captures a screenshot of the specified view.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="view">The view to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, UIKit.UIView view) =>
			screenshot.AsPlatform().CaptureAsync(view);

		/// <summary>
		/// Captures a screenshot of the specified layer.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="layer">The layer to capture.</param>
		/// <param name="skipChildren">Specifies whether to include the child layers or not.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, CoreAnimation.CALayer layer, bool skipChildren) =>
			screenshot.AsPlatform().CaptureAsync(layer, skipChildren);

#elif WINDOWS
		/// <summary>
		/// Captures a screenshot of the specified window.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="window">The window to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, UI.Xaml.Window window) =>
			screenshot.AsPlatform().CaptureAsync(window);

		/// <summary>
		/// Captures a screenshot of the specified user-interface element.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="element">The user-interface element to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, UI.Xaml.UIElement element) =>
			screenshot.AsPlatform().CaptureAsync(element);

#elif TIZEN
		/// <summary>
		/// Captures a screenshot of the specified window.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="window">The window to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, Tizen.NUI.Window window) =>
			screenshot.AsPlatform().CaptureAsync(window);

		/// <summary>
		/// Captures a screenshot of the specified view.
		/// </summary>
		/// <param name="screenshot">The object this method is invoked on.</param>
		/// <param name="view">The view to capture.</param>
		/// <returns>An instance of <see cref="IScreenshotResult"/> with information about the captured screenshot.</returns>
		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, Tizen.NUI.BaseComponents.View view) =>
			screenshot.AsPlatform().CaptureAsync(view);
#endif
	}

	/// <summary>
	/// The possible formats for reading screenshot images.
	/// </summary>
	public enum ScreenshotFormat
	{
		/// <summary>Read the screenshot image as a PNG image.</summary>
		Png,

		/// <summary>Read the screenshot image as a JPEG image.</summary>
		Jpeg
	}

	/// <summary>
	/// A representation of a screenshot, as a result of a screenshot captured by the user.
	/// </summary>
	partial class ScreenshotResult : IScreenshotResult
	{
		public int Width { get; }

		public int Height { get; }

		public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
			=> PlatformOpenReadAsync(format, quality);

		public Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
			=> PlatformCopyToAsync(destination, format, quality);
	}
}
