using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;

namespace Microsoft.Caboodle
{
    public partial class FileSystem
    {
        public static string CacheDirectory
            => Application.Context.CacheDir.AbsolutePath;

        public static string AppDataDirectory
            => Application.Context.FilesDir.AbsolutePath;

        public static Task<Stream> OpenAppPackageFileAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            filename = filename.Replace('\\', Path.DirectorySeparatorChar);
            try
            {
                return Task.FromResult(Application.Context.Assets.Open(filename));
            }
            catch (Java.IO.FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message, filename, ex);
            }
        }
    }
}
