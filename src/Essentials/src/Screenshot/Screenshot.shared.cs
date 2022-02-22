#nullable enable

using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public interface IScreenshot
	{
		bool IsCaptureSupported { get; }

		Task<IScreenshotResult> CaptureAsync();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.IScreenshotResult']/Docs" />
	public interface IScreenshotResult
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='Width']/Docs" />
		int Width { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='Height']/Docs" />
		int Height { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/IScreenshotResult.xml" path="//Member[@MemberName='OpenReadAsync']/Docs" />
		Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png);
	}

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

		static IScreenshot? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IScreenshot Current =>
			currentImplementation ??= new Implementations.ScreenshotImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IScreenshot? implementation) =>
			currentImplementation = implementation;
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

namespace Microsoft.Maui.Essentials.Implementations
{
	internal partial class ScreenshotResult : IScreenshotResult
	{
		public int Width { get; }
		
		public int Height { get; }

		public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png)
			=> PlatformOpenReadAsync(format);
	}
}
