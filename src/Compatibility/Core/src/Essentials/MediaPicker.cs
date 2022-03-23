#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPicker']/Docs" />
	public static partial class MediaPicker
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='IsCaptureSupported']/Docs" />
		public static bool IsCaptureSupported
			=> Current.IsCaptureSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='PickPhotoAsync']/Docs" />
		public static Task<FileResult> PickPhotoAsync(MediaPickerOptions options = null) =>
			Current.PickPhotoAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='CapturePhotoAsync']/Docs" />
		public static Task<FileResult> CapturePhotoAsync(MediaPickerOptions options = null)
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return Current.CapturePhotoAsync(options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='PickVideoAsync']/Docs" />
		public static Task<FileResult> PickVideoAsync(MediaPickerOptions options = null) =>
			Current.PickVideoAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='CaptureVideoAsync']/Docs" />
		public static Task<FileResult> CaptureVideoAsync(MediaPickerOptions options = null)
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return Current.CaptureVideoAsync(options);
		}

#nullable enable
		static IMediaPicker? currentImplementation;

		public static IMediaPicker Current =>
			currentImplementation ??= new MediaPickerImplementation();

		internal static void SetCurrent(IMediaPicker? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
