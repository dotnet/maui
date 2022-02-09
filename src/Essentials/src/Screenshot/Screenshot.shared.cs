using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Screenshot']/Docs" />
	public static partial class Screenshot
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="//Member[@MemberName='IsCaptureSupported']/Docs" />
		public static bool IsCaptureSupported
			=> PlatformIsCaptureSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="//Member[@MemberName='CaptureAsync']/Docs" />
		public static Task<ScreenshotResult> CaptureAsync()
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return PlatformCaptureAsync();
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ScreenshotResult']/Docs" />
	public partial class ScreenshotResult
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotResult.xml" path="//Member[@MemberName='Width']/Docs" />
		public int Width { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotResult.xml" path="//Member[@MemberName='Height']/Docs" />
		public int Height { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotResult.xml" path="//Member[@MemberName='OpenReadAsync']/Docs" />
		public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png) =>
			PlatformOpenReadAsync(format);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="Type[@FullName='Microsoft.Maui.Essentials.ScreenshotFormat']/Docs" />
	public enum ScreenshotFormat
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="//Member[@MemberName='Png']/Docs" />
		Png,
		/// <include file="../../docs/Microsoft.Maui.Essentials/ScreenshotFormat.xml" path="//Member[@MemberName='Jpeg']/Docs" />
		Jpeg
	}
}
