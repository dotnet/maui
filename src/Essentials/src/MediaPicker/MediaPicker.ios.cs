using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public bool IsCaptureSupported
			=> UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> PhotoAsync(options, true, true);

		public Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions options)
			=> PhotosAsync(options, true);

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
			=> PhotosAsync(options, false);

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
			UIViewController pickerRef = null;

			PHPickerFileResult.CleanupTemporaryFiles();

			if (pickExisting && OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				var config = new PHPickerConfiguration
				{
					Filter = photo
						? PHPickerFilter.ImagesFilter
						: PHPickerFilter.VideosFilter
				};

				if (!photo)
				{
					config.PreferredAssetRepresentationMode = PHPickerConfigurationAssetRepresentationMode.Compatible;
				}

				var picker = new PHPickerViewController(config)
				{
					Delegate = new Media.PhotoPickerDelegate
					{
						CompletedHandler = res =>
							_ = CompletePickerResultAsync(res, options, tcs)
					}
				};

				pickerRef = picker;
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

				pickerRef = picker;

				picker.Delegate = new PhotoPickerDelegate
				{
					CompletedHandler = info =>
					{
						_ = CompleteUIImagePickerResultAsync(info, options, tcs);
					}
				};
			}

			if (!string.IsNullOrWhiteSpace(options?.Title))
			{
				pickerRef.Title = options.Title;
			}

			if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
			{
				pickerRef.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
			}

			pickerRef.PresentationController?.Delegate = new PhotoPickerPresentationControllerDelegate
			{
				Handler = () => tcs.TrySetResult(null)
			};

			try
			{
				await vc.PresentViewControllerAsync(pickerRef, true);
				var result = await tcs.Task;

				if (!pickExisting && result is not null && options?.SaveToGallery == true)
				{
					await SaveToPhotoLibraryAsync(result);
				}

				return result;
			}
			finally
			{
				pickerRef?.Dispose();
			}
		}

		async Task<List<FileResult>> PhotosAsync(MediaPickerOptions options, bool photo)
		{
			// iOS 14+ only supports multiple selection
			// TODO throw exception?
			if (!OperatingSystem.IsIOSVersionAtLeast(14, 0))
			{
				return [];
			}

			var vc = WindowStateManager.Default.GetCurrentUIViewController(true);
			var tcs = new TaskCompletionSource<List<FileResult>>();
			UIViewController pickerRef = null;

			PHPickerFileResult.CleanupTemporaryFiles();

			var config = new PHPickerConfiguration
			{
				Filter = photo
					? PHPickerFilter.ImagesFilter
					: PHPickerFilter.VideosFilter,
				SelectionLimit = options?.SelectionLimit ?? 1
			};

			if (!photo)
			{
				config.PreferredAssetRepresentationMode = PHPickerConfigurationAssetRepresentationMode.Compatible;
			}

			var picker = new PHPickerViewController(config)
			{
				Delegate = new Media.PhotoPickerDelegate
				{
					CompletedHandler = res =>
						_ = CompletePickerResultsAsync(res, options, tcs)
				}
			};

			pickerRef = picker;

			if (!string.IsNullOrWhiteSpace(options?.Title))
			{
				pickerRef.Title = options.Title;
			}

			if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
			{
				pickerRef.ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
			}

			pickerRef.PresentationController?.Delegate = new PhotoPickerPresentationControllerDelegate
			{
				Handler = () => tcs.TrySetResult([])
			};

			try
			{
				await vc.PresentViewControllerAsync(pickerRef, true);
				return await tcs.Task;
			}
			finally
			{
				pickerRef?.Dispose();
			}
		}

		static async Task CompletePickerResultAsync(PHPickerResult[] results, MediaPickerOptions options, TaskCompletionSource<FileResult> tcs)
		{
			try
			{
				var result = await PickerResultsToMediaFiles(results, options).ConfigureAwait(false);
				tcs.TrySetResult(result.FirstOrDefault());
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		static async Task CompletePickerResultsAsync(PHPickerResult[] results, MediaPickerOptions options, TaskCompletionSource<List<FileResult>> tcs)
		{
			try
			{
				var result = await PickerResultsToMediaFiles(results, options).ConfigureAwait(false);
				tcs.TrySetResult(result);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		static async Task<List<FileResult>> PickerResultsToMediaFiles(PHPickerResult[] results, MediaPickerOptions options = null)
		{
			// Handle null or empty results (cancellation) - return empty list per API contract
			if (results == null || results.Length == 0)
				return new List<FileResult>();

			// Rotation, resizing and recompression are all handled together by a single Graphics-based
			// pass (see PHPickerProcessedFileResult).
			var needsProcessing = ImageProcessor.IsProcessingNeeded(options);
			var processingOptions = new ImageProcessingOptions(
				options?.MaximumWidth,
				options?.MaximumHeight,
				options?.CompressionQuality ?? 100,
				options?.RotateImage ?? false,
				options?.PreserveMetaData ?? true);

			var fileResults = new List<FileResult>(results.Length);

			// Tracks the result being built but not yet handed to the list, so it (and anything it owns)
			// can be disposed if a later await throws mid-construction.
			FileResult pending = null;
			try
			{
				foreach (var file in results)
				{
					// Materialize the picked item to a real file up-front so FullPath is immediately usable for
					// direct filesystem access (for example File.Copy(result.FullPath, ...)) — see #32832.
					var pickedFile = new PHPickerFileResult(file.ItemProvider);
					pending = pickedFile;
					await pickedFile.LoadFileRepresentationAsync().ConfigureAwait(false);

					// Only images are processed; videos and other files pass through untouched. Processing is
					// performed eagerly (and takes ownership of the picked file) so the processed result also
					// exposes a ready-to-read FullPath.
					if (needsProcessing && IsImageFile(pickedFile.FileName))
					{
						var processed = new PHPickerProcessedFileResult(pickedFile, processingOptions);
						pending = processed;
						await processed.LoadProcessedFileAsync().ConfigureAwait(false);
					}

					fileResults.Add(pending);
					pending = null;
				}
			}
			catch
			{
				(pending as IDisposable)?.Dispose();
				DisposeFileResults(fileResults);
				throw;
			}

			return fileResults;
		}

		static void DisposeFileResults(IEnumerable<FileResult> fileResults)
		{
			foreach (var fileResult in fileResults)
			{
				PHPickerFileResult.TryDeleteTemporaryFile(fileResult.FullPath);
				(fileResult as IDisposable)?.Dispose();
			}
		}

		static async Task CompleteUIImagePickerResultAsync(NSDictionary info, MediaPickerOptions options, TaskCompletionSource<FileResult> tcs)
		{
			try
			{
				var result = await DictionaryToMediaFile(info, options).ConfigureAwait(false);
				tcs.TrySetResult(result);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		static async Task<FileResult> DictionaryToMediaFile(NSDictionary info, MediaPickerOptions options = null)
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
								var rotatedResult = await RotateImageFile(docResult).ConfigureAwait(false);
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
					var rotatedResult = await RotateImageFile(assetResult).ConfigureAwait(false);
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
				if (picker.PresentationController?.Delegate is PhotoPickerPresentationControllerDelegate pd)
				{
					pd.Handler = null;
				}

				picker.DismissViewController(true, () => CompletedHandler?.Invoke(info));
			}

			public override void Canceled(UIImagePickerController picker)
			{
				if (picker.PresentationController?.Delegate is PhotoPickerPresentationControllerDelegate pd)
				{
					pd.Handler = null;
				}

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

	class PHPickerFileResult : FileResult, IDisposable
	{
		const string TemporaryDirectoryName = "maui-mediapicker";
		static readonly TimeSpan TemporaryFileRetention = TimeSpan.FromDays(1);

		readonly string _identifier;
		readonly NSItemProvider _provider;
		readonly object _loadLock = new();
		Task _loadFileTask;
		TaskCompletionSource<bool> _loadTcs;
		NSProgress _loadProgress;
		bool _isFileLoaded;
		bool _disposed;

		static string TemporaryDirectory =>
			Path.Combine(Path.GetTempPath(), TemporaryDirectoryName);

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

			var extension = GetFileExtension(_identifier);
			FileName = GetFileName(provider?.SuggestedName, extension);
			FullPath = CreateTemporaryFilePath(Path.GetExtension(FileName));
		}

		internal override async Task<Stream> PlatformOpenReadAsync()
		{
			await LoadFileRepresentationAsync().ConfigureAwait(false);

			return File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		internal Task LoadFileRepresentationAsync()
		{
			ValidateFileRepresentation();

			TaskCompletionSource<bool> loadTcs = null;
			Task loadTask;

			lock (_loadLock)
			{
				ThrowIfDisposedNoLock();

				if (_isFileLoaded)
				{
					return Task.CompletedTask;
				}

				if (_loadFileTask is null)
				{
					loadTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
					_loadTcs = loadTcs;
					_loadFileTask = loadTcs.Task;
				}

				loadTask = _loadFileTask;
			}

			if (loadTcs is not null)
			{
				StartLoadFileRepresentation(loadTcs);
			}

			return AwaitLoadFileRepresentationAsync(loadTask);
		}

		internal static void CleanupTemporaryFiles()
		{
			var temporaryDirectory = TemporaryDirectory;

			if (!Directory.Exists(temporaryDirectory))
			{
				return;
			}

			var cutoff = DateTime.UtcNow - TemporaryFileRetention;

			try
			{
				foreach (var file in Directory.EnumerateFiles(temporaryDirectory))
				{
					DeleteTemporaryFileIfStale(file, cutoff);
				}
			}
			catch (IOException ex)
			{
				Debug.WriteLine($"Unable to enumerate MediaPicker temporary files: {ex}");
			}
			catch (UnauthorizedAccessException ex)
			{
				Debug.WriteLine($"Unable to enumerate MediaPicker temporary files: {ex}");
			}
		}

		async Task AwaitLoadFileRepresentationAsync(Task loadTask)
		{
			try
			{
				await loadTask.ConfigureAwait(false);
			}
			catch
			{
				lock (_loadLock)
				{
					if (ReferenceEquals(_loadFileTask, loadTask))
					{
						_loadFileTask = null;
					}
				}

				throw;
			}
		}

		void ValidateFileRepresentation()
		{
			if (_provider is null)
			{
				throw new InvalidOperationException("Item provider is null.");
			}

			if (string.IsNullOrWhiteSpace(_identifier))
			{
				throw new InvalidOperationException("Item provider does not contain a supported file representation.");
			}

			if (string.IsNullOrWhiteSpace(FullPath))
			{
				throw new InvalidOperationException("Destination file path is not set.");
			}
		}

		void StartLoadFileRepresentation(TaskCompletionSource<bool> tcs)
		{
			var destinationPath = FullPath;

			try
			{
				var progress = _provider.LoadFileRepresentation(_identifier, (url, error) =>
				{
					try
					{
						if (error is not null)
						{
							ClearLoadOperation(tcs);
							tcs.TrySetException(new NSErrorException(error));
							return;
						}

						if (string.IsNullOrWhiteSpace(url?.Path))
						{
							ClearLoadOperation(tcs);
							tcs.TrySetException(new InvalidOperationException("Item provider did not return a file URL."));
							return;
						}

						ThrowIfDisposed();
						CopyTemporaryFile(url.Path, destinationPath);

						lock (_loadLock)
						{
							ClearLoadOperationNoLock(tcs);

							if (_disposed)
							{
								TryDeleteTemporaryFile(destinationPath);
								tcs.TrySetException(new ObjectDisposedException(nameof(PHPickerFileResult)));
								return;
							}

							_isFileLoaded = true;
						}

						tcs.TrySetResult(true);
					}
					catch (Exception ex)
					{
						ClearLoadOperation(tcs);
						tcs.TrySetException(ex);
					}
				});

				lock (_loadLock)
				{
					if (_disposed)
					{
						progress?.Cancel();
					}
					else if (!_isFileLoaded && ReferenceEquals(_loadTcs, tcs) && !tcs.Task.IsCompleted)
					{
						_loadProgress = progress;
					}
				}
			}
			catch (Exception ex)
			{
				ClearLoadOperation(tcs);
				tcs.TrySetException(ex);
			}
		}

		void ClearLoadOperation(TaskCompletionSource<bool> tcs)
		{
			lock (_loadLock)
			{
				ClearLoadOperationNoLock(tcs);
			}
		}

		void ClearLoadOperationNoLock(TaskCompletionSource<bool> tcs)
		{
			if (ReferenceEquals(_loadTcs, tcs))
			{
				_loadTcs = null;
				_loadProgress = null;
			}
		}

		void ThrowIfDisposed()
		{
			lock (_loadLock)
			{
				ThrowIfDisposedNoLock();
			}
		}

		void ThrowIfDisposedNoLock()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(PHPickerFileResult));
			}
		}

		internal static string CreateTemporaryFilePath(string extension)
		{
			Directory.CreateDirectory(TemporaryDirectory);

			return Path.Combine(TemporaryDirectory, $"{Guid.NewGuid():N}{NormalizeExtension(extension)}");
		}

		static void CopyTemporaryFile(string sourcePath, string destinationPath)
		{
			try
			{
				File.Copy(sourcePath, destinationPath, overwrite: true);
				File.SetLastWriteTimeUtc(destinationPath, DateTime.UtcNow);
			}
			catch
			{
				TryDeleteTemporaryFile(destinationPath);
				throw;
			}
		}

		static void DeleteTemporaryFileIfStale(string path, DateTime cutoff)
		{
			try
			{
				if (File.GetLastWriteTimeUtc(path) < cutoff)
				{
					TryDeleteTemporaryFile(path);
				}
			}
			catch (IOException ex)
			{
				Debug.WriteLine($"Unable to inspect MediaPicker temporary file '{path}': {ex}");
			}
			catch (UnauthorizedAccessException ex)
			{
				Debug.WriteLine($"Unable to inspect MediaPicker temporary file '{path}': {ex}");
			}
		}

		internal static void TryDeleteTemporaryFile(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !IsTemporaryFile(path))
			{
				return;
			}

			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (IOException ex)
			{
				Debug.WriteLine($"Unable to delete MediaPicker temporary file '{path}': {ex}");
			}
			catch (UnauthorizedAccessException ex)
			{
				Debug.WriteLine($"Unable to delete MediaPicker temporary file '{path}': {ex}");
			}
		}

		static bool IsTemporaryFile(string path)
		{
			var fullDirectory = Path.GetFullPath(TemporaryDirectory + Path.DirectorySeparatorChar);
			var fullPath = Path.GetFullPath(path);

			return fullPath.StartsWith(fullDirectory, StringComparison.Ordinal);
		}

		static string NormalizeExtension(string extension)
		{
			return string.IsNullOrWhiteSpace(extension)
				? string.Empty
				: $".{extension.TrimStart('.')}";
		}

		static string GetFileExtension(string identifier)
		{
			var extension = GetTag(identifier, UTType.TagClassFilenameExtension);

			return NormalizeExtension(extension);
		}

		static string GetFileName(string suggestedName, string extension)
		{
			var fileName = string.IsNullOrWhiteSpace(suggestedName)
				? Guid.NewGuid().ToString("N")
				: Path.GetFileName(suggestedName);

			if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)) && !string.IsNullOrWhiteSpace(extension))
			{
				fileName += extension;
			}

			return fileName;
		}

		protected internal static string GetTag(string identifier, string tagClass)
			   => UTType.CopyAllTags(identifier, tagClass)?.FirstOrDefault();

		public void Dispose()
		{
			TaskCompletionSource<bool> loadTcs;
			NSProgress loadProgress;

			lock (_loadLock)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;
				loadTcs = _loadTcs;
				_loadTcs = null;
				loadProgress = _loadProgress;
				_loadProgress = null;
			}

			loadProgress?.Cancel();
			loadTcs?.TrySetException(new ObjectDisposedException(nameof(PHPickerFileResult)));
			TryDeleteTemporaryFile(FullPath);
		}
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
			var extension = ImageProcessor.GetOutputExtension(format, originalFileName);
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

	class PHPickerProcessedFileResult : FileResult, IDisposable
	{
		readonly FileResult _originalResult;
		readonly ImageProcessingOptions _options;
		readonly object _processLock = new();
		Task _processFileTask;
		bool _isProcessed;
		bool _disposed;

		// Path to the processed cache file once produced; used for cleanup. Null until (and unless)
		// processing succeeds — before then FullPath still points at the materialized original.
		string _processedCachePath;

		internal PHPickerProcessedFileResult(FileResult originalResult, ImageProcessingOptions options)
			: base()
		{
			_originalResult = originalResult;
			_options = options;

			// Until processing runs, mirror the materialized original so FullPath is already usable; the
			// processed output (with any corrected extension) replaces these once produced.
			FileName = originalResult.FileName;
			FullPath = originalResult.FullPath;
			ContentType = originalResult.ContentType;
		}

		internal override async Task<Stream> PlatformOpenReadAsync()
		{
			await LoadProcessedFileAsync().ConfigureAwait(false);

			return File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		// Processes the picked image exactly once (idempotent and thread-safe) and materializes it to a
		// real cache file so FullPath points to a readable file even before the stream is opened.
		internal Task LoadProcessedFileAsync()
		{
			Task processTask;

			lock (_processLock)
			{
				ThrowIfDisposedNoLock();

				if (_isProcessed)
				{
					return Task.CompletedTask;
				}

				_processFileTask ??= ProcessAndWriteFileAsync();
				processTask = _processFileTask;
			}

			return AwaitProcessedFileAsync(processTask);
		}

		async Task AwaitProcessedFileAsync(Task processTask)
		{
			try
			{
				await processTask.ConfigureAwait(false);
			}
			catch
			{
				lock (_processLock)
				{
					if (ReferenceEquals(_processFileTask, processTask))
					{
						_processFileTask = null;
					}
				}

				throw;
			}
		}

		async Task ProcessAndWriteFileAsync()
		{
			string processedPath;
			try
			{
				// Load the original once (NSItemProvider loads are expensive) and process it straight to a
				// cache file through the shared Graphics pipeline — no in-memory buffering of the encoded image.
				using var originalStream = await _originalResult.OpenReadAsync().ConfigureAwait(false);

				processedPath = await ImageProcessor.ProcessImageToCacheFileAsync(
					originalStream,
					_originalResult.FileName,
					_options).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				// Processing failed (for example an undecodable image): fall back to the materialized
				// original, which FileName/FullPath/ContentType already point at.
				Debug.WriteLine($"Unable to process picked image '{_originalResult.FileName}': {ex}");
				lock (_processLock)
				{
					ThrowIfDisposedNoLock();
					_isProcessed = true;
				}

				return;
			}

			var processedContentType = ImageProcessor.GetOutputFormat(processedPath) == Microsoft.Maui.Graphics.ImageFormat.Png
				? "image/png"
				: "image/jpeg";

			lock (_processLock)
			{
				if (_disposed)
				{
					TryDeleteProcessedCacheFile(processedPath);
					throw new ObjectDisposedException(nameof(PHPickerProcessedFileResult));
				}

				FileName = Path.GetFileName(processedPath);
				FullPath = processedPath;
				ContentType = processedContentType;
				_processedCachePath = processedPath;
				_isProcessed = true;
			}

			// The processed file supersedes the picked original; reclaim the original temp file.
			PHPickerFileResult.TryDeleteTemporaryFile(_originalResult.FullPath);
			(_originalResult as IDisposable)?.Dispose();
		}

		public void Dispose()
		{
			string processedCachePath;

			lock (_processLock)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;
				processedCachePath = _processedCachePath;
			}

			// Only reclaim a path we produced; the original picked file is swept from the temp folder.
			TryDeleteProcessedCacheFile(processedCachePath);
			PHPickerFileResult.TryDeleteTemporaryFile(_originalResult.FullPath);
			(_originalResult as IDisposable)?.Dispose();
		}

		// The processed file lives in its own unique sub-directory of the app cache (not the MediaPicker
		// temp folder), so it is removed directly rather than via PHPickerFileResult.TryDeleteTemporaryFile.
		static void TryDeleteProcessedCacheFile(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return;
			}

			try
			{
				var directory = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
				{
					Directory.Delete(directory, recursive: true);
				}
				else if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
				// Best-effort cleanup.
			}
		}

		void ThrowIfDisposedNoLock()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(PHPickerProcessedFileResult));
			}
		}
	}
}
