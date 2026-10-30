using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Storage
{
	partial class FilePickerImplementation : IFilePicker
	{
		Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
			=> throw new NotImplementedInReferenceAssemblyException();
	}

	public partial class FilePickerFileType
	{
		// The built-in file types are plain data (MIME types), so they are defined here instead of throwing:
		// a type initializer that throws would make FilePickerFileType.Images/Png/... unusable for every
		// custom platform backend (for example Linux/GTK) that only supplies its own IFilePicker.
		// Backends that report DevicePlatform "Linux" get these values; the picker implementation interprets them.

		static FilePickerFileType PlatformImageFileType()
			=> ForLinux("image/png", "image/jpeg", "image/gif", "image/bmp", "image/webp");

		static FilePickerFileType PlatformPngFileType()
			=> ForLinux("image/png");

		static FilePickerFileType PlatformJpegFileType()
			=> ForLinux("image/jpeg");

		static FilePickerFileType PlatformVideoFileType()
			=> ForLinux("video/mp4", "video/x-matroska", "video/quicktime", "video/webm", "video/x-msvideo");

		static FilePickerFileType PlatformPdfFileType()
			=> ForLinux("application/pdf");

		static FilePickerFileType ForLinux(params string[] mimeTypes)
			=> new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Create("Linux"), mimeTypes }
			});
	}
}
