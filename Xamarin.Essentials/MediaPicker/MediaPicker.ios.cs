using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssetsLibrary;
using Foundation;
using MobileCoreServices;
using Photos;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static UIImagePickerController picker;

        static async Task<MediaPickerResult> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
        {
            if (!UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.PhotoLibrary))
                throw new FeatureNotSupportedException();
            if (!UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary).Contains(UTType.Image))
                throw new FeatureNotSupportedException();

            // permission is not required on iOS 11 for the picker
            if (!Platform.HasOSVersion(11, 0))
                await Permissions.EnsureGrantedAsync<Permissions.Photos>();

            var vc = Platform.GetCurrentViewController(true);

            picker = new UIImagePickerController();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            picker.MediaTypes = new string[] { UTType.Image };
            picker.AllowsEditing = false;

            if (DeviceInfo.Idiom == DeviceIdiom.Tablet && picker.PopoverPresentationController != null && vc.View != null)
                picker.PopoverPresentationController.SourceView = vc.View;

            var tcs = new TaskCompletionSource<MediaPickerResult>(picker);
            picker.Delegate = new PhotoPickerDelegate
            {
                CompletedHandler = info =>
                    tcs.TrySetResult(DictionaryToMediaFile(info))
            };

            await vc.PresentViewControllerAsync(picker, true);

            var result = await tcs.Task;

            await vc.DismissViewControllerAsync(true);

            picker?.Dispose();
            picker = null;

            return result;
        }

        static MediaPickerResult DictionaryToMediaFile(NSDictionary info)
        {
            PHAsset phAsset = null;
            NSUrl assetUrl;

            if (Platform.HasOSVersion(11, 0))
            {
                assetUrl = info[UIImagePickerController.ImageUrl] as NSUrl;

                if (assetUrl != null)
                {
                    if (!assetUrl.Scheme.Equals("assets-library", StringComparison.InvariantCultureIgnoreCase))
                        return new MediaPickerResult(assetUrl);

                    phAsset = info.ValueForKey(UIImagePickerController.PHAsset) as PHAsset;
                }
            }
            else
            {
                assetUrl = info[UIImagePickerController.ReferenceUrl] as NSUrl;

                if (assetUrl != null)
                    phAsset = PHAsset.FetchAssets(new NSUrl[] { assetUrl }, null)?.LastObject as PHAsset;
            }

            if (phAsset == null || assetUrl == null)
                return null;

            string originalFilename;

            if (Platform.HasOSVersion(9, 0))
                originalFilename = PHAssetResource.GetAssetResources(phAsset).FirstOrDefault()?.OriginalFilename;
            else
                originalFilename = phAsset.ValueForKey(new NSString("filename")) as NSString;

            return new MediaPickerResult(assetUrl, phAsset, originalFilename);
        }

        class PhotoPickerDelegate : UIImagePickerControllerDelegate
        {
            public Action<NSDictionary> CompletedHandler { get; set; }

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info) =>
                CompletedHandler?.Invoke(info);

            public override void Canceled(UIImagePickerController picker) =>
                CompletedHandler?.Invoke(null);
        }
    }

    public partial class MediaPickerResult
    {
        PHAsset phAsset;

        internal MediaPickerResult(NSUrl url)
            : base()
        {
            var doc = new UIDocument(url);
            FullPath = doc.FileUrl?.Path;
            FileName = doc.LocalizedName ?? Path.GetFileName(FullPath);
        }

        internal MediaPickerResult(NSUrl url, PHAsset asset, string originalFilename)
            : base()
        {
            FullPath = url?.AbsoluteString;
            FileName = originalFilename;
            phAsset = asset;
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            if (phAsset != null)
            {
                var tcsStream = new TaskCompletionSource<Stream>();

                PHImageManager.DefaultManager.RequestImageData(phAsset, null, new PHImageDataHandler((data, str, orientation, dict) =>
                    tcsStream.TrySetResult(data.AsStream())));

                return tcsStream.Task;
            }

            return Task.FromResult<Stream>(File.OpenRead(FullPath));
        }
    }
}
