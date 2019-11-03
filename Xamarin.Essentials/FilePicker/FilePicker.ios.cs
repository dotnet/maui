using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MobileCoreServices;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static TaskCompletionSource<PickResult> completionSource;

        static Task<PickResult> PlatformPickFileAsync(PickOptions options)
        {
            var previousCompletionSource = Interlocked.Exchange(ref completionSource, null);
            if (previousCompletionSource != null)
            {
                previousCompletionSource.SetCanceled();
            }

            var allowedUtis = options?.FileTypes?.Value?.ToArray() ?? new string[]
            {
                UTType.Content,
                UTType.Item,
                "public.data"
            };

            // Note: Importing (UIDocumentPickerMode.Import) makes a local copy of the document,
            // while opening (UIDocumentPickerMode.Open) opens the document directly. We do the
            // first, so the user has to read the file immediately.
            var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Import);

            documentPicker.DidPickDocumentAtUrls += DocumentPicker_DidPickDocumentAtUrls;
            documentPicker.DidPickDocument += DocumentPicker_DidPickDocument;
            documentPicker.WasCancelled += DocumentPicker_WasCancelled;

            var parentController = Platform.GetCurrentViewController();

            parentController.PresentViewController(documentPicker, true, null);

            var tcs = new TaskCompletionSource<PickResult>();

            Interlocked.Exchange(ref completionSource, tcs);

            return tcs.Task;
        }

        static void DocumentPicker_DidPickDocumentAtUrls(object sender, UIDocumentPickedAtUrlsEventArgs args)
        {
            // this is called starting from iOS 11.
            var control = (UIDocumentPickerViewController)sender;
            foreach (var url in args.Urls)
            {
                DocumentPicker_DidPickDocument(control, new UIDocumentPickedEventArgs(url));
            }

            control.Dispose();
        }

        static void DocumentPicker_DidPickDocument(object sender, UIDocumentPickedEventArgs args)
        {
            try
            {
                var securityEnabled = args.Url.StartAccessingSecurityScopedResource();
                var doc = new UIDocument(args.Url);

                var filename = doc.LocalizedName;
                var pathname = doc.FileUrl?.Path;

                args.Url.StopAccessingSecurityScopedResource();

                // iCloud drive can return null for LocalizedName.
                if (filename == null && pathname != null)
                {
                    filename = Path.GetFileName(pathname);
                }

                // immediately open a file stream, in case iOS cleans up the picked file
                var stream = new FileStream(pathname, FileMode.Open, FileAccess.Read);

                var result = new PickResult(pathname, filename, stream);
                var tcs = Interlocked.Exchange(ref completionSource, null);
                tcs?.SetResult(result);
            }
            catch (Exception ex)
            {
                // pass exception to task so that it doesn't get lost in the UI main loop
                var tcs = Interlocked.Exchange(ref completionSource, null);
                tcs.SetException(ex);
            }
        }

        static void DocumentPicker_WasCancelled(object sender, EventArgs args)
        {
            var tcs = Interlocked.Exchange(ref completionSource, null);
            tcs.SetResult(null);
        }
    }

    public partial class FilePickerFileType
    {
        public static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { (string)UTType.Image } }
            });

        public static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { (string)UTType.PNG } }
            });
    }

    public partial class PickResult
    {
        Stream stream;

        Stream PlatformGetStream()
        {
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        internal PickResult(string pathname, string filename, Stream stream)
            : this(pathname, filename)
        {
            this.stream = stream;
        }
    }
}
