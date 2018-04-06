using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        public static string CacheDirectory
            => throw new NotImplementedInReferenceAssemblyException();

        public static string AppDataDirectory
            => throw new NotImplementedInReferenceAssemblyException();

        public static Task<Stream> OpenAppPackageFileAsync(string filename)
             => throw new NotImplementedInReferenceAssemblyException();
    }
}
