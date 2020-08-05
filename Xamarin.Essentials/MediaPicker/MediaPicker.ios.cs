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

        static bool PlatformIsPhotoCaptureAvailable
            => UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

        static Task<MediaPickerResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => PhotoAsync(options, true, true);

        static Task<MediaPickerResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => PhotoAsync(options, true, false);

        static Task<MediaPickerResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => PhotoAsync(options, false, true);

        static Task<MediaPickerResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => PhotoAsync(options, false, false);

        static async Task<MediaPickerResult> PhotoAsync(MediaPickerOptions options, bool photo, bool pickExisting)
        {
            var sourceType = pickExisting ? UIImagePickerControllerSourceType.PhotoLibrary : UIImagePickerControllerSourceType.Camera;

            if (!UIImagePickerController.IsSourceTypeAvailable(sourceType))
                throw new FeatureNotSupportedException();
            if (!UIImagePickerController.AvailableMediaTypes(sourceType).Contains(photo ? UTType.Image : UTType.Movie))
                throw new FeatureNotSupportedException();

            // permission is not required on iOS 11 for the picker
            if (!Platform.HasOSVersion(11, 0))
            {
                await Permissions.EnsureGrantedAsync<Permissions.Photos>();
            }

            var vc = Platform.GetCurrentViewController(true);

            picker = new UIImagePickerController();
            picker.SourceType = sourceType;
            picker.MediaTypes = new string[] { photo ? UTType.Image : UTType.Movie };
            picker.AllowsEditing = false;

            if (!string.IsNullOrWhiteSpace(options?.Title))
                picker.Title = options.Title;

            if (DeviceInfo.Idiom == DeviceIdiom.Tablet && picker.PopoverPresentationController != null && vc.View != null)
                picker.PopoverPresentationController.SourceRect = vc.View.Bounds;

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
            NSUrl assetUrl = null;

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

            if (phAsset == null)
            {
                assetUrl = info[UIImagePickerController.ReferenceUrl] as NSUrl;

                if (assetUrl != null)
                    phAsset = PHAsset.FetchAssets(new NSUrl[] { assetUrl }, null)?.LastObject as PHAsset;
            }

            if (phAsset == null || assetUrl == null)
            {
                var img = info.ValueForKey(UIImagePickerController.OriginalImage) as UIImage;

                if (img != null)
                    return new MediaPickerResult(img);
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
        UIImage uiImage;

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

        internal MediaPickerResult(UIImage image)
        {
            FullPath = Guid.NewGuid().ToString() + ".png";
            FileName = FullPath;
            uiImage = image;
        }

        internal override Task<Stream> PlatformOpenReadAsync()
        {
            if (uiImage != null)
                return Task.FromResult<Stream>(uiImage.AsPNG().AsStream());

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
