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
        static Task<IEnumerable<FilePickerResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            if (allowMultiple && !Platform.HasOSVersion(11, 0))
                throw new FeatureNotSupportedException("Multiple file picking is only available on iOS 11 or later.");

            var allowedUtis = options?.FileTypes?.Value?.ToArray() ?? new string[]
            {
                UTType.Content,
                UTType.Item,
                "public.data"
            };

            var tcs = new TaskCompletionSource<IEnumerable<FilePickerResult>>();

            // Note: Importing (UIDocumentPickerMode.Import) makes a local copy of the document,
            // while opening (UIDocumentPickerMode.Open) opens the document directly. We do the
            // latter, so the user accesses the original file.
            var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Open);
            documentPicker.AllowsMultipleSelection = allowMultiple;
            documentPicker.Delegate = new PickerDelegate
            {
                PickHandler = urls =>
                {
                    try
                    {
                        // there was a cancellation
                        if (urls?.Any() ?? false)
                            tcs.TrySetResult(urls.Select(url => new FilePickerResult(url)));
                        else
                            tcs.TrySetResult(Enumerable.Empty<FilePickerResult>());
                    }
                    catch (Exception ex)
                    {
                        // pass exception to task so that it doesn't get lost in the UI main loop
                        tcs.SetException(ex);
                    }
                }
            };

            var parentController = Platform.GetCurrentViewController();

            parentController.PresentViewController(documentPicker, true, null);

            return tcs.Task;
        }

        class PickerDelegate : UIDocumentPickerDelegate
        {
            public Action<IEnumerable<NSUrl>> PickHandler { get; set; }

            public override void WasCancelled(UIDocumentPickerViewController controller)
                => PickHandler?.Invoke(null);

            public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl[] urls)
                => PickHandler?.Invoke(urls);

            public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl url)
                => PickHandler?.Invoke(new List<NSUrl> { url });
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
