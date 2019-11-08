using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        static Task<FilePickerResult> PlatformPickFileAsync(PickOptions options)
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

            var tcs = new TaskCompletionSource<FilePickerResult>();

            documentPicker.Picked += (sender, e) =>
            {
                // there was a cancellation
                if (e == null)
                {
                    tcs.TrySetResult(null);
                    return;
                }

                try
                {
                    var result = new FilePickerResult(e.Url);
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

    public partial class PickerResultBase
    {
        readonly Stream fileStream;
        readonly string fullPath;

        internal PickerResultBase(NSUrl url)
        {
            url.StartAccessingSecurityScopedResource();

            var doc = new UIDocument(url);
            fullPath = doc.FileUrl?.Path;
            FileName = doc.LocalizedName ?? Path.GetFileName(fullPath);

            url.StopAccessingSecurityScopedResource();

            // immediately open a file stream, in case iOS cleans up the picked file
            fileStream = File.OpenRead(fullPath);
        }

        Task<Stream> PlatformOpenReadStreamAsync()
        {
            // make sure we are at he beginning
            fileStream.Seek(0, SeekOrigin.Begin);

            return Task.FromResult(fileStream);
        }
    }

    public partial class FilePickerResult
    {
        internal FilePickerResult(NSUrl url)
            : base(url)
        {
        }
    }
}
