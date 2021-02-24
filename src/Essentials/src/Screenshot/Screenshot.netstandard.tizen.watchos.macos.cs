using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class Screenshot
	{
		static bool PlatformIsCaptureSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<ScreenshotResult> PlatformCaptureAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	public partial class ScreenshotResult
	{
		ScreenshotResult()
		{
		}

		internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
