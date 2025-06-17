using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;
using AndroidUri = Android.Net.Uri;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{		
		public bool IsCaptureSupported
			=> Application.Context?.PackageManager?.HasSystemFeature(PackageManager.FeatureCameraAny) ?? false;

		internal static bool IsPhotoPickerAvailable
			=> PickVisualMedia.InvokeIsPhotoPickerAvailable(Platform.AppContext);

		[Obsolete("Switch to PickPhotoAsync which also allows multiple selections.")]
		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> PickAsync(options, true);

		public Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions options)
			=> PickMultipleAsync(options, true);

		[Obsolete("Switch to PickVideosAsync which also allows multiple selections.")]
		public Task<FileResult> PickVideoAsync(MediaPickerOptions options)
			=> PickAsync(options, false);

		public Task<List<FileResult>> PickVideosAsync(MediaPickerOptions options)
			=> PickMultipleAsync(options, false);

		public async Task<FileResult> PickAsync(MediaPickerOptions options, bool photo)
			=> IsPhotoPickerAvailable
				? await PickUsingPhotoPicker(options, photo)
				: await PickUsingIntermediateActivity(options, photo);

		public async Task<List<FileResult>> PickMultipleAsync(MediaPickerOptions options, bool photo)
			=> IsPhotoPickerAvailable
				? await PickMultipleUsingPhotoPicker(options, photo)
				: await PickMultipleUsingIntermediateActivity(options, photo);

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, true);

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, false);

		public async Task<FileResult> CaptureAsync(MediaPickerOptions options, bool photo)
		{
			if (!IsCaptureSupported)
			{
				throw new FeatureNotSupportedException();
			}

			await Permissions.EnsureGrantedAsync<Permissions.Camera>();
			// StorageWrite no longer exists starting from Android API 33
			if (!OperatingSystem.IsAndroidVersionAtLeast(33))
				await Permissions.EnsureGrantedAsync<Permissions.StorageWrite>();

			var captureIntent = new Intent(photo ? MediaStore.ActionImageCapture : MediaStore.ActionVideoCapture);

			if (!PlatformUtils.IsIntentSupported(captureIntent))
				throw new FeatureNotSupportedException($"Either there was no camera on the device or '{captureIntent.Action}' was not added to the <queries> element in the app's manifest file. See more: https://developer.android.com/about/versions/11/privacy/package-visibility");

			captureIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
			captureIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);

			try
			{
				var activity = ActivityStateManager.Default.GetCurrentActivity(true);

				string captureResult = null;

				if (photo)
				{
					captureResult = await CapturePhotoAsync(captureIntent);
				}
				else
				{
					captureResult = await CaptureVideoAsync(captureIntent);
				}

				// Return the file that we just captured
				return captureResult is not null ? new FileResult(captureResult) : null;
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}		
		
		async Task<FileResult> PickUsingIntermediateActivity(MediaPickerOptions options, bool photo)
		{
			var intent = new Intent(Intent.ActionGetContent);
			intent.SetType(photo ? FileMimeTypes.ImageAll : FileMimeTypes.VideoAll);

			var pickerIntent = Intent.CreateChooser(intent, options?.Title);

			if (pickerIntent is null)
			{
				return null;
			}

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

				return path is not null ? new FileResult(path) : null;
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		async Task<FileResult> PickUsingPhotoPicker(MediaPickerOptions options, bool photo)
		{
			var pickVisualMediaRequest = new PickVisualMediaRequest.Builder()
				.SetMediaType(photo ? ActivityResultContracts.PickVisualMedia.ImageOnly.Instance : ActivityResultContracts.PickVisualMedia.VideoOnly.Instance)
				.Build();

			var androidUri = await PickVisualMediaForResult.Instance.Launch(pickVisualMediaRequest);

			if (androidUri?.Equals(AndroidUri.Empty) ?? true)
			{
				return null;
			}

			var path = FileSystemUtils.EnsurePhysicalPath(androidUri);
			return new FileResult(path);
		}

		async Task<List<FileResult>> PickMultipleUsingPhotoPicker(MediaPickerOptions options, bool photo)
		{
			// Android has a limitation that you need to use a different request for single and multiple picks.
			// If the selection limit is 1, we can use the single pick method,
			// otherwise we need to use the multiple pick method.
			if (options.SelectionLimit == 1)
			{
				var singleResult = await PickUsingPhotoPicker(options, photo);
				return singleResult is not null ? [singleResult] : null;
			}
			
			var pickVisualMediaRequestBuilder = new PickVisualMediaRequest.Builder()
				.SetMediaType(photo ? ActivityResultContracts.PickVisualMedia.ImageOnly.Instance : ActivityResultContracts.PickVisualMedia.VideoOnly.Instance);

			// Only set the limit for 2 and up. For single selection (limit == 1) is handled above,
			// and limit == 0 should be treated as unlimited.
			if (options.SelectionLimit >= 2)
			{
				pickVisualMediaRequestBuilder.SetMaxItems(options.SelectionLimit);
			}

			var pickVisualMediaRequest = pickVisualMediaRequestBuilder.Build();

			var androidUris = await PickMultipleVisualMediaForResult.Instance.Launch(pickVisualMediaRequest);

			if (androidUris?.IsEmpty ?? true)
			{
				return [];
			}

			var resultList = new List<FileResult>();

            for (var i = 0; i < androidUris.Size(); i++)
			{
				var uri = androidUris.Get(i) as AndroidUri;
				if (!uri?.Equals(AndroidUri.Empty) ?? false)
				{
					var path = FileSystemUtils.EnsurePhysicalPath(uri);
					resultList.Add(new FileResult(path));
				}
			}

			return resultList;
		}

		async Task<string> CapturePhotoAsync(Intent captureIntent)
		{
			// Create the temporary file
			var fileName = Guid.NewGuid().ToString("N") + FileExtensions.Jpg;
			var tmpFile = FileSystemUtils.GetTemporaryFile(Application.Context.CacheDir, fileName);

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

			await IntermediateActivity.StartAsync(captureIntent, PlatformUtils.requestCodeMediaCapture, OnCreate);

			return tmpFile.AbsolutePath;
		}

		async Task<string> CaptureVideoAsync(Intent captureIntent)
		{
			string path = null;

			void OnResult(Intent intent)
			{
				// The uri returned is only temporary and only lives as long as the Activity that requested it,
				// so this means that it will always be cleaned up by the time we need it because we are using
				// an intermediate activity.
				path = FileSystemUtils.EnsurePhysicalPath(intent.Data);
			}

			// Start the capture process
			await IntermediateActivity.StartAsync(captureIntent, PlatformUtils.requestCodeMediaCapture, onResult: OnResult);

			return path;
		}
		
		async Task<List<FileResult>> PickMultipleUsingIntermediateActivity(MediaPickerOptions options, bool photo)
		{
			var intent = new Intent(Intent.ActionGetContent);
			intent.SetType(photo ? FileMimeTypes.ImageAll : FileMimeTypes.VideoAll);

			if (options is not null)
			{
				intent.PutExtra(Intent.ExtraAllowMultiple, options.SelectionLimit > 1 || options.SelectionLimit == 0);

				// Set a maximum when 2 or more. When the limit is 1 we only allow a single one and 0 should allow unlimited.
				if (options.SelectionLimit >= 2)
				{
					intent.PutExtra(MediaStore.ExtraPickImagesMax, options.SelectionLimit);
				}
			}

			var pickerIntent = Intent.CreateChooser(intent, options?.Title);
			
			if (pickerIntent is null)
			{
				return [];
			}

			try
			{
				var resultList = new List<FileResult>();
				void OnResult(Intent resultIntent)
				{
					// The uri returned is only temporary and only lives as long as the Activity that requested it,
					// so this means that it will always be cleaned up by the time we need it because we are using
					// an intermediate activity.
					
					if (resultIntent.ClipData is null)
					{
						// Single selection result
						if (resultIntent.Data is not null)
						{
							var path = FileSystemUtils.EnsurePhysicalPath(resultIntent.Data);
							resultList.Add(new FileResult(path));
						}
					}
					else
					{	
						for (var i = 0; i < resultIntent.ClipData.ItemCount; i++)
						{
							var uri = resultIntent.ClipData.GetItemAt(i)?.Uri;
							if (uri is not null)
							{
								var path = FileSystemUtils.EnsurePhysicalPath(uri);
								resultList.Add(new FileResult(path));
							}
						}
					}
				}

				await IntermediateActivity.StartAsync(pickerIntent, PlatformUtils.requestCodeMediaPicker, onResult: OnResult);

				return resultList;
			}
			catch (OperationCanceledException)
			{
				return [];
			}
		}
	}
}