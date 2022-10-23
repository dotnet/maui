#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

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
		Task<IScreenshotResult> CaptureAsync(Android.App.Activity activity);
		Task<IScreenshotResult?> CaptureAsync(Android.Views.View view);
#elif IOS || MACCATALYST
		Task<IScreenshotResult> CaptureAsync(UIKit.UIWindow window);
		Task<IScreenshotResult?> CaptureAsync(UIKit.UIView view);
		Task<IScreenshotResult?> CaptureAsync(CoreAnimation.CALayer layer, bool skipChildren);
#elif WINDOWS
		Task<IScreenshotResult> CaptureAsync(UI.Xaml.Window window);
		Task<IScreenshotResult?> CaptureAsync(UI.Xaml.UIElement element);
#elif TIZEN
		Task<IScreenshotResult> CaptureAsync(Tizen.NUI.Window window);
		Task<IScreenshotResult?> CaptureAsync(Tizen.NUI.BaseComponents.View view);
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.IScreenshotResult']/Docs/*" />
	public interface IScreenshotResult
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='Width']/Docs/*" />
		int Width { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='Height']/Docs/*" />
		int Height { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='OpenReadAsync']/Docs/*" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100);
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='CopyToAsync']/Docs/*" />
		Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Screenshot']/Docs/*" />
	public static partial class Screenshot
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="//Member[@MemberName='IsCaptureSupported']/Docs/*" />
		public static bool IsCaptureSupported
			=> Default.IsCaptureSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="//Member[@MemberName='CaptureAsync']/Docs/*" />
		public static Task<IScreenshotResult> CaptureAsync()
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return Default.CaptureAsync();
		}

		static IScreenshot? defaultImplementation;

		public static IScreenshot Default =>
			defaultImplementation ??= new ScreenshotImplementation();

		internal static void SetDefault(IScreenshot? implementation) =>
			defaultImplementation = implementation;
	}

	public static class ScreenshotExtensions
	{
		static IPlatformScreenshot AsPlatform(this IScreenshot screenshot)
		{
			if (screenshot is not IPlatformScreenshot platform)
				throw new PlatformNotSupportedException("This implementation of IScreenshot does not implement IPlatformScreenshot.");

			return platform;
		}

#if ANDROID

		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, Android.App.Activity activity) =>
			screenshot.AsPlatform().CaptureAsync(activity);

		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, Android.Views.View view) =>
			screenshot.AsPlatform().CaptureAsync(view);

#elif IOS || MACCATALYST

		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, UIKit.UIWindow window) =>
			screenshot.AsPlatform().CaptureAsync(window);

		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, UIKit.UIView view) =>
			screenshot.AsPlatform().CaptureAsync(view);

		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, CoreAnimation.CALayer layer, bool skipChildren) =>
			screenshot.AsPlatform().CaptureAsync(layer, skipChildren);

#elif WINDOWS

		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, UI.Xaml.Window window) =>
			screenshot.AsPlatform().CaptureAsync(window);

		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, UI.Xaml.UIElement element) =>
			screenshot.AsPlatform().CaptureAsync(element);

#elif TIZEN

		public static Task<IScreenshotResult> CaptureAsync(this IScreenshot screenshot, Tizen.NUI.Window window) =>
			screenshot.AsPlatform().CaptureAsync(window);

		public static Task<IScreenshotResult?> CaptureAsync(this IScreenshot screenshot, Tizen.NUI.BaseComponents.View view) =>
			screenshot.AsPlatform().CaptureAsync(view);

#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ScreenshotFormat']/Docs/*" />
	public enum ScreenshotFormat
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="//Member[@MemberName='Png']/Docs/*" />
		Png,
		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="//Member[@MemberName='Jpeg']/Docs/*" />
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
	}
}
