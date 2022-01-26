using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPicker']/Docs" />
	public static partial class MediaPicker
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='IsCaptureSupported']/Docs" />
		public static bool IsCaptureSupported
			=> PlatformIsCaptureSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='PickPhotoAsync']/Docs" />
		public static Task<FileResult> PickPhotoAsync(MediaPickerOptions options = null) =>
			PlatformPickPhotoAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='CapturePhotoAsync']/Docs" />
		public static Task<FileResult> CapturePhotoAsync(MediaPickerOptions options = null)
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return PlatformCapturePhotoAsync(options);
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='PickVideoAsync']/Docs" />
		public static Task<FileResult> PickVideoAsync(MediaPickerOptions options = null) =>
			PlatformPickVideoAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='CaptureVideoAsync']/Docs" />
		public static Task<FileResult> CaptureVideoAsync(MediaPickerOptions options = null)
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return PlatformCaptureVideoAsync(options);
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPickerOptions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPickerOptions']/Docs" />
	public class MediaPickerOptions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPickerOptions.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; set; }
	}
}
