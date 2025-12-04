#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Storage
{
	/// <summary>
	/// Lets the user pick a file from the device's storage.
	/// </summary>
	/// <remarks>To enable iCloud capabilities in the file picker on iOS, you'll need to add the iCloud Documents in your <c>Entitlements.plist</c>.</remarks>
	public interface IFilePicker
	{
		/// <summary>
		/// Opens the default file picker to allow the user to pick a single file.
		/// </summary>
		/// <param name="options">File picker options to use; may be <see langword="null"/> for default options.</param>
		/// <returns>File picking result object, or <see langword="null"/> if picking was cancelled by the user.</returns>
		/// <remarks>
		/// File types can be specified in order to limit files that can be selected, using a
		/// <see cref="PickOptions"/> object. Note that this method may re-throw platform specific exceptions that
		/// occurred during file picking. When calling <see cref="PickAsync(PickOptions?)"/> again while showing a file
		/// picker, the <see cref="Task"/> object that was returned from the first call is cancelled. Be sure to
		/// also handle the <see cref="TaskCanceledException"/> in this case.
		/// </remarks>
		Task<FileResult?> PickAsync(PickOptions? options = null);

		/// <summary>
		/// Opens the default file picker to allow the user to pick one or more files.
		/// </summary>
		/// <param name="options">File picker options to use; may be <see langword="null"/> for default options.</param>
		/// <returns>An IEnumerable of file picking result objects, or <see langword="null"/> if picking was cancelled by the user.</returns>
		/// <remarks>
		/// File types can be specified in order to limit files that can be selected, using a
		/// <see cref="PickOptions"/> object. Note that this method may re-throw platform specific exceptions that
		/// occurred during file picking. When calling <see cref="PickMultipleAsync(PickOptions?)"/> again while showing a file
		/// picker, the <see cref="Task"/> object that was returned from the first call is cancelled. Be sure to
		/// also handle the <see cref="TaskCanceledException"/> in this case.
		/// </remarks>
		Task<IEnumerable<FileResult?>> PickMultipleAsync(PickOptions? options = null);
	}

	/// <summary>
	/// Lets the user pick a file from the device's storage.
	/// </summary>
	/// <remarks>To enable iCloud capabilities in the file picker on iOS, you'll need to add the iCloud Documents in your <c>Entitlements.plist</c>.</remarks>
	public static partial class FilePicker
	{
		/// <summary>
		/// Opens the default file picker to allow the user to pick a single file.
		/// </summary>
		/// <param name="options">File picker options to use; may be <see langword="null"/> for default options.</param>
		/// <returns>File picking result object, or <see langword="null"/> if picking was cancelled by the user.</returns>
		/// <remarks>
		/// File types can be specified in order to limit files that can be selected, using a
		/// <see cref="PickOptions"/> object. Note that this method may re-throw platform specific exceptions that
		/// occurred during file picking. When calling <see cref="PickAsync(PickOptions?)"/> again while showing a file
		/// picker, the <see cref="Task"/> object that was returned from the first call is cancelled. Be sure to
		/// also handle the <see cref="TaskCanceledException"/> in this case.
		/// </remarks>
		public static Task<FileResult?> PickAsync(PickOptions? options = null) =>
			Default.PickAsync(options);

		/// <summary>
		/// Opens the default file picker to allow the user to pick one or more files.
		/// </summary>
		/// <param name="options">File picker options to use; may be <see langword="null"/> for default options.</param>
		/// <returns>An IEnumerable of file picking result objects, or <see langword="null"/> if picking was cancelled by the user.</returns>
		/// <remarks>
		/// File types can be specified in order to limit files that can be selected, using a
		/// <see cref="PickOptions"/> object. Note that this method may re-throw platform specific exceptions that
		/// occurred during file picking. When calling <see cref="PickMultipleAsync(PickOptions?)"/> again while showing a file
		/// picker, the <see cref="Task"/> object that was returned from the first call is cancelled. Be sure to
		/// also handle the <see cref="TaskCanceledException"/> in this case.
		/// </remarks>
		public static Task<IEnumerable<FileResult?>> PickMultipleAsync(PickOptions? options = null) =>
			Default.PickMultipleAsync(options);

		static IFilePicker? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IFilePicker Default =>
			defaultImplementation ??= new FilePickerImplementation();

		internal static void SetDefault(IFilePicker? implementation) =>
			defaultImplementation = implementation;
	}

	partial class FilePickerImplementation : IFilePicker
	{
		public async Task<FileResult?> PickAsync(PickOptions? options = null) =>
			(await PlatformPickAsync(options))?.FirstOrDefault();

		public Task<IEnumerable<FileResult?>> PickMultipleAsync(PickOptions? options = null) =>
			PlatformPickAsync(options ?? PickOptions.Default, true);
	}

	/// <summary>
	/// Represents the file types that are allowed to be picked by the user when using <see cref="IFilePicker"/>.
	/// </summary>
	public partial class FilePickerFileType
	{
		/// <summary>
		/// Predefined <see cref="FilePickerFileType"/> object for allowing picking image files only.
		/// </summary>
		public static readonly FilePickerFileType Images = PlatformImageFileType();

		/// <summary>
		/// Predefined <see cref="FilePickerFileType"/> object for allowing picking PNG image files only.
		/// </summary>
		public static readonly FilePickerFileType Png = PlatformPngFileType();

		/// <summary>
		/// Predefined <see cref="FilePickerFileType"/> object for allowing picking JPEG image files only.
		/// </summary>
		public static readonly FilePickerFileType Jpeg = PlatformJpegFileType();

		/// <summary>
		/// Predefined <see cref="FilePickerFileType"/> object for allowing picking video files only.
		/// </summary>
		public static readonly FilePickerFileType Videos = PlatformVideoFileType();

		/// <summary>
		/// Predefined <see cref="FilePickerFileType"/> object for allowing picking PDF files only.
		/// </summary>
		public static readonly FilePickerFileType Pdf = PlatformPdfFileType();

		readonly IDictionary<DevicePlatform, IEnumerable<string>> fileTypes;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilePickerFileType"/> class.
		/// </summary>
		protected FilePickerFileType() =>
			fileTypes = new Dictionary<DevicePlatform, IEnumerable<string>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="FilePickerFileType"/> class.
		/// </summary>
		/// <param name="fileTypes">
		/// A <see cref="IDictionary{TKey, TValue}"/> where the key is the platform
		/// and the value is a collection of file types that are allowed to be picked by the user.
		/// </param>
		/// <remarks>Note that the file types (specified in the <paramref name="fileTypes"/> value)
		/// should be identified differently per platform.</remarks>
		public FilePickerFileType(IDictionary<DevicePlatform, IEnumerable<string>> fileTypes) =>
			this.fileTypes = fileTypes;

		/// <summary>
		/// Gets the configured allowed file types that can be picked by the user for the current platform.
		/// </summary>
		/// <exception cref="PlatformNotSupportedException">Thrown if the current platform does not have any file types configured.</exception>
		public IEnumerable<string> Value => GetPlatformFileType(DeviceInfo.Current.Platform);

		/// <summary>
		/// Gets the configured allowed file types that can be picked by the user for the current platform.
		/// </summary>
		/// <exception cref="PlatformNotSupportedException">Thrown if the current platform does not have any file types configured.</exception>
		protected virtual IEnumerable<string> GetPlatformFileType(DevicePlatform platform)
		{
			if (fileTypes.TryGetValue(platform, out var type))
				return type;

			throw new PlatformNotSupportedException("This platform does not support this file type.");
		}
	}

	/// <summary>
	/// Represents file picking options that can be used to customize the working of <see cref="IFilePicker"/>.
	/// </summary>
	public class PickOptions
	{
		/// <summary>
		/// Default file picking options. This object can be used when no special pick options are necessary.
		/// </summary>
		public static PickOptions Default =>
			new PickOptions
			{
				FileTypes = null,
			};

		/// <summary>
		/// Predefined <see cref="PickOptions"/> object for picking image files only.
		/// </summary>
		public static PickOptions Images =>
			new PickOptions
			{
				FileTypes = FilePickerFileType.Images
			};

		/// <summary>
		/// Title used for the file picker that is shown to the user.
		/// </summary>
		/// <remarks>The file picker title is only used on the Android platform and only for certain file pickers. This title is not guaranteed to be shown.</remarks>
		public string? PickerTitle { get; set; }

		/// <summary>
		/// List of file types that file file picker should return.
		/// </summary>
		/// <remarks>
		/// On Android and iOS the files not matching this list are shown in the file picker,
		/// but will be grayed out and cannot be selected.
		/// When the <see cref="FilePickerFileType.Value"/> array is <see langword="null"/> or empty,
		/// all file types can be selected while picking.
		/// The contents of this array is platform specific; every platform has its own way to
		/// specify the file types.
		/// <para>On Android you can specify one or more MIME types, e.g.
		/// <c>image/png</c>. Additionally, wildcard characters can be used, e.g. <c>image/*</c></para>
		/// <para>On iOS, you can specify <c>UTType</c> constants, e.g. <c>UTType.Image</c>.</para>
		/// <para>On Windows, you can specify a list of extensions, like this: <c>".jpg", ".png"</c>.</para>
		///</remarks>
		public FilePickerFileType? FileTypes { get; set; }
	}
}
