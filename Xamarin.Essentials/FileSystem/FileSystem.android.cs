using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;

namespace Xamarin.Essentials
{
    public partial class FileSystem
    {
        public static string CacheDirectory
            => Platform.AppContext.CacheDir.AbsolutePath;

        public static string AppDataDirectory
            => Platform.AppContext.FilesDir.AbsolutePath;

        public static Task<Stream> OpenAppPackageFileAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            filename = filename.Replace('\\', Path.DirectorySeparatorChar);
            try
            {
                return Task.FromResult(Platform.AppContext.Assets.Open(filename));
            }
            catch (Java.IO.FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message, filename, ex);
            }
        }
    }
}
