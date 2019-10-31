using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        public static Task<bool> CanOpenAsync(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            return PlatformCanOpenAsync(new Uri(uri));
        }

        public static Task<bool> CanOpenAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return PlatformCanOpenAsync(uri);
        }

        public static Task OpenAsync(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            return PlatformOpenAsync(new Uri(uri));
        }

        public static Task OpenAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return PlatformOpenAsync(uri);
        }

        public static Task OpenAsync(OpenFileRequest request)
        {
            return PlatformOpenAsync(request);
        }

        public static Task<bool> TryOpenAsync(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            return PlatformTryOpenAsync(new Uri(uri));
        }

        public static Task<bool> TryOpenAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return PlatformCanOpenAsync(uri);
        }
    }

    public class OpenFileRequest
    {
        public OpenFileRequest()
        {
        }

        public OpenFileRequest(string title, ReadOnlyFile file)
        {
            Title = title;
            File = file;
        }

        public OpenFileRequest(string title, FileBase file)
        {
            Title = title;
            File = new ReadOnlyFile(file);
        }

        public string Title { get; set; }

        public ReadOnlyFile File { get; set; }
    }
}
