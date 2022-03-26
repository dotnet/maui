#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	public interface IMediaPicker
	{
		bool IsCaptureSupported { get; }

		Task<FileResult> PickPhotoAsync(MediaPickerOptions? options = null);

		Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null);

		Task<FileResult> PickVideoAsync(MediaPickerOptions? options = null);

		Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPicker']/Docs" />
	public static class MediaPicker
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

		static IMediaPicker? defaultImplementation;

		public static IMediaPicker Default =>
			defaultImplementation ??= new MediaPickerImplementation();

		internal static void SetDefault(IMediaPicker? implementation) =>
			defaultImplementation = implementation;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPickerOptions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPickerOptions']/Docs" />
	public class MediaPickerOptions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPickerOptions.xml" path="//Member[@MemberName='Title']/Docs" />
		public string? Title { get; set; }
	}
}
