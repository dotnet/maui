#nullable enable
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
		Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null);

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
		Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null);

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
		public static Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null) =>
			Default.PickPhotoAsync(options);

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
		public static Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null) =>
			Default.PickVideoAsync(options);

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
		/// <summary>
		/// Gets or sets the title that is displayed when picking media.
		/// </summary>
		/// <remarks>This title is not guaranteed to be shown on all operating systems.</remarks>
		public string? Title { get; set; }
	}
}
