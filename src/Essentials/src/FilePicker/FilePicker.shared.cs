using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class FilePicker
	{
		public static async Task<FileResult> PickAsync(PickOptions options = null) =>
			(await PlatformPickAsync(options))?.FirstOrDefault();

		public static Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options = null) =>
			PlatformPickAsync(options ?? PickOptions.Default, true);
	}

	public partial class FilePickerFileType
	{
		public static readonly FilePickerFileType Images = PlatformImageFileType();
		public static readonly FilePickerFileType Png = PlatformPngFileType();
		public static readonly FilePickerFileType Jpeg = PlatformJpegFileType();
		public static readonly FilePickerFileType Videos = PlatformVideoFileType();
		public static readonly FilePickerFileType Pdf = PlatformPdfFileType();

		readonly IDictionary<DevicePlatform, IEnumerable<string>> fileTypes;

		protected FilePickerFileType()
		{
		}

		public FilePickerFileType(IDictionary<DevicePlatform, IEnumerable<string>> fileTypes) =>
			this.fileTypes = fileTypes;

		public IEnumerable<string> Value => GetPlatformFileType(DeviceInfo.Platform);

		protected virtual IEnumerable<string> GetPlatformFileType(DevicePlatform platform)
		{
			if (fileTypes.TryGetValue(platform, out var type))
				return type;

			throw new PlatformNotSupportedException("This platform does not support this file type.");
		}
	}

	public class PickOptions
	{
		public static PickOptions Default =>
			new PickOptions
			{
				FileTypes = null,
			};

		public static PickOptions Images =>
			new PickOptions
			{
				FileTypes = FilePickerFileType.Images
			};

		public string PickerTitle { get; set; }

		public FilePickerFileType FileTypes { get; set; }
	}
}
