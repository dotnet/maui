using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;
using Photos;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static async Task<MediaFile> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
        {
            if (!UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.PhotoLibrary))
                throw new FeatureNotSupportedException();
            if (!UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary).Contains(UTType.Image))
                throw new FeatureNotSupportedException();

            // permission is not required on iOS 11 for the picker
            if (!Platform.HasOSVersion(11, 0))
                await Permissions.EnsureGrantedAsync<Permissions.Photos>();

            var vc = Platform.GetCurrentViewController(true);

            var picker = new UIImagePickerController();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            picker.MediaTypes = new string[] { UTType.Image };
            picker.AllowsEditing = false;

            var tcs = new TaskCompletionSource<MediaFile>(picker);
            picker.Delegate = new PhotoPickerDelegate(tcs);

            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                if (picker.PopoverPresentationController != null && vc.View != null)
                    picker.PopoverPresentationController.SourceView = vc.View;
            }

            await vc.PresentViewControllerAsync(picker, true);

            return await tcs.Task;
        }

        class PhotoPickerDelegate : UIImagePickerControllerDelegate
        {
            readonly TaskCompletionSource<MediaFile> tcs;

            public PhotoPickerDelegate(TaskCompletionSource<MediaFile> tcs) =>
                this.tcs = tcs;

            public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
                var url = (NSUrl)info[UIImagePickerController.ReferenceUrl];
                tcs.TrySetResult(new MediaFile(url?.AbsoluteString));
            }

            public override void Canceled(UIImagePickerController picker)
                => tcs.TrySetResult(null);
        }
    }
}
