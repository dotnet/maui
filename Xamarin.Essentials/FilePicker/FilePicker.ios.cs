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
        static Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
        {
            var allowedUtis = options?.FileTypes?.Value?.ToArray() ?? new string[]
            {
                UTType.Content,
                UTType.Item,
                "public.data"
            };

            var tcs = new TaskCompletionSource<IEnumerable<FileResult>>();

            // Note: Importing (UIDocumentPickerMode.Import) makes a local copy of the document,
            // while opening (UIDocumentPickerMode.Open) opens the document directly. We do the
            // latter, so the user accesses the original file.
            var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Open);
            if (Platform.HasOSVersion(11, 0))
                documentPicker.AllowsMultipleSelection = allowMultiple;
            documentPicker.Delegate = new PickerDelegate
            {
                PickHandler = urls =>
                {
                    try
                    {
                        // there was a cancellation
                        if (urls?.Any() ?? false)
                            tcs.TrySetResult(urls.Select(url => new UIDocumentFileResult(url)));
                        else
                            tcs.TrySetResult(Enumerable.Empty<FileResult>());
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
        static FilePickerFileType PlatformImageFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { (string)UTType.Image } }
            });

        static FilePickerFileType PlatformPngFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { (string)UTType.PNG } }
            });

        static FilePickerFileType PlatformJpegFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { (string)UTType.JPEG } }
            });

        static FilePickerFileType PlatformVideoFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new string[] { UTType.MPEG4, UTType.Video, UTType.AVIMovie, UTType.AppleProtectedMPEG4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } }
            });

        static FilePickerFileType PlatformPdfFileType() =>
            new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { (string)UTType.PDF } }
            });
    }
}
