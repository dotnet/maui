using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public partial class ScreenshotImplementation : IScreenshot
	{
		public bool IsCaptureSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<IScreenshotResult> CaptureAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ScreenshotResult']/Docs" />
	internal partial class ScreenshotResult
	{
		ScreenshotResult()
		{
		}

		internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
