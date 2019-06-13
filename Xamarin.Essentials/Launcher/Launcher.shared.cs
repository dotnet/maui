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
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.ShareFileRequest);

            return PlatformOpenAsync(request);
        }
    }

    public class OpenFileRequest
    {
        public OpenFileRequest()
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.OpenFileRequest);
        }

        public OpenFileRequest(string title, ReadOnlyFile file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.OpenFileRequest);
            Title = title;
            File = file;
        }

        public OpenFileRequest(string title, FileBase file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.OpenFileRequest);
            Title = title;
            File = new ReadOnlyFile(file);
        }

        public string Title { get; set; }

        public ReadOnlyFile File { get; set; }
    }
}
