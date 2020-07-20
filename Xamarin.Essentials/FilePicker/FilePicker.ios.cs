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
        static Task<IEnumerable<FilePickerResult>> PlatformPickAsync(PickOptions options)
        {
            if (!Platform.HasOSVersion(11, 0))
                throw new FeatureNotSupportedException("Multiple file picking is only available on iOS 11 or later.");

            var allowedUtis = options?.FileTypes?.Value?.ToArray() ?? new string[]
            {
                UTType.Content,
                UTType.Item,
                "public.data"
            };

            // Note: Importing (UIDocumentPickerMode.Import) makes a local copy of the document,
            // while opening (UIDocumentPickerMode.Open) opens the document directly. We do the
            // latter, so the user accesses the original file.
            var documentPicker = new DocumentPicker(allowedUtis, UIDocumentPickerMode.Open);
            documentPicker.AllowsMultipleSelection = options.AllowMultiple;

            var tcs = new TaskCompletionSource<IEnumerable<FilePickerResult>>();

            documentPicker.Picked += (sender, e) =>
            {
                // there was a cancellation
                if (e == null || e.Urls.Length == 0)
                {
                    tcs.TrySetResult(Enumerable.Empty<FilePickerResult>());
                    return;
                }

                try
                {
                    var resultList = e.Urls.Select(url => new FilePickerResult(url));
                    tcs.TrySetResult(resultList);
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

            public event EventHandler<UIDocumentPickedAtUrlsEventArgs> Picked;

            void OnUrlsPicked(object sender, UIDocumentPickedAtUrlsEventArgs e) =>
                Picked?.Invoke(this, e);

            void OnDocumentPicked(object sender, UIDocumentPickedEventArgs e) =>
                Picked?.Invoke(this, new UIDocumentPickedAtUrlsEventArgs(new NSUrl[] { e.Url }));

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

    public partial class FilePickerResult
    {
        Stream fileStream;

        internal FilePickerResult(NSUrl url)
            : base()
        {
            url.StartAccessingSecurityScopedResource();

            var doc = new UIDocument(url);
            FullPath = doc.FileUrl?.Path;
            FileName = doc.LocalizedName ?? Path.GetFileName(FullPath);

            url.StopAccessingSecurityScopedResource();

            // immediately open a file stream, in case iOS cleans up the picked file
            fileStream = File.OpenRead(FullPath);
        }

        Task<Stream> PlatformOpenReadStreamAsync()
        {
            // make sure we are at he beginning
            fileStream.Seek(0, SeekOrigin.Begin);

            return Task.FromResult(fileStream);
        }
    }
}
