using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FilePickerImplementation : IFilePicker
	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePicker']/Docs" />
	{
		public Task<IEnumerable<FileResult>> PickAsync(PickOptions options, bool allowMultiple = false)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options)
			=> throw new NotImplementedInReferenceAssemblyException();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/FilePickerFileType.xml" path="Type[@FullName='Microsoft.Maui.Essentials.FilePickerFileType']/Docs" />
	public partial class FilePickerFileTypeImplementation : IFilePickerFileType
	{
		public IFilePickerFileType ImageFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		public IFilePickerFileType PngFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		public IFilePickerFileType JpegFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		public IFilePickerFileType VideoFileType()
			=> throw new NotImplementedInReferenceAssemblyException();

		public IFilePickerFileType PdfFileType()
			=> throw new NotImplementedInReferenceAssemblyException();
	}
}
