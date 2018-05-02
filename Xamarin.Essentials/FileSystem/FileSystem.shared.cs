using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        public static string CacheDirectory
            => PlatformCacheDirectory;

        public static string AppDataDirectory
            => PlatformAppDataDirectory;

        public static Task<Stream> OpenAppPackageFileAsync(string filename)
            => PlatformOpenAppPackageFileAsync(filename);
    }
}
