using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IFilePicker
	{
		Task<IEnumerable<FileResult>> PickAsync(PickOptions options, bool allowMultiple);

		Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePicker']/Docs" />
	public static partial class FilePicker
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="//Member[@MemberName='PickAsync']/Docs" />
		public static async Task<FileResult> PickAsync(PickOptions options = null) =>
			(await Current.PickAsync(options, false))?.FirstOrDefault();

		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="//Member[@MemberName='PickMultipleAsync']/Docs" />
		public static Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options = null) =>
			Current.PickAsync(options ?? PickOptions.Default, true);

#nullable enable
		static IFilePicker? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IFilePicker Current =>
			currentImplementation ??= new FilePickerImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IFilePicker? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}

	public interface IFilePickerFileType
	{
		IFilePickerFileType ImageFileType();

		IFilePickerFileType PngFileType();

		IFilePickerFileType JpegFileType();

		IFilePickerFileType VideoFileType();

		IFilePickerFileType PdfFileType();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePickerFileType']/Docs" />
	public partial class FilePickerFileType
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Images']/Docs" />
		public static readonly IFilePickerFileType Images = Current.ImageFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Png']/Docs" />
		public static readonly IFilePickerFileType Png = Current.PngFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Jpeg']/Docs" />
		public static readonly IFilePickerFileType Jpeg = Current.JpegFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Videos']/Docs" />
		public static readonly IFilePickerFileType Videos = Current.VideoFileType();
		/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="//Member[@MemberName='Pdf']/Docs" />
		public static readonly IFilePickerFileType Pdf = Current.PdfFileType();

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

#nullable enable
		static IFilePickerFileType? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IFilePickerFileType Current =>
			currentImplementation ??= new FilePickerFileTypeImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IFilePickerFileType? implementation) =>
			currentImplementation = implementation;
#nullable disable
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
		public IFilePickerFileType FileTypes { get; set; }
	}
}
