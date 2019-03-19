using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Webkit;

namespace Xamarin.Essentials
{
    public partial class FileSystem
    {
        static string PlatformCacheDirectory
            => Platform.AppContext.CacheDir.AbsolutePath;

        static string PlatformAppDataDirectory
            => Platform.AppContext.FilesDir.AbsolutePath;

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
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

    public partial class FileBase
    {
        internal FileBase(Java.IO.File file)
            : this(file?.Path)
        {
        }

        internal static string PlatformGetContentType(string extension) =>
            MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension.TrimStart('.'));

        internal void PlatformInit(FileBase file)
        {
        }
    }
}
