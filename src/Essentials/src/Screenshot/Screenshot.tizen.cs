using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using NView = Tizen.NUI.BaseComponents.View;
using Window = Tizen.NUI.Window;

namespace Microsoft.Maui.Media
{
	// TODO: Need to impl
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
		public bool IsCaptureSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IScreenshotResult> CaptureAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IScreenshotResult> CaptureAsync(Window window) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IScreenshotResult> CaptureAsync(NView view) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	partial class ScreenshotResult
	{
		ScreenshotResult()
		{
		}

		Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<byte[]> PlatformToPixelBufferAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
