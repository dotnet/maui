using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Storage;
using MobileCoreServices;
using Photos;
using PhotosUI;
using UIKit;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{
		static UIViewController PickerRef;

		public bool IsCaptureSupported
			=> UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> PhotoAsync(options, true, true);

		public Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions options)
			=> PhotosAsync(options, true, true);

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options)
		{
			if (!IsCaptureSupported)
			{
				throw new FeatureNotSupportedException();
			}

			return PhotoAsync(options, true, false);
		}

		public Task<FileResult> PickVideoAsync(MediaPickerOptions options)
			=> PhotoAsync(options, false, true);

		public Task<List<FileResult>> PickVideosAsync(MediaPickerOptions options)
			=> PhotosAsync(options, false, true);

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options)
		{
			if (!IsCaptureSupported)
			{
				throw new FeatureNotSupportedException();
			}

			return PhotoAsync(options, false, false);
		}

		public async Task<FileResult> PhotoAsync(MediaPickerOptions options, bool photo, bool pickExisting)
		{
			if (!photo && !pickExisting)
			{
				await Permissions.EnsureGrantedAsync<Permissions.Microphone>();
			}

			// Check if picking existing or not and ensure permission accordingly as they can be set independently from each other
			if (pickExisting && !OperatingSystem.IsIOSVersionAtLeast(11, 0))
			{
				await Permissions.EnsureGrantedAsync<Permissions.Photos>();
			}

			if (!pickExisting)
			{
				await Permissions.EnsureGrantedAsync<Permissions.Camera>();
			}

			var vc = WindowStateManager.Default.GetCurrentUIViewController(true);
			var tcs = new TaskCompletionSource<FileResult>();

			if (pickExisting && OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				var config = new PHPickerConfiguration
				{
					Filter = photo
						? PHPickerFilter.ImagesFilter
						: PHPickerFilter.VideosFilter
				};

				var picker = new PHPickerViewController(config)
				{
					Delegate = new Media.PhotoPickerDelegate
					{
						CompletedHandler = res =>
							tcs.TrySetResult(PickerResultsToMediaFile(res))
					}
				};

				PickerRef = picker;
			}
			else
			{
				if (!pickExisting && options?.SaveToGallery == true)
				{
					await Permissions.EnsureGrantedAsync<Permissions.PhotosAddOnly>();
				}

				var sourceType = pickExisting
					? UIImagePickerControllerSourceType.PhotoLibrary
					: UIImagePickerControllerSourceType.Camera;

				var mediaType = photo ? UTType.Image : UTType.Movie;

				if (!UIImagePickerController.IsSourceTypeAvailable(sourceType))
				{
					tcs.TrySetCanceled();
					throw new FeatureNotSupportedException();
				}

				if (!UIImagePickerController.AvailableMediaTypes(sourceType).Contains(mediaType))
				{
					tcs.TrySetCanceled();
					throw new FeatureNotSupportedException();
				}

				var picker = new UIImagePickerController
				{
					SourceType = sourceType,
					MediaTypes = [mediaType],
					AllowsEditing = false
				};

				if (!photo && !pickExisting)
				{
					picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Video;
				}

				PickerRef = picker;

				picker.Delegate = new PhotoPickerDelegate
				{
					CompletedHandler = info =>
					{
						GetFileResult(info, tcs, options);
					}
				};
			}

			if (!string.IsNullOrWhiteSpace(options?.Title))
			{
				PickerRef.Title = options.Title;
			}

			if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
			{
				PickerRef.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
			}

			if (PickerRef.PresentationController is not null)
			{
				PickerRef.PresentationController.Delegate = new PhotoPickerPresentationControllerDelegate
				{
					Handler = () => tcs.TrySetResult(null)
				};
			}

			await vc.PresentViewControllerAsync(PickerRef, true);

			var result = await tcs.Task;

			PickerRef?.Dispose();
			PickerRef = null;

			// Save captured media to the photo gallery if requested
			if (!pickExisting && result is not null && options?.SaveToGallery == true)
			{
				await SaveToPhotoLibraryAsync(result);
			}

			return result;
		}

		async Task<List<FileResult>> PhotosAsync(MediaPickerOptions options, bool photo, bool pickExisting)
		{
			// iOS 14+ only supports multiple selection
			// TODO throw exception?
			if (!OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				return [];
			}

			if (!photo && !pickExisting)
			{
				await Permissions.EnsureGrantedAsync<Permissions.Microphone>();
			}

			// Check if picking existing or not and ensure permission accordingly as they can be set independently from each other
			if (pickExisting && !OperatingSystem.IsIOSVersionAtLeast(11, 0))
			{
				await Permissions.EnsureGrantedAsync<Permissions.Photos>();
			}

			if (!pickExisting)
			{
				await Permissions.EnsureGrantedAsync<Permissions.Camera>();
			}

			var vc = WindowStateManager.Default.GetCurrentUIViewController(true);
			var tcs = new TaskCompletionSource<List<FileResult>>();

			if (pickExisting)
			{
				var config = new PHPickerConfiguration
				{
					Filter = photo
						? PHPickerFilter.ImagesFilter
						: PHPickerFilter.VideosFilter,
					SelectionLimit = options?.SelectionLimit ?? 1,
				};

				var picker = new PHPickerViewController(config)
				{
					Delegate = new Media.PhotoPickerDelegate
					{
						CompletedHandler = async res =>
						{
							var result = await PickerResultsToMediaFiles(res, options);
							tcs.TrySetResult(result);
						}
					}
				};

				PickerRef = picker;
			}

			if (!string.IsNullOrWhiteSpace(options?.Title))
			{
				PickerRef.Title = options.Title;
			}

			if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
			{
				PickerRef.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
			}

			if (PickerRef.PresentationController is not null)
			{
				PickerRef.PresentationController.Delegate = new PhotoPickerPresentationControllerDelegate
				{
					Handler = () => tcs.TrySetResult([])
				};
			}

			await vc.PresentViewControllerAsync(PickerRef, true);

			var result = await tcs.Task;

			PickerRef?.Dispose();
			PickerRef = null;

			return result;
		}

		static FileResult PickerResultsToMediaFile(PHPickerResult[] results)
		{
			var file = results?.FirstOrDefault();

			return file == null
				? null
				: new PHPickerFileResult(file.ItemProvider);
		}

		static Task<List<FileResult>> PickerResultsToMediaFiles(PHPickerResult[] results, MediaPickerOptions options = null)
		{
			// Handle null or empty results (cancellation) - return empty list per API contract
			if (results == null || results.Length == 0)
				return Task.FromResult(new List<FileResult>());

			// Rotation, resizing and recompression are all handled lazily by the single Graphics-based
			// processing wrapper below (see PHPickerProcessedFileResult).
			var needsProcessing = ImageProcessor.IsProcessingNeeded(options);
			var processingOptions = new ImageProcessingOptions(
				options?.MaximumWidth,
				options?.MaximumHeight,
				options?.CompressionQuality ?? 100,
				options?.RotateImage ?? false,
				options?.PreserveMetaData ?? true);

			var fileResults = results
				.Select(file =>
				{
					FileResult result = new PHPickerFileResult(file.ItemProvider);
					if (needsProcessing)
					{
						result = new PHPickerProcessedFileResult(result, processingOptions);
					}
					return result;
				})
				.ToList();

			return Task.FromResult(fileResults);
		}

		static void GetFileResult(NSDictionary info, TaskCompletionSource<FileResult> tcs, MediaPickerOptions options = null)
		{
			try
			{
				tcs.TrySetResult(DictionaryToMediaFile(info, options));
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		static FileResult DictionaryToMediaFile(NSDictionary info, MediaPickerOptions options = null)
		{
			// This method should only be called for iOS < 14
			if (!OperatingSystem.IsIOSVersionAtLeast(14))
			{
				return null;
			}

			if (info is null)
			{
				return null;
			}

			PHAsset phAsset = null;
			NSUrl assetUrl = null;

			if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
			{
				assetUrl = info[UIImagePickerController.ImageUrl] as NSUrl;

				// Try the MediaURL sometimes used for videos
				assetUrl ??= info[UIImagePickerController.MediaURL] as NSUrl;

				if (assetUrl is not null)
				{
					if (!assetUrl.Scheme.Equals("assets-library", StringComparison.OrdinalIgnoreCase))
					{
						var docResult = new UIDocumentFileResult(assetUrl);

						// Apply rotation if needed and this is a photo
						if (ImageProcessor.IsRotationNeeded(options) && IsImageFile(docResult.FileName))
						{
							try
							{
								var rotatedResult = RotateImageFile(docResult).GetAwaiter().GetResult();
								if (rotatedResult != null)
									return rotatedResult;
							}
							catch
							{
								// If rotation fails, continue with the original file
							}
						}

						return docResult;
					}

					phAsset = info.ValueForKey(UIImagePickerController.PHAsset) as PHAsset;
				}
			}

#if !MACCATALYST
			if (phAsset is null)
			{
				assetUrl = info[UIImagePickerController.ReferenceUrl] as NSUrl;

				if (assetUrl is not null)
				{
					phAsset = PHAsset.FetchAssets([assetUrl], null)?.LastObject as PHAsset;
				}
			}
#endif

			if (phAsset is null || assetUrl is null)
			{
				var img = info.ValueForKey(UIImagePickerController.OriginalImage) as UIImage;

				if (img is not null)
				{
					// A captured UIImage is processed entirely in memory through the shared Graphics
					// pipeline (see CompressedUIImageFileResult); there is no source file to preserve.
					var processingOptions = new ImageProcessingOptions(
						options?.MaximumWidth,
						options?.MaximumHeight,
						options?.CompressionQuality ?? 100,
						options?.RotateImage ?? false,
						preserveMetadata: false);

					return new CompressedUIImageFileResult(img, null, processingOptions);
				}
			}

			if (phAsset is null || assetUrl is null)
			{
				return null;
			}

			string originalFilename = PHAssetResource.GetAssetResources(phAsset).FirstOrDefault()?.OriginalFilename;
			var assetResult = new PHAssetFileResult(assetUrl, phAsset, originalFilename);

			// Apply rotation if needed and this is a photo
			if (ImageProcessor.IsRotationNeeded(options) && IsImageFile(assetResult.FileName))
			{
				try
				{
					var rotatedResult = RotateImageFile(assetResult).GetAwaiter().GetResult();
					if (rotatedResult != null)
						return rotatedResult;
				}
				catch
				{
					// If rotation fails, continue with the original file
				}
			}

			return assetResult;
		}

		// Helper method to check if a file is an image based on extension
		static bool IsImageFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return false;

			var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
			return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".heic" || ext == ".heif";
		}

		// Helper method to rotate an image file
		static async Task<FileResult> RotateImageFile(FileResult original)
		{
			if (original == null)
				return null;

			try
			{
				using var originalStream = await original.OpenReadAsync();

				var outputPath = await ImageProcessor.ProcessImageToCacheFileAsync(
					originalStream,
					original.FileName,
					new ImageProcessingOptions(
						maximumWidth: null,
						maximumHeight: null,
						compressionQuality: 100,
						rotateImage: true,
						preserveMetadata: true));

				return new FileResult(outputPath, original.FileName);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error rotating image: {ex.Message}");
				return original;
			}
		}
		
		/// <summary>
		/// Saves the captured media file to the device's photo library using PHPhotoLibrary.
		/// </summary>
		static async Task SaveToPhotoLibraryAsync(FileResult fileResult)
		{
			string tempPath = null;

			try
			{
				using var stream = await fileResult.OpenReadAsync();
				var extension = System.IO.Path.GetExtension(fileResult.FileName);
				tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
				using (var fileStream = File.Create(tempPath))
				{
					if (stream.CanSeek)
					{
						stream.Position = 0;
					}

					await stream.CopyToAsync(fileStream);
				}

				using var url = NSUrl.FromFilename(tempPath);

				await PerformPhotoLibraryChangesAsync(() =>
				{
					if (IsImageFile(fileResult.FileName))
					{
						PHAssetChangeRequest.FromImage(url);
					}
					else
					{
						PHAssetChangeRequest.FromVideo(url);
					}
				});
			}
			finally
			{
				DeleteTemporaryPhotoLibraryFile(tempPath);
			}
		}

		static Task PerformPhotoLibraryChangesAsync(Action changeHandler)
		{
			var tcs = new TaskCompletionSource<bool>();

			PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(changeHandler, (success, error) =>
			{
				if (success)
				{
					tcs.TrySetResult(true);
				}
				else if (error is not null)
				{
					tcs.TrySetException(new NSErrorException(error));
				}
				else
				{
					tcs.TrySetException(new InvalidOperationException("Unable to save the captured media to the photo library."));
				}
			});

			return tcs.Task;
		}

		static void DeleteTemporaryPhotoLibraryFile(string tempPath)
		{
			if (string.IsNullOrEmpty(tempPath))
			{
				return;
			}

			try
			{
				File.Delete(tempPath);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to delete temporary photo library file: {ex.Message}");
			}
		}

		class PhotoPickerDelegate : UIImagePickerControllerDelegate
		{
			public Action<NSDictionary> CompletedHandler { get; set; }
			public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
            {
				picker.DismissViewController(true, () => CompletedHandler?.Invoke(info));
            }

			public override void Canceled(UIImagePickerController picker)
			{
				picker.DismissViewController(true, () => CompletedHandler?.Invoke(null));
			}
		}
	}

	class PhotoPickerDelegate : PHPickerViewControllerDelegate
	{
		public Action<PHPickerResult[]> CompletedHandler { get; set; }

		public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
		{
			// Null out the presentation delegate handler before dismiss to prevent a GC race condition.
			// Without this, Dispose() on PhotoPickerPresentationControllerDelegate can fire tcs.TrySetResult([])
			// while the async CompletedHandler is still processing (especially slow for HEIC transcoding).
			if (picker.PresentationController?.Delegate is PhotoPickerPresentationControllerDelegate pd)
			{
				pd.Handler = null;
			}

			var captured = results?.Length > 0 ? results : [];
            picker.DismissViewController(true, () => CompletedHandler?.Invoke(captured));
		}
	}

	class PhotoPickerPresentationControllerDelegate : UIAdaptivePresentationControllerDelegate
	{
		public Action Handler { get; set; }

		public override void DidDismiss(UIPresentationController presentationController) =>
			Handler?.Invoke();

		protected override void Dispose(bool disposing)
		{
			Handler?.Invoke();
			base.Dispose(disposing);
		}
	}

	class PHPickerFileResult : FileResult
	{
		readonly string _identifier;
		readonly NSItemProvider _provider;

		internal PHPickerFileResult(NSItemProvider provider)
		{
			_provider = provider;
			var identifiers = provider?.RegisteredTypeIdentifiers;

			_identifier = (identifiers?.Any(i => i.StartsWith(UTType.LivePhoto)) ?? false)
				&& (identifiers?.Contains(UTType.JPEG) ?? false)
				? identifiers?.FirstOrDefault(i => i == UTType.JPEG)
				: identifiers?.FirstOrDefault();

			if (string.IsNullOrWhiteSpace(_identifier))
			{
				return;
			}

			FileName = FullPath
				= $"{provider?.SuggestedName}.{GetTag(_identifier, UTType.TagClassFilenameExtension)}";
		}

		internal override async Task<Stream> PlatformOpenReadAsync()
			=> (await _provider?.LoadDataRepresentationAsync(_identifier))?.AsStream();

		protected internal static string GetTag(string identifier, string tagClass)
			   => UTType.CopyAllTags(identifier, tagClass)?.FirstOrDefault();
	}

	class CompressedUIImageFileResult : FileResult
	{
		readonly UIImage uiImage;
		readonly ImageProcessingOptions options;
		readonly Microsoft.Maui.Graphics.ImageFormat format;
		byte[] data;

		internal CompressedUIImageFileResult(UIImage image, string originalFileName, ImageProcessingOptions options)
			: base()
		{
			uiImage = image;
			this.options = options;

			// Deterministic output container: PNG stays PNG, everything else becomes JPEG (matching the
			// shared Graphics processor). A captured photo has no original name, so it becomes JPEG.
			format = ImageProcessor.GetOutputFormat(originalFileName);
			var extension = ImageProcessor.GetOutputExtension(format);
			FullPath = Guid.NewGuid().ToString() + extension;
			FileName = FullPath;
			ContentType = format == Microsoft.Maui.Graphics.ImageFormat.Png ? "image/png" : "image/jpeg";
		}

		internal override async Task<Stream> PlatformOpenReadAsync()
		{
			if (data is null)
			{
				// UIImage.AsJPEG/AsPNG encode the raw CGImage and ignore imageOrientation, so the
				// orientation must be baked into the pixels first. Unlike the file-based pick path there
				// is no EXIF channel to carry orientation forward, so a capture is always normalized.
				using var image = new Microsoft.Maui.Graphics.Platform.PlatformImage(uiImage.NormalizeOrientation());

				// Resize and encode entirely in memory through the shared Graphics pipeline — no file.
				using var memory = new MemoryStream();
				await ImageProcessor.SaveImageAsync(image, memory, format, options).ConfigureAwait(false);
				data = memory.ToArray();
			}

			return new MemoryStream(data, writable: false);
		}
	}

	/// <summary>
	/// Wrapper that applies compression lazily when the stream is opened.
	/// This avoids iOS resource limits when processing multiple photos.
	/// </summary>
	class PHPickerProcessedFileResult : FileResult, IDisposable
	{
		readonly FileResult _originalResult;
		readonly ImageProcessingOptions _options;

		// Path to the processed cache file, produced on the first call to PlatformOpenReadAsync and
		// reused on subsequent calls to avoid re-processing.
		string _processedPath;

		internal PHPickerProcessedFileResult(FileResult originalResult, ImageProcessingOptions options)
			: base()
		{
			_originalResult = originalResult;
			_options = options;

			// Copy metadata from original, adjusting extension for compressed output
			var originalFileName = originalResult.FileName;
			var originalFullPath = originalResult.FullPath;

			// Deterministic output container: PNG stays PNG, everything else becomes JPEG (matching the
			// shared Graphics processor). FileName/ContentType reflect the actual processed output.
			var outputExtension = ImageProcessor.GetOutputExtension(ImageProcessor.GetOutputFormat(originalFileName));
			var isPng = string.Equals(outputExtension, ".png", StringComparison.OrdinalIgnoreCase);

			FileName = !string.IsNullOrEmpty(originalFileName) && !string.IsNullOrEmpty(outputExtension)
				? Path.ChangeExtension(originalFileName, outputExtension)
				: originalFileName;

			FullPath = !string.IsNullOrEmpty(originalFullPath) && !string.IsNullOrEmpty(outputExtension)
				? Path.ChangeExtension(originalFullPath, outputExtension)
				: originalFullPath;

			ContentType = isPng ? "image/png" : "image/jpeg";
		}

		internal override async Task<Stream> PlatformOpenReadAsync()
		{
			// Return the already-processed file on subsequent calls to avoid re-processing.
			if (_processedPath is not null && File.Exists(_processedPath))
				return File.OpenRead(_processedPath);

			try
			{
				// Load the original once (NSItemProvider loads are expensive) and process it directly to
				// a cache file through the shared Graphics pipeline — no in-memory buffering of the
				// encoded image.
				using var originalStream = await _originalResult.OpenReadAsync();

				_processedPath = await ImageProcessor.ProcessImageToCacheFileAsync(
					originalStream,
					_originalResult.FileName,
					_options);

				return File.OpenRead(_processedPath);
			}
			catch
			{
				// Fall back to the original file if processing fails.
				return await _originalResult.OpenReadAsync();
			}
		}

		public void Dispose()
		{
			(_originalResult as IDisposable)?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
