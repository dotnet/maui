using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<IEnumerable<FilePickerResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class FilePickerFileType
    {
        static FilePickerFileType PlatformImageFileType()
            => throw new NotImplementedInReferenceAssemblyException();

        static FilePickerFileType PlatformPngFileType()
            => throw new NotImplementedInReferenceAssemblyException();

        static FilePickerFileType PlatformVideoFileType()
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class FilePickerResult
    {
        Task<Stream> PlatformOpenReadStreamAsync()
            => throw new NotImplementedInReferenceAssemblyException();
    }
}
