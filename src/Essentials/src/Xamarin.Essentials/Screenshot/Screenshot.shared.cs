using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
	public static partial class Screenshot
	{
		public static bool IsCaptureSupported
			=> PlatformIsCaptureSupported;

		public static Task<ScreenshotResult> CaptureAsync()
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return PlatformCaptureAsync();
		}
	}

	public partial class ScreenshotResult
	{
		public int Width { get; }

		public int Height { get; }

		public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png) =>
			PlatformOpenReadAsync(format);
	}

	public enum ScreenshotFormat
	{
		Png,
		Jpeg
	}
}
