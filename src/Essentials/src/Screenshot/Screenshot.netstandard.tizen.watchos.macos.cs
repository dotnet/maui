using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Screenshot']/Docs" />
	public static partial class Screenshot
	{
		static bool PlatformIsCaptureSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<ScreenshotResult> PlatformCaptureAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ScreenshotResult']/Docs" />
	public partial class ScreenshotResult
	{
		ScreenshotResult()
		{
		}

		internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
