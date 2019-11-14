using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<FilePickerResult> PlatformPickFileAsync(PickOptions options)
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class FilePickerFileType
    {
        public static FilePickerFileType PlatformImageFileType()
            => throw new NotImplementedInReferenceAssemblyException();

        public static FilePickerFileType PlatformPngFileType()
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class PickerResultBase
    {
        internal PickerResultBase()
        {
        }

        Task<Stream> PlatformOpenReadStreamAsync()
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class FilePickerResult
    {
        internal FilePickerResult()
        {
        }
    }
}
