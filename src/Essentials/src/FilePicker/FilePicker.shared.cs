using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePicker']/Docs" />
	public static partial class FilePicker
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="//Member[@MemberName='PickAsync']/Docs" />
		public static async Task<FileResult> PickAsync(PickOptions options = null) =>
			(await PlatformPickAsync(options))?.FirstOrDefault();

		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="//Member[@MemberName='PickMultipleAsync']/Docs" />
		public static Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options = null) =>
			PlatformPickAsync(options ?? PickOptions.Default, true);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePickerFileType']/Docs" />
	public partial class FilePickerFileType
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Images']/Docs" />
		public static readonly FilePickerFileType Images = PlatformImageFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Png']/Docs" />
		public static readonly FilePickerFileType Png = PlatformPngFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Jpeg']/Docs" />
		public static readonly FilePickerFileType Jpeg = PlatformJpegFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Videos']/Docs" />
		public static readonly FilePickerFileType Videos = PlatformVideoFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Pdf']/Docs" />
		public static readonly FilePickerFileType Pdf = PlatformPdfFileType();

		readonly IDictionary<DevicePlatform, IEnumerable<string>> fileTypes;

		protected FilePickerFileType()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public FilePickerFileType(IDictionary<DevicePlatform, IEnumerable<string>> fileTypes) =>
			this.fileTypes = fileTypes;

		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Value']/Docs" />
		public IEnumerable<string> Value => GetPlatformFileType(DeviceInfo.Platform);

		protected virtual IEnumerable<string> GetPlatformFileType(DevicePlatform platform)
		{
			if (fileTypes.TryGetValue(platform, out var type))
				return type;

			throw new PlatformNotSupportedException("This platform does not support this file type.");
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/PickOptions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PickOptions']/Docs" />
	public class PickOptions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/PickOptions.xml" path="//Member[@MemberName='Default']/Docs" />
		public static PickOptions Default =>
			new PickOptions
			{
				FileTypes = null,
			};

		/// <include file="../../docs/Microsoft.Maui.Essentials/PickOptions.xml" path="//Member[@MemberName='Images']/Docs" />
		public static PickOptions Images =>
			new PickOptions
			{
				FileTypes = FilePickerFileType.Images
			};

		/// <include file="../../docs/Microsoft.Maui.Essentials/PickOptions.xml" path="//Member[@MemberName='PickerTitle']/Docs" />
		public string PickerTitle { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/PickOptions.xml" path="//Member[@MemberName='FileTypes']/Docs" />
		public FilePickerFileType FileTypes { get; set; }
	}
}
