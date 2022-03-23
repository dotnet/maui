#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Media
{
	public interface IScreenshot
	{
		bool IsCaptureSupported { get; }

		Task<IScreenshotResult> CaptureAsync();
	}

	public interface IPlatformScreenshot : IScreenshot
	{
#if ANDROID
		Task<IScreenshotResult> CaptureAsync(Android.Views.View view);
#elif IOS || MACCATALYST
		Task<IScreenshotResult> CaptureAsync(UIKit.UIWindow window);
		Task<IScreenshotResult> CaptureAsync(UIKit.UIView view);
		Task<IScreenshotResult> CaptureAsync(CoreAnimation.CALayer layer, bool skipChildren);
#elif WINDOWS
		Task<IScreenshotResult> CaptureAsync(UI.Xaml.UIElement element);
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.IScreenshotResult']/Docs" />
	public interface IScreenshotResult
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='Width']/Docs" />
		int Width { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='Height']/Docs" />
		int Height { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='OpenReadAsync']/Docs" />
		Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100);

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='CopyToAsync']/Docs" />
		Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100);

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='ToPixelBufferAsync']/Docs" />
		Task<byte[]> ToPixelBufferAsync();
	}

	public static class Screenshot
	{
		static IScreenshot? currentImplementation;

		public static IScreenshot Current =>
			currentImplementation ??= new ScreenshotImplementation();

		internal static void SetCurrent(IScreenshot? implementation) =>
			currentImplementation = implementation;
	}

	public static class ScreenshotExtensions
	{
#if ANDROID
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, Android.Views.View view)
		{
			if (screenshot is not IPlatformScreenshot platform)
				throw new PlatformNotSupportedException("This implementation of IScreenshot does not implement IPlatformScreenshot.");

			return platform.CaptureAsync(view);
		}
#elif IOS || MACCATALYST
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, UIKit.UIWindow window)
		{
			if (screenshot is not IPlatformScreenshot platform)
				throw new PlatformNotSupportedException("This implementation of IScreenshot does not implement IPlatformScreenshot.");

			return platform.CaptureAsync(window);
		}
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, UIKit.UIView view)
		{
			if (screenshot is not IPlatformScreenshot platform)
				throw new PlatformNotSupportedException("This implementation of IScreenshot does not implement IPlatformScreenshot.");

			return platform.CaptureAsync(view);
		}
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, CoreAnimation.CALayer layer, bool skipChildren)
		{
			if (screenshot is not IPlatformScreenshot platform)
				throw new PlatformNotSupportedException("This implementation of IScreenshot does not implement IPlatformScreenshot.");

			return platform.CaptureAsync(layer, skipChildren);
		}
#elif WINDOWS
		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, UI.Xaml.UIElement element)
		{
			if (screenshot is not IPlatformScreenshot platform)
				throw new PlatformNotSupportedException("This implementation of IScreenshot does not implement IPlatformScreenshot.");

			return platform.CaptureAsync(element);
		}
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ScreenshotFormat']/Docs" />
	public enum ScreenshotFormat
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="//Member[@MemberName='Png']/Docs" />
		Png,
		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="//Member[@MemberName='Jpeg']/Docs" />
		Jpeg
	}

	partial class ScreenshotResult : IScreenshotResult
	{
		public int Width { get; }

		public int Height { get; }

		public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
			=> PlatformOpenReadAsync(format, quality);

		public Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
			=> PlatformCopyToAsync(destination, format, quality);

		public Task<byte[]> ToPixelBufferAsync()
			=> PlatformToPixelBufferAsync();
	}
}
