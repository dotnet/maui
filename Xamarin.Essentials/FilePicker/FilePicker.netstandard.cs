using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<PickResult> PlatformPickFileAsync(PickOptions options)
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class PickOptions
    {
        static PickOptions PlatformGetImagesPickOptions()
            => throw new NotImplementedInReferenceAssemblyException();
    }

    public partial class PickResult
    {
        Stream PlatformGetStream() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
