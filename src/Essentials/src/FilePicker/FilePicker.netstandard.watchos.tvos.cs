using System;
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

	/// <summary>
	/// Lets a backend built over the platform-less assemblies (for example the Linux/GTK 4 backend, which installs it from its
	/// Essentials registration) define the built-in <see cref="FilePickerFileType"/> values. The argument is the member name
	/// (<c>Images</c>, <c>Png</c>, <c>Jpeg</c>, <c>Video</c> or <c>Pdf</c>). With the default <see langword="null"/> factory those
	/// members throw <see cref="NotImplementedInReferenceAssemblyException"/>, as in the stock Standard behaviour.
	/// </summary>
	/// <remarks>The members are <see langword="static readonly"/>, so the factory must be installed before <see cref="FilePickerFileType"/> is first used.</remarks>
	internal static class FilePickerFileTypeBackend
	{
		public static Func<string, FilePickerFileType> Factory { get; set; }

		public static FilePickerFileType Create(string name)
			=> Factory is null ? throw new NotImplementedInReferenceAssemblyException() : Factory(name);
	}

	public partial class FilePickerFileType
	{
		static FilePickerFileType PlatformImageFileType()
			=> FilePickerFileTypeBackend.Create("Images");

		static FilePickerFileType PlatformPngFileType()
			=> FilePickerFileTypeBackend.Create("Png");

		static FilePickerFileType PlatformJpegFileType()
			=> FilePickerFileTypeBackend.Create("Jpeg");

		static FilePickerFileType PlatformVideoFileType()
			=> FilePickerFileTypeBackend.Create("Video");

		static FilePickerFileType PlatformPdfFileType()
			=> FilePickerFileTypeBackend.Create("Pdf");
	}
}
