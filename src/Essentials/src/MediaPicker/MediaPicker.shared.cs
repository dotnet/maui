#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// The MediaPicker API lets a user pick or take a photo or video on the device.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never), Obsolete(MediaPicker.ObsoleteMessage)]
	public interface IMediaPicker
	{
		/// <summary>
		/// Gets a value indicating whether capturing media is supported on this device.
		/// </summary>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		bool IsCaptureSupported { get; }

		/// <summary>
		/// Opens the media browser to select a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked photo.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		Task<FileResult> PickPhotoAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the camera to take a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured photo.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the media browser to select a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked video.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		Task<FileResult> PickVideoAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the camera to take a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured video.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null);
	}

	/// <summary>
	/// The MediaPicker API lets a user pick or take a photo or video on the device.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never), Obsolete(MediaPicker.ObsoleteMessage)]
	public static class MediaPicker
	{
		internal const string ObsoleteMessage = "Use MediaGallery";

		/// <summary>
		/// Gets a value indicating whether capturing media is supported on this device.
		/// </summary>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public static bool IsCaptureSupported =>
			Default.IsCaptureSupported;

		/// <summary>
		/// Opens the media browser to select a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked photo.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public static Task<FileResult> PickPhotoAsync(MediaPickerOptions? options = null) =>
			Default.PickPhotoAsync(options);

		/// <summary>
		/// Opens the camera to take a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured photo.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public static Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options = null) =>
			Default.CapturePhotoAsync(options);

		/// <summary>
		/// Opens the media browser to select a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked video.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public static Task<FileResult> PickVideoAsync(MediaPickerOptions? options = null) =>
			Default.PickVideoAsync(options);

		/// <summary>
		/// Opens the camera to take a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured video.</returns>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public static Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options = null) =>
			Default.CaptureVideoAsync(options);

		static IMediaPicker? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public static IMediaPicker Default =>
			defaultImplementation ??= new MediaPickerImplementation();

		[Obsolete(MediaPicker.ObsoleteMessage)]
		internal static void SetDefault(IMediaPicker? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Pick options for picking media from the device.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never), Obsolete(MediaPicker.ObsoleteMessage)]
	public class MediaPickerOptions
	{
		/// <summary>
		/// Gets or sets the title that is displayed when picking media.
		/// </summary>
		/// <remarks>This title is not guaranteed to be shown on all operating systems.</remarks>
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public string? Title { get; set; }
	}

	[EditorBrowsable(EditorBrowsableState.Never), Obsolete(MediaPicker.ObsoleteMessage)]
	class MediaPickerImplementation : IMediaPicker
	{
		[Obsolete(MediaPicker.ObsoleteMessage)]
		public bool IsCaptureSupported
			=> MediaGallery.CheckCaptureSupport(MediaFileType.Image)
				&& MediaGallery.CheckCaptureSupport(MediaFileType.Video);

		[Obsolete(MediaPicker.ObsoleteMessage)]
		public Task<FileResult> PickPhotoAsync(MediaPickerOptions? options)
			=> MediaGallery.PickAsync(
				new(options?.Title, 1, default, MediaFileType.Image));

		[Obsolete(MediaPicker.ObsoleteMessage)]
		public Task<FileResult> PickVideoAsync(MediaPickerOptions? options)
			=> MediaGallery.PickAsync(
				new(options?.Title, 1, default, MediaFileType.Video));

		[Obsolete(MediaPicker.ObsoleteMessage)]
		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions? options)
			=> MediaGallery.CaptureAsync(MediaFileType.Image);

		[Obsolete(MediaPicker.ObsoleteMessage)]
		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions? options)
			=> MediaGallery.CaptureAsync(MediaFileType.Video);
	}
}
