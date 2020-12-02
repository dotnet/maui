using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using AndroidUri = Android.Net.Uri;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static bool PlatformIsCaptureSupported
            => Platform.AppContext.PackageManager.HasSystemFeature(PackageManager.FeatureCameraAny);

        static Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options)
            => PlatformPickAsync(options, true);

        static Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options)
            => PlatformPickAsync(options, false);

        static async Task<FileResult> PlatformPickAsync(MediaPickerOptions options, bool photo)
        {
            // We only need the permission when accessing the file, but it's more natural
            // to ask the user first, then show the picker.
            await Permissions.RequestAsync<Permissions.StorageRead>();

            var intent = new Intent(Intent.ActionGetContent);
            intent.SetType(photo ? FileSystem.MimeTypes.ImageAll : FileSystem.MimeTypes.VideoAll);

            var pickerIntent = Intent.CreateChooser(intent, options?.Title);

            try
            {
                string path = null;
                void OnResult(Intent intent)
                {
                    // The uri returned is only temporary and only lives as long as the Activity that requested it,
                    // so this means that it will always be cleaned up by the time we need it because we are using
                    // an intermediate activity.

                    path = FileSystem.EnsurePhysicalPath(intent.Data);
                }

                await IntermediateActivity.StartAsync(pickerIntent, Platform.requestCodeMediaPicker, onResult: OnResult);

                return new FileResult(path);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        static Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
            => PlatformCaptureAsync(options, true);

        static Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
            => PlatformCaptureAsync(options, false);

        static async Task<FileResult> PlatformCaptureAsync(MediaPickerOptions options, bool photo)
        {
            await Permissions.EnsureGrantedAsync<Permissions.Camera>();
            await Permissions.EnsureGrantedAsync<Permissions.StorageWrite>();

            var capturePhotoIntent = new Intent(photo ? MediaStore.ActionImageCapture : MediaStore.ActionVideoCapture);

            if (!Platform.IsIntentSupported(capturePhotoIntent))
                throw new FeatureNotSupportedException($"Either there was no camera on the device or '{capturePhotoIntent.Action}' was not added to the <queries> element in the app's manifest file. See more: https://developer.android.com/about/versions/11/privacy/package-visibility");

            capturePhotoIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
            capturePhotoIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);

            try
            {
                var activity = Platform.GetCurrentActivity(true);

                // Create the temporary file
                var ext = photo
                    ? FileSystem.Extensions.Jpg
                    : FileSystem.Extensions.Mp4;
                var fileName = Guid.NewGuid().ToString("N") + ext;
                var tmpFile = FileSystem.GetEssentialsTemporaryFile(Platform.AppContext.CacheDir, fileName);

                // Set up the content:// uri
                AndroidUri outputUri = null;
                void OnCreate(Intent intent)
                {
                    // Android requires that using a file provider to get a content:// uri for a file to be called
                    // from within the context of the actual activity which may share that uri with another intent
                    // it launches.

                    outputUri ??= FileProvider.GetUriForFile(tmpFile);

                    intent.PutExtra(MediaStore.ExtraOutput, outputUri);
                }

                // Start the capture process
                await IntermediateActivity.StartAsync(capturePhotoIntent, Platform.requestCodeMediaCapture, OnCreate);

                // Return the file that we just captured
                return new FileResult(tmpFile.AbsolutePath);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}
