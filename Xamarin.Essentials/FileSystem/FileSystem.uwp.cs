using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        public static string CacheDirectory
            => ApplicationData.Current.LocalCacheFolder.Path;

        public static string AppDataDirectory
            => ApplicationData.Current.LocalFolder.Path;

        public static Task<Stream> OpenAppPackageFileAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            filename = filename.Replace('/', Path.DirectorySeparatorChar);
            return Package.Current.InstalledLocation.OpenStreamForReadAsync(filename);
        }
    }
}
