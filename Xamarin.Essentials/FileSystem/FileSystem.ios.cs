using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using Photos;
using UIKit;

namespace Xamarin.Essentials
{
    class UIDocumentFileResult : FileResult
    {
        readonly Stream fileStream;

        internal UIDocumentFileResult(NSUrl url)
            : base()
        {
            url.StartAccessingSecurityScopedResource();

            var doc = new UIDocument(url);
            FullPath = doc.FileUrl?.Path;
            FileName = doc.LocalizedName ?? Path.GetFileName(FullPath);

            // immediately open a file stream, in case iOS cleans up the picked file
            fileStream = File.OpenRead(FullPath);

            url.StopAccessingSecurityScopedResource();
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            // make sure we are at he beginning
            fileStream.Seek(0, SeekOrigin.Begin);

            return Task.FromResult(fileStream);
        }
    }

    class PHAssetFileResult : FileResult
    {
        readonly PHAsset phAsset;

        internal PHAssetFileResult(NSUrl url, PHAsset asset, string originalFilename)
            : base()
        {
            phAsset = asset;

            FullPath = url?.AbsoluteString;
            FileName = originalFilename;
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            var tcsStream = new TaskCompletionSource<Stream>();

            PHImageManager.DefaultManager.RequestImageData(phAsset, null, new PHImageDataHandler((data, str, orientation, dict) =>
                tcsStream.TrySetResult(data.AsStream())));

            return tcsStream.Task;
        }
    }

    class UIImageFileResult : FileResult
    {
        readonly UIImage uiImage;
        NSData data;

        internal UIImageFileResult(UIImage image)
            : base()
        {
            uiImage = image;

            FullPath = Guid.NewGuid().ToString() + ".png";
            FileName = FullPath;
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            data ??= uiImage.AsPNG();

            return Task.FromResult(data.AsStream());
        }
    }
}
