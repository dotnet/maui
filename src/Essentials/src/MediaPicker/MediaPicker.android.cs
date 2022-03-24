using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Media
{
	public partial class MediaPickerImplementation : IMediaPicker
	{
		public bool IsCaptureSupported
			=> Platform.AppContext.PackageManager.HasSystemFeature(PackageManager.FeatureCameraAny);

		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> PickAsync(options, true);

		public Task<FileResult> PickVideoAsync(MediaPickerOptions options)
			=> PickAsync(options, false);

		public async Task<FileResult> PickAsync(MediaPickerOptions options, bool photo)
		{
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

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, true);

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, false);

		public async Task<FileResult> CaptureAsync(MediaPickerOptions options, bool photo)
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
				var tmpFile = FileSystem.GetTemporaryFile(Platform.AppContext.CacheDir, fileName);

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
