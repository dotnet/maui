using System;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IMediaPicker
	{
		bool IsCaptureSupported { get; }

		Task<FileResult> PickPhotoAsync(MediaPickerOptions options);

		Task<FileResult> CapturePhotoAsync(MediaPickerOptions options);

		Task<FileResult> PickVideoAsync(MediaPickerOptions options);

		Task<FileResult> CaptureVideoAsync(MediaPickerOptions options);
	}

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
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IMediaPicker Current =>
			currentImplementation ??= new MediaPickerImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IMediaPicker? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPickerOptions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPickerOptions']/Docs" />
	public class MediaPickerOptions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPickerOptions.xml" path="//Member[@MemberName='Title']/Docs" />
		public string Title { get; set; }
	}
}
