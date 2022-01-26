using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePicker']/Docs" />
	public static partial class FilePicker
	{
		static Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
			=> throw new NotImplementedInReferenceAssemblyException();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePickerFileType']/Docs" />
	public partial class FilePickerFileType
	{
		static FilePickerFileType PlatformImageFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		static FilePickerFileType PlatformPngFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		static FilePickerFileType PlatformJpegFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		static FilePickerFileType PlatformVideoFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		static FilePickerFileType PlatformPdfFileType()
			=> throw new NotImplementedInReferenceAssemblyException();
	}
}
