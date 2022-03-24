#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPicker']/Docs" />
	public static partial class MediaPicker
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='IsCaptureSupported']/Docs" />
		public static bool IsCaptureSupported =>
			Default.IsCaptureSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='PickPhotoAsync']/Docs" />
		public static Task<FileResult> PickPhotoAsync(MediaPickerOptions? options = null) =>
			Default.PickPhotoAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='CapturePhotoAsync']/Docs" />
		public static Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null) =>
			Default.CapturePhotoAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='PickVideoAsync']/Docs" />
		public static Task<FileResult> PickVideoAsync(MediaPickerOptions? options = null) =>
			Default.PickVideoAsync(options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="//Member[@MemberName='CaptureVideoAsync']/Docs" />
		public static Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null) =>
			Default.CaptureVideoAsync(options);
	}
}
