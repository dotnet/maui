using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static bool PlatformIsPhotoCaptureAvailable
               => false;

        static async Task<MediaPickerResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => new MediaPickerResult(await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            }));

        static Task<MediaPickerResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => PlatformMediaAsync(options, true);

        static async Task<MediaPickerResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => new MediaPickerResult(await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Videos
            }));

        static Task<MediaPickerResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => PlatformMediaAsync(options, false);

        static async Task<MediaPickerResult> PlatformMediaAsync(MediaPickerOptions options, bool photo)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            await Permissions.EnsureGrantedAsync<Permissions.StorageRead>();

            var tcs = new TaskCompletionSource<MediaPickerResult>();

            var appControl = new AppControl();
            appControl.Operation = photo ? AppControlOperations.ImageCapture : AppControlOperations.VideoCapture;
            appControl.LaunchMode = AppControlLaunchMode.Group;

            var appId = AppControl.GetMatchedApplicationIds(appControl)?.FirstOrDefault();

            if (!string.IsNullOrEmpty(appId))
                appControl.ApplicationId = appId;

            AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
            {
                if (result == AppControlReplyResult.Succeeded && reply.ExtraData.Count() > 0)
                {
                    var file = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected)?.FirstOrDefault();
                    tcs.TrySetResult(new MediaPickerResult(file));
                }
                else
                {
                    tcs.TrySetCanceled();
                }
            });

            return await tcs.Task;
        }
    }
}
