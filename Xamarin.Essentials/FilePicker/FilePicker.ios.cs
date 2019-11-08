using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MobileCoreServices;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<PickResult> PlatformPickFileAsync(PickOptions options)
        {
            var allowedUtis = options?.FileTypes?.Value?.ToArray() ?? new string[]
            {
                UTType.Content,
                UTType.Item,
                "public.data"
            };

            // Note: Importing (UIDocumentPickerMode.Import) makes a local copy of the document,
            // while opening (UIDocumentPickerMode.Open) opens the document directly. We do the
            // first, so the user has to read the file immediately.
            var documentPicker = new DocumentPicker(allowedUtis, UIDocumentPickerMode.Import);

            var tcs = new TaskCompletionSource<PickResult>();

            documentPicker.Picked += (sender, e) =>
            {
                if (e == null)
                {
                    tcs.TrySetResult(null);
                    return;
                }

                try
                {
                    var url = e.Url;

                    url.StartAccessingSecurityScopedResource();

                    var doc = new UIDocument(url);
                    var filename = doc.LocalizedName;
                    var pathname = doc.FileUrl?.Path;

                    url.StopAccessingSecurityScopedResource();

                    // immediately open a file stream, in case iOS cleans up the picked file
                    var stream = new FileStream(pathname, FileMode.Open, FileAccess.Read);

                    var result = new PickResult(pathname, filename, stream);

                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    // pass exception to task so that it doesn't get lost in the UI main loop
                    tcs.SetException(ex);
                }
            };

            var parentController = Platform.GetCurrentViewController();

            parentController.PresentViewController(documentPicker, true, null);

            return tcs.Task;
        }

        class DocumentPicker : UIDocumentPickerViewController
        {
            public DocumentPicker(string[] allowedUTIs, UIDocumentPickerMode mode)
                : base(allowedUTIs, mode)
            {
                // this is called starting from iOS 11.
                DidPickDocumentAtUrls += OnUrlsPicked;
                DidPickDocument += OnDocumentPicked;
                WasCancelled += OnCancelled;
            }

            public event EventHandler<UIDocumentPickedEventArgs> Picked;

            void OnUrlsPicked(object sender, UIDocumentPickedAtUrlsEventArgs e) =>
                Picked?.Invoke(this, new UIDocumentPickedEventArgs(e.Urls[0]));

            void OnDocumentPicked(object sender, UIDocumentPickedEventArgs e) =>
                Picked?.Invoke(this, e);

            void OnCancelled(object sender, EventArgs args) =>
                Picked?.Invoke(this, null);
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
        readonly Stream stream;

        internal PickResult(string pathname, string filename, Stream stream)
            : base(pathname)
        {
            FileName = filename;
            this.stream = stream;
        }

        Task<Stream> PlatformOpenReadStreamAsync()
        {
            // make sure we are at he beginning
            stream.Seek(0, SeekOrigin.Begin);

            return Task.FromResult(stream);
        }
    }
}
