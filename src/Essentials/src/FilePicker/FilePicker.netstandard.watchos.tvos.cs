using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{
	partial class FilePickerImplementation : IFilePicker
	{
		Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
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
