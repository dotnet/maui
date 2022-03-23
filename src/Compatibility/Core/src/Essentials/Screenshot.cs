#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Screenshot']/Docs" />
	public static partial class Screenshot
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="//Member[@MemberName='IsCaptureSupported']/Docs" />
		public static bool IsCaptureSupported
			=> Current.IsCaptureSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Screenshot.xml" path="//Member[@MemberName='CaptureAsync']/Docs" />
		public static Task<IScreenshotResult> CaptureAsync()
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return Current.CaptureAsync();
		}

		static IScreenshot Current => Media.Screenshot.Current;
	}
}
