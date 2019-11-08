using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<PickResult> PlatformPickFileAsync(PickOptions options)
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class FilePickerFileType
    {
        public static FilePickerFileType PlatformImageFileType()
            => throw new NotImplementedInReferenceAssemblyException();

        public static FilePickerFileType PlatformPngFileType()
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class PickResult
    {
        PickResult()
            : base((string)null)
        {
        }

        Task<Stream> PlatformOpenReadStreamAsync() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
