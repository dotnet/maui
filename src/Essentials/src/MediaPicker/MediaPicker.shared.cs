#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// The MediaPicker API lets a user pick or take a photo or video on the device.
	/// </summary>
	public interface IMediaPicker
	{
		/// <summary>
		/// Gets a value indicating whether capturing media is supported on this device.
		/// </summary>
		bool IsCaptureSupported { get; }

		/// <summary>
		/// Opens the media browser to select a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked photo. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		/// <remarks>When using <see cref="MediaPickerOptions.SelectionLimit"/> on this overload, it will <b>not</b> have effect.</remarks>
		[Obsolete("Switch to PickPhotosAsync which also allows multiple selections.")]
		Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the media browser to select photos.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A list of <see cref="FileResult"/> objects containing details of the picked photos. When the operation was cancelled by the user, this will return an empty list.</returns>
		/// <remarks>
		/// <para>On Android, not all picker user interfaces enforce the <see cref="MediaPickerOptions.SelectionLimit"/>.</para>
		/// <para>On Windows, <see cref="MediaPickerOptions.SelectionLimit"/> is not supported.</para>
		/// <para>Implement your own logic to ensure that the limit is maintained and/or notify the user on these platforms.</para>
		/// </remarks>
		Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the camera to take a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured photo. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the media browser to select a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked video. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		/// <remarks>When using <see cref="MediaPickerOptions.SelectionLimit"/> on this overload, it will <b>not</b> have effect.</remarks>
		[Obsolete("Switch to PickVideosAsync which also allows multiple selections.")]
		Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the media browser to select videos.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A list of <see cref="FileResult"/> objects containing details of the picked videos. When the operation was cancelled by the user, this will return an empty list.</returns>
		/// <remarks>
		/// <para>On Android, not all picker user interfaces enforce the <see cref="MediaPickerOptions.SelectionLimit"/>.</para>
		/// <para>On Windows, <see cref="MediaPickerOptions.SelectionLimit"/> is not supported.</para>
		/// <para>Implement your own logic to ensure that the limit is maintained and/or notify the user on these platforms.</para>
		/// </remarks>
		Task<List<FileResult>> PickVideosAsync(MediaPickerOptions? options = null);

		/// <summary>
		/// Opens the camera to take a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured video. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null);
	}

	/// <summary>
	/// The MediaPicker API lets a user pick or take a photo or video on the device.
	/// </summary>
	public static class MediaPicker
	{
		/// <summary>
		/// Gets a value indicating whether capturing media is supported on this device.
		/// </summary>
		public static bool IsCaptureSupported =>
			Default.IsCaptureSupported;

		/// <summary>
		/// Opens the media browser to select a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked photo. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		/// <remarks>When using <see cref="MediaPickerOptions.SelectionLimit"/> on this overload, it will <b>not</b> have effect.</remarks>
		[Obsolete("Switch to PickPhotosAsync which also allows multiple selections.")]
		public static Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null) =>
			Default.PickPhotoAsync(options);

		/// <inheritdoc cref="IMediaPicker.PickPhotosAsync(MediaPickerOptions?)"/>
		public static Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions? options = null) =>
			Default.PickPhotosAsync(options);

		/// <summary>
		/// Opens the camera to take a photo.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured photo. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		public static Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null) =>
			Default.CapturePhotoAsync(options);

		/// <summary>
		/// Opens the media browser to select a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the picked video. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		/// <remarks>When using <see cref="MediaPickerOptions.SelectionLimit"/> on this overload, it will <b>not</b> have effect.</remarks>
		[Obsolete("Switch to PickVideosAsync which also allows multiple selections.")]
		public static Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null) =>
			Default.PickVideoAsync(options);

		/// <inheritdoc cref="IMediaPicker.PickVideosAsync(MediaPickerOptions?)"/>
		public static Task<List<FileResult>> PickVideosAsync(MediaPickerOptions? options = null) =>
			Default.PickVideosAsync(options);

		/// <summary>
		/// Opens the camera to take a video.
		/// </summary>
		/// <param name="options">Pick options to use.</param>
		/// <returns>A <see cref="FileResult"/> object containing details of the captured video. When the operation was cancelled by the user, this will return <see langword="null"/>.</returns>
		public static Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null) =>
			Default.CaptureVideoAsync(options);

		static IMediaPicker? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IMediaPicker Default =>
			defaultImplementation ??= new MediaPickerImplementation();

		internal static void SetDefault(IMediaPicker? implementation) =>
			defaultImplementation = implementation;
	}
	/// <summary>
	/// Pick options for picking media from the device.
	/// </summary>
	public class MediaPickerOptions
	{
		private int compressionQuality = 100;

		/// <summary>
		/// Gets or sets the compression quality for picked media.
		/// The value should be between 0 and 100, where 0 is the lowest quality (most compression) and 100 is the highest quality (least compression).
		/// </summary>
		/// <remarks>
		/// <para>Please note that performance might be affected by the compression quality, especially on lower-end devices.</para>
		/// <para>For JPEG images, this controls the lossy compression quality directly.</para>
		/// <para>For PNG images, values below 90 will convert to JPEG format for better compression. Values 90-99 will scale down the PNG image. Value 100 preserves original PNG format and quality.</para>
		/// </remarks>
		public int CompressionQuality
		{
			get => compressionQuality;
			set => compressionQuality = Math.Max(0, Math.Min(100, value));
		}

		/// <summary>
		/// Gets or sets the maximum width for image resizing.
		/// When set, images will be resized to fit within this width while preserving aspect ratio.
		/// A value of 0 or null means no width constraint.
		/// </summary>
		/// <remarks>
		/// <para>This property only applies to images. It has no effect on video files.</para>
		/// <para>The image will be resized to fit within the specified maximum dimensions while maintaining aspect ratio.
		/// If both MaximumWidth and MaximumHeight are specified, the image will be scaled to fit within both constraints.
		/// This resizing is applied before any compression quality settings.</para>
		/// </remarks>
		public int? MaximumWidth { get; set; }

		/// <summary>
		/// Gets or sets the maximum height for image resizing.
		/// When set, images will be resized to fit within this height while preserving aspect ratio.
		/// A value of 0 or null means no height constraint.
		/// </summary>
		/// <remarks>
		/// <para>This property only applies to images. It has no effect on video files.</para>
		/// <para>The image will be resized to fit within the specified maximum dimensions while maintaining aspect ratio.
		/// If both MaximumWidth and MaximumHeight are specified, the image will be scaled to fit within both constraints.
		/// This resizing is applied before any compression quality settings.</para>
		/// </remarks>
		public int? MaximumHeight { get; set; }

		/// <summary>
		/// Gets or sets the title that is displayed when picking media.
		/// </summary>
		/// <remarks>This title is not guaranteed to be shown on all operating systems.</remarks>
		public string? Title { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of items that can be selected. Default value is 1.
		/// </summary>
		/// <remarks>
		/// A value of 0 means no limit.
		/// </remarks>
		public int SelectionLimit { get; set; } = 1;

		/// <summary>
		/// Gets or sets whether to automatically rotate the image based on EXIF orientation data.
		/// When true, the image will be rotated to the correct orientation.
		/// Default value is false.
		/// </summary>
		/// <remarks>
		/// <para>This property only applies to images. It has no effect on video files.</para>
		/// <para>When enabled, the EXIF orientation data will be applied to correctly orient the image,
		/// and the orientation flag will be reset to avoid duplicate rotations in image viewers.</para>
		/// <para>This rotation happens before any resizing or compression is applied.</para>
		/// <para>Please note that performance might be affected by the rotation operation, especially on lower-end devices.</para>
		/// </remarks>
		public bool RotateImage { get; set; } = false;

		/// <summary>
		/// Gets or sets whether to preserve metadata (including EXIF data) when processing images.
		/// When true, metadata from the original image will be preserved in the processed image.
		/// Default value is true.
		/// </summary>
		/// <remarks>
		/// <para>This property only applies to images. It has no effect on video files.</para>
		/// <para>When enabled, metadata such as EXIF data, GPS information, camera settings, and timestamps
		/// will be copied from the original image to the processed image during operations like resizing,
		/// compression, or rotation.</para>
		/// <para>Setting this to false may result in smaller file sizes but will lose the image's metadata.</para>
		/// <para>Currently not supported on Windows.</para>
		/// </remarks>
		public bool PreserveMetaData { get; set; } = true;
	}
}
