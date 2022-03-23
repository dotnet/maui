#nullable enable
using System.Threading.Tasks;
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

	public static class MediaPicker
	{
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
