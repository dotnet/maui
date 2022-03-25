using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Media
{
	public partial class ScreenshotImplementation : IScreenshot
	{
		public bool IsCaptureSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IScreenshotResult> CaptureAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	internal partial class ScreenshotResult
	{
		ScreenshotResult()
		{
		}

		internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
