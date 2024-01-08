using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{
		public bool IsCaptureSupported
			=> Application.Context.PackageManager.HasSystemFeature(PackageManager.FeatureCameraAny);

		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> PickAsync(options, true);

		public Task<FileResult> PickVideoAsync(MediaPickerOptions options)
			=> PickAsync(options, false);

		public async Task<FileResult> PickAsync(MediaPickerOptions options, bool photo)
		{
			var intent = new Intent(Intent.ActionGetContent);
			intent.SetType(photo ? FileMimeTypes.ImageAll : FileMimeTypes.VideoAll);

			var pickerIntent = Intent.CreateChooser(intent, options?.Title);

			try
			{
				string path = null;
				void OnResult(Intent intent)
				{
					// The uri returned is only temporary and only lives as long as the Activity that requested it,
					// so this means that it will always be cleaned up by the time we need it because we are using
					// an intermediate activity.

					path = FileSystemUtils.EnsurePhysicalPath(intent.Data);
				}

				await IntermediateActivity.StartAsync(pickerIntent, PlatformUtils.requestCodeMediaPicker, onResult: OnResult);

				return new FileResult(path);
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, true);

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, false);

		public async Task<FileResult> CaptureAsync(MediaPickerOptions options, bool photo)
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			await Permissions.EnsureGrantedAsync<Permissions.Camera>();
			// StorageWrite no longer exists starting from Android API 33
			if (!OperatingSystem.IsAndroidVersionAtLeast(33))
				await Permissions.EnsureGrantedAsync<Permissions.StorageWrite>();

			var capturePhotoIntent = new Intent(photo ? MediaStore.ActionImageCapture : MediaStore.ActionVideoCapture);

			if (!PlatformUtils.IsIntentSupported(capturePhotoIntent))
				throw new FeatureNotSupportedException($"Either there was no camera on the device or '{capturePhotoIntent.Action}' was not added to the <queries> element in the app's manifest file. See more: https://developer.android.com/about/versions/11/privacy/package-visibility");

			capturePhotoIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
			capturePhotoIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);

			try
			{
				var activity = ActivityStateManager.Default.GetCurrentActivity(true);

				// Create the temporary file
				var ext = photo
					? FileExtensions.Jpg
					: FileExtensions.Mp4;
				var fileName = Guid.NewGuid().ToString("N") + ext;
				var tmpFile = FileSystemUtils.GetTemporaryFile(Application.Context.CacheDir, fileName);

				string path = null;

				void OnResult(Intent intent)
				{
					// The uri returned is only temporary and only lives as long as the Activity that requested it,
					// so this means that it will always be cleaned up by the time we need it because we are using
					// an intermediate activity.

					path = FileSystemUtils.EnsurePhysicalPath(intent.Data);
				}

				// Start the capture process
				await IntermediateActivity.StartAsync(capturePhotoIntent, PlatformUtils.requestCodeMediaCapture, onResult: OnResult);

				// Return the file that we just captured
				return new FileResult(path);
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}
	}
}
