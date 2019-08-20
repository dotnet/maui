using System;
using System.IO;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class FileSystem
    {
        static string PlatformCacheDirectory
            => Application.Current.DirectoryInfo.Cache;

        static string PlatformAppDataDirectory
            => Application.Current.DirectoryInfo.Data;

        static Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            filename = filename.Replace('\\', Path.DirectorySeparatorChar);
            Stream fs = File.OpenRead(Path.Combine(Application.Current.DirectoryInfo.Resource, filename));
            return Task.FromResult(fs);
        }
    }

    public partial class FileBase
    {
        static string PlatformGetContentType(string extension)
        {
            return Tizen.Content.MimeType.MimeUtil.GetMimeType(extension);
        }

        internal void PlatformInit(FileBase file)
        {
        }
    }
}
