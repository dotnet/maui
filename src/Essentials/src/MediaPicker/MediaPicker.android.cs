using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Provider;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Essentials;
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
			// Only request storage write permission when saving to gallery on older Android versions
			if (options?.SaveToGallery == true && !OperatingSystem.IsAndroidVersionAtLeast(29))
				await Permissions.EnsureGrantedAsync<Permissions.StorageWrite>();

			var captureIntent = new Intent(photo ? MediaStore.ActionImageCapture : MediaStore.ActionVideoCapture);

			if (!PlatformUtils.IsIntentSupported(captureIntent))
				throw new FeatureNotSupportedException($"Either there was no camera on the device or '{captureIntent.Action}' was not added to the <queries> element in the app's manifest file. See more: https://developer.android.com/about/versions/11/privacy/package-visibility");

			captureIntent.AddFlags(global::Android.Content.ActivityFlags.GrantReadUriPermission);
			captureIntent.AddFlags(global::Android.Content.ActivityFlags.GrantWriteUriPermission);

			try
			{
				var activity = ActivityStateManager.Default.GetCurrentActivity(true);

				string capturePath = null;

				var useActivityResultCapture = activity is ComponentActivity;

				if (photo)
				{
					capturePath = useActivityResultCapture
						? await CapturePhotoWithActivityResultAsync(options)
						: await ProcessPhotoAsync(await CapturePhotoAsync(captureIntent), options);
				}
				else
				{
					capturePath = useActivityResultCapture
						? await CaptureVideoWithActivityResultAsync(options)
						: await CaptureVideoAsync(captureIntent);
				}

				// Save to gallery if requested
				if (capturePath is not null && options?.SaveToGallery == true)
				{
					await SaveToGalleryAsync(capturePath, photo);
				}

				// Return the file that we just captured
				return capturePath is not null ? new FileResult(capturePath) : null;
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		static async Task<string> CapturePhotoWithActivityResultAsync(MediaPickerOptions options)
		{
			var fileName = Guid.NewGuid().ToString("N") + FileExtensions.Jpg;
			var captureFile = FileSystemUtils.GetTemporaryFile(Application.Context.CacheDir, fileName);
			var outputUri = FileProvider.GetUriForFile(captureFile);

			var processingOptions = GetPhotoProcessingOptions(options);
			var pendingOperation = await MediaPickerRecoveryManager.BeginOperationWithRecoveryAsync(
				RecoveredMediaPickerResultKind.CapturePhoto,
				[captureFile.AbsolutePath],
				processingOptions);

			try
			{
				var result = await CapturePhotoForResult.Instance.Launch(outputUri);

				if (result?.BooleanValue() != true || !MediaPickerRecoveryManager.IsFileAvailable(captureFile.AbsolutePath))
				{
					return null;
				}

				return await ProcessPhotoPreservingSourceAsync(captureFile.AbsolutePath, processingOptions);
			}
			finally
			{
				// The live task completed or failed, so prevent the same capture from being published as recovered later.
				MediaPickerRecoveryManager.ClearActiveOperation(pendingOperation.Id);
			}
		}

		async Task<string> CaptureVideoWithActivityResultAsync(MediaPickerOptions options)
		{
			var fileName = Guid.NewGuid().ToString("N") + FileExtensions.Mp4;
			var captureFile = FileSystemUtils.GetTemporaryFile(Application.Context.CacheDir, fileName);
			var outputUri = FileProvider.GetUriForFile(captureFile);

			var pendingOperation = await MediaPickerRecoveryManager.BeginOperationWithRecoveryAsync(
				RecoveredMediaPickerResultKind.CaptureVideo,
				[captureFile.AbsolutePath],
				PersistedPhotoProcessingOptions.Default);

			try
			{
				var result = await CaptureVideoForResult.Instance.Launch(outputUri);

				if (result?.BooleanValue() != true || !MediaPickerRecoveryManager.IsFileAvailable(captureFile.AbsolutePath))
				{
					return null;
				}

				return captureFile.AbsolutePath;
			}
			finally
			{
				// The live task completed or failed, so prevent the same capture from being published as recovered later.
				MediaPickerRecoveryManager.ClearActiveOperation(pendingOperation.Id);
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

				if (path is not null)
				{
					if (photo)
					{
						path = await ProcessPhotoAsync(path, options);
					}

					return new FileResult(path);
				}

				return null;
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		async Task<FileResult> PickUsingPhotoPicker(
			MediaPickerOptions options,
			bool photo,
			RecoveredMediaPickerResultKind? operationKind = null)
		{
			var pickVisualMediaRequest = new PickVisualMediaRequest.Builder()
				.SetMediaType(photo ? ActivityResultContracts.PickVisualMedia.ImageOnly.Instance : ActivityResultContracts.PickVisualMedia.VideoOnly.Instance)
				.Build();

			var processingOptions = GetPhotoProcessingOptions(options);
			var pendingOperation = await MediaPickerRecoveryManager.BeginOperationWithRecoveryAsync(
				operationKind ?? (photo ? RecoveredMediaPickerResultKind.PickPhoto : RecoveredMediaPickerResultKind.PickVideo),
				[],
				processingOptions);

			try
			{
				var androidUri = await PickVisualMediaForResult.Instance.Launch(pickVisualMediaRequest);

				if (androidUri?.Equals(AndroidUri.Empty) ?? true)
				{
					return null;
				}

				var acceptedPaths = await MediaPickerRecoveryManager.MaterializeAcceptedFilePathsAsync(pendingOperation.Id, throwOnMaterializationFailure: true);
				var path = acceptedPaths.FirstOrDefault() ?? await FileSystemUtils.EnsurePhysicalPathAsync(androidUri);

				if (photo)
				{
					path = await ProcessPhotoPreservingSourceAsync(path, processingOptions);
				}

				return new FileResult(path);
			}
			finally
			{
				// The live task completed or failed, so prevent the same pick from being published as recovered later.
				MediaPickerRecoveryManager.ClearActiveOperation(pendingOperation.Id);
			}
		}

		async Task<List<FileResult>> PickMultipleUsingPhotoPicker(MediaPickerOptions options, bool photo)
		{
			// Android has a limitation that you need to use a different request for single and multiple picks.
			// If the selection limit is 1, we can use the single pick method,
			// otherwise we need to use the multiple pick method.
			int selectionLimit = options?.SelectionLimit ?? 1;
			if (selectionLimit == 1)
			{
				var singleResult = await PickUsingPhotoPicker(
					options,
					photo,
					photo ? RecoveredMediaPickerResultKind.PickPhotos : RecoveredMediaPickerResultKind.PickVideos);
				return singleResult is not null ? [singleResult] : [];
			}

			var pickVisualMediaRequestBuilder = new PickVisualMediaRequest.Builder()
				.SetMediaType(photo ? ActivityResultContracts.PickVisualMedia.ImageOnly.Instance : ActivityResultContracts.PickVisualMedia.VideoOnly.Instance);

			// Only set the limit for 2 and up. For single selection (limit == 1) is handled above,
			// and limit == 0 should be treated as unlimited.
			if (selectionLimit >= 2)
			{
				pickVisualMediaRequestBuilder.SetMaxItems(selectionLimit);
			}

			var processingOptions = GetPhotoProcessingOptions(options);
			var pendingOperation = await MediaPickerRecoveryManager.BeginOperationWithRecoveryAsync(
				photo ? RecoveredMediaPickerResultKind.PickPhotos : RecoveredMediaPickerResultKind.PickVideos,
				[],
				processingOptions);

			try
			{
				var pickVisualMediaRequest = pickVisualMediaRequestBuilder.Build();
				var androidUris = await PickMultipleVisualMediaForResult.Instance.Launch(pickVisualMediaRequest);

				if (androidUris?.IsEmpty ?? true)
					return [];

				var acceptedPaths = await MediaPickerRecoveryManager.MaterializeAcceptedFilePathsAsync(pendingOperation.Id, throwOnMaterializationFailure: true);

				var resultList = new List<FileResult>();

				foreach (var acceptedPath in acceptedPaths)
				{
					var path = acceptedPath;

					if (photo)
						path = await ProcessPhotoPreservingSourceAsync(path, processingOptions);

					resultList.Add(new FileResult(path));
				}

				return resultList;
			}
			finally
			{
				// The live task completed or failed, so prevent the same pick from being published as recovered later.
				MediaPickerRecoveryManager.ClearActiveOperation(pendingOperation.Id);
			}
		}

		async Task<string> CapturePhotoAsync(Intent captureIntent)
		{
			// Create the temporary file
			var fileName = Guid.NewGuid().ToString("N") + FileExtensions.Jpg;
			var captureFile = FileSystemUtils.GetTemporaryFile(Application.Context.CacheDir, fileName);

			// Set up the content:// uri
			AndroidUri outputUri = null;

			void OnCreate(Intent intent)
			{
				// Android requires that using a file provider to get a content:// uri for a file to be called
				// from within the context of the actual activity which may share that uri with another intent
				// it launches.
				outputUri ??= FileProvider.GetUriForFile(captureFile);

				intent.PutExtra(MediaStore.ExtraOutput, outputUri);
			}

			await IntermediateActivity.StartAsync(captureIntent, PlatformUtils.requestCodeMediaCapture, OnCreate);

			return captureFile.AbsolutePath;
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

		/// <summary>
		/// Saves the captured media file to the device's gallery.
		/// On API 29+, uses MediaStore with scoped storage and IsPending flag. On older versions, copies to public external storage and scans the copied file.
		/// </summary>
		static async Task SaveToGalleryAsync(string filePath, bool isPhoto)
		{
			var context = Application.Context ?? throw new InvalidOperationException("An Android application context is required to save media to the gallery.");
			var fileName = System.IO.Path.GetFileName(filePath);
			var extension = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
			var mimeType = GetMimeType(extension, isPhoto);

			if (OperatingSystem.IsAndroidVersionAtLeast(29))
			{
				await SaveToMediaStoreAsync(context, filePath, fileName, mimeType, isPhoto);
				return;
			}

			SaveToExternalStorageAndScan(context, filePath, fileName, mimeType, isPhoto);
		}

		static async Task SaveToMediaStoreAsync(Context context, string filePath, string fileName, string mimeType, bool isPhoto)
		{
			var contentResolver = context.ContentResolver ?? throw new InvalidOperationException("An Android content resolver is required to save media to the gallery.");
			var contentValues = new ContentValues();
			contentValues.Put(MediaStore.IMediaColumns.DisplayName, fileName);
			contentValues.Put(MediaStore.IMediaColumns.MimeType, mimeType);
			contentValues.Put(MediaStore.IMediaColumns.RelativePath,
				isPhoto ? global::Android.OS.Environment.DirectoryPictures : global::Android.OS.Environment.DirectoryMovies);
			contentValues.Put(MediaStore.IMediaColumns.IsPending, 1);

			var collection = isPhoto
				? MediaStore.Images.Media.ExternalContentUri
				: MediaStore.Video.Media.ExternalContentUri;

			var insertUri = contentResolver.Insert(collection, contentValues)
				?? throw new IOException("Unable to create a MediaStore entry for the captured media.");

			try
			{
				using (var outputStream = contentResolver.OpenOutputStream(insertUri) ?? throw new IOException("Unable to open the MediaStore entry for writing."))
				using (var inputStream = File.OpenRead(filePath))
				{
					await inputStream.CopyToAsync(outputStream);
				}

				contentValues.Clear();
				contentValues.Put(MediaStore.IMediaColumns.IsPending, 0);

				if (contentResolver.Update(insertUri, contentValues, null, null) == 0)
				{
					throw new IOException("Unable to publish the captured media to the gallery.");
				}
			}
			catch (Exception saveException)
			{
				try
				{
					contentResolver.Delete(insertUri, null, null);
				}
				catch (Exception cleanupException)
				{
					throw new System.AggregateException("Failed to save media to the gallery and clean up the pending MediaStore entry.", saveException, cleanupException);
				}

				throw;
			}
		}

		static void SaveToExternalStorageAndScan(Context context, string filePath, string fileName, string mimeType, bool isPhoto)
		{
			var directory = global::Android.OS.Environment.GetExternalStoragePublicDirectory(
				isPhoto ? global::Android.OS.Environment.DirectoryPictures : global::Android.OS.Environment.DirectoryMovies);

			if (directory?.AbsolutePath is not string directoryPath || string.IsNullOrEmpty(directoryPath))
			{
				throw new IOException("Unable to find the public gallery directory for the captured media.");
			}

			Directory.CreateDirectory(directoryPath);

			var destinationPath = System.IO.Path.Combine(directoryPath, fileName);
			if (!string.Equals(filePath, destinationPath, StringComparison.Ordinal))
			{
				File.Copy(filePath, destinationPath, overwrite: true);
			}

			global::Android.Media.MediaScannerConnection.ScanFile(
				context,
				new[] { destinationPath },
				new[] { mimeType },
				null);
		}

		static string GetMimeType(string extension, bool isPhoto)
		{
			return extension switch
			{
				".jpg" or ".jpeg" => "image/jpeg",
				".png" => "image/png",
				".heic" or ".heif" => "image/heif",
				".webp" => "image/webp",
				".gif" => "image/gif",
				".mp4" => "video/mp4",
				".3gp" => "video/3gpp",
				".mkv" => "video/x-matroska",
				".webm" => "video/webm",
				_ => isPhoto ? "image/jpeg" : "video/mp4",
			};
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

				// Process images if necessary
				if (photo)
				{
					var tempResultList = resultList.Select(fr => fr.FullPath).ToList();
					resultList.Clear();

					foreach (var path in tempResultList)
					{
						var processedPath = await ProcessPhotoAsync(path, options);
						resultList.Add(new FileResult(processedPath));
					}
				}

				return resultList;
			}
			catch (OperationCanceledException)
			{
				return [];
			}
		}

		// --- Photo processing (shared MAUI Graphics pipeline) -------------------------------------

		// Entry point used by the picker paths: resolves the MediaPickerOptions and processes the photo.
		internal static Task<string> ProcessPhotoAsync(string imagePath, MediaPickerOptions options)
			=> ProcessPhotoPreservingSourceAsync(imagePath, GetPhotoProcessingOptions(options));

		// Loads the image through MAUI Graphics (applying EXIF orientation and capturing metadata per the
		// options), applies any resize, and writes the result to a new MAUI-owned cache file that
		// preserves the original file name. The source file is only ever read, never modified.
		//
		// When processing is performed this ALWAYS returns a separate MAUI-owned file (never the source),
		// so on a processing failure we copy the source bytes to the new file rather than handing back the
		// source path. The source path is only returned unchanged in the early-out cases below (nothing to
		// do, or the source no longer exists). Also used directly by MediaPickerRecoveryManager.
		internal static async Task<string> ProcessPhotoPreservingSourceAsync(string imagePath, PersistedPhotoProcessingOptions options)
		{
			if (imagePath is null)
			{
				return null;
			}

			// Nothing to do unless the caller asked to rotate, resize, or recompress.
			if (!options.RotateImage &&
				!ImageProcessor.IsProcessingNeeded(options.MaximumWidth, options.MaximumHeight, options.CompressionQuality))
			{
				return imagePath;
			}

			if (!File.Exists(imagePath))
			{
				return imagePath;
			}

			// Preserve the original container (JPEG/PNG). Deterministic: no automatic format switching.
			var format = ImageProcessor.GetOutputFormat(imagePath);
			var outputFileName = System.IO.Path.GetFileNameWithoutExtension(imagePath) + ImageProcessor.GetOutputExtension(format);
			var outputFile = FileSystemUtils.GetTemporaryFile(Application.Context.CacheDir, outputFileName);
			var outputPath = outputFile.AbsolutePath;

			var processingOptions = new ImageProcessingOptions(
				options.MaximumWidth,
				options.MaximumHeight,
				options.CompressionQuality,
				options.RotateImage,
				options.PreserveMetaData);

			try
			{
				using (var input = File.OpenRead(imagePath))
				using (var output = File.Create(outputPath))
				{
					await ImageProcessor.ProcessImageAsync(input, output, format, processingOptions);
				}

				return outputPath;
			}
			catch
			{
				// Processing failed (e.g. an undecodable image). Still hand back a separate MAUI-owned
				// copy so callers never receive the untouched source path.
				try
				{
					File.Copy(imagePath, outputPath, overwrite: true);
					return outputPath;
				}
				catch
				{
					return imagePath;
				}
			}
		}

		internal static PersistedPhotoProcessingOptions GetPhotoProcessingOptions(MediaPickerOptions options)
			=> new(
				options?.MaximumWidth,
				options?.MaximumHeight,
				options?.CompressionQuality ?? 100,
				options?.RotateImage ?? false,
				options?.PreserveMetaData ?? true);
	}
}
