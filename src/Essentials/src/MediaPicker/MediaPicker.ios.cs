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
				return await tcs.Task;
			}
			finally
			{
				pickerRef?.Dispose();
			}
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
			UIViewController pickerRef = null;

			PHPickerFileResult.CleanupTemporaryFiles();

			if (pickExisting)
			{
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
				return [];

			var fileResults = new List<FileResult>(results.Length);
			try
			{
				foreach (var file in results)
				{
					var fileResult = new PHPickerFileResult(file.ItemProvider);
					await fileResult.LoadFileRepresentationAsync().ConfigureAwait(false);
					fileResults.Add(fileResult);
				}
			}
			catch
			{
				DisposeFileResults(fileResults);
				throw;
			}

			var processingNeeded = ImageProcessor.IsProcessingNeeded(options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100);

			// Apply standalone rotation only when no later processing pass will do it from the original file.
			if (ImageProcessor.IsRotationNeeded(options) && !processingNeeded)
			{
				var rotatedResults = new List<FileResult>();
				foreach (var result in fileResults)
				{
					if (!IsImageFile(result.FileName))
					{
						rotatedResults.Add(result);
						continue;
					}

					try
					{
						using var originalStream = await result.OpenReadAsync();
						using var rotatedStream = await ImageProcessor.RotateImageAsync(originalStream, result.FileName);

						var tempFilePath = PHPickerFileResult.CreateTemporaryFilePath(Path.GetExtension(result.FileName));

						using (var fileStream = File.Create(tempFilePath))
						{
							rotatedStream.Position = 0;
							await rotatedStream.CopyToAsync(fileStream);
						}

						var rotatedResult = new FileResult(tempFilePath)
						{
							FileName = result.FileName,
							ContentType = result.ContentType
						};
						rotatedResults.Add(rotatedResult);
						(result as IDisposable)?.Dispose();
					}
					catch
					{
						// If rotation fails, use the original file
						rotatedResults.Add(result);
					}
				}
				fileResults = rotatedResults;
			}

			// Apply resizing and compression if specified and dealing with images
			if (processingNeeded)
			{
				var compressedResults = new List<FileResult>();
				PHPickerProcessedFileResult compressedResult = null;
				try
				{
					foreach (var result in fileResults)
					{
						if (!IsImageFile(result.FileName))
						{
							compressedResults.Add(result);
							continue;
						}

						compressedResult = new PHPickerProcessedFileResult(result, options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100, options?.RotateImage ?? false, options?.PreserveMetaData ?? true);
						await compressedResult.LoadProcessedFileAsync().ConfigureAwait(false);
						compressedResults.Add(compressedResult);
						compressedResult = null;
					}
				}
				catch
				{
					compressedResult?.Dispose();
					DisposeFileResults(compressedResults);
					DisposeFileResults(fileResults);
					throw;
				}
				return compressedResults;
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
					// Apply rotation if needed for the UIImage
					if (ImageProcessor.IsRotationNeeded(options) && img.Orientation != UIImageOrientation.Up)
					{
						img = img.NormalizeOrientation();
					}

					return new CompressedUIImageFileResult(img, null, options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100);
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
				using var originalStream = await original.OpenReadAsync().ConfigureAwait(false);
				using var rotatedStream = await ImageProcessor.RotateImageAsync(originalStream, original.FileName).ConfigureAwait(false);

				var tempFilePath = PHPickerFileResult.CreateTemporaryFilePath(Path.GetExtension(original.FileName));

				using (var fileStream = File.Create(tempFilePath))
				{
					rotatedStream.Position = 0;
					await rotatedStream.CopyToAsync(fileStream).ConfigureAwait(false);
				}

				return new FileResult(tempFilePath, original.FileName);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error rotating image: {ex.Message}");
				return original;
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
		readonly int? maximumWidth;
		readonly int? maximumHeight;
		readonly int compressionQuality;
		readonly string originalFileName;
		NSData data;

		// Static factory method to create compressed result from existing FileResult
		internal static async Task<FileResult> CreateCompressedFromFileResult(FileResult originalResult, int? maximumWidth, int? maximumHeight, int compressionQuality = 100, bool rotateImage = false, bool preserveMetaData = true)
		{
			if (originalResult is null || !ImageProcessor.IsProcessingNeeded(maximumWidth, maximumHeight, compressionQuality))
				return originalResult;

			try
			{
				using var originalStream = await originalResult.OpenReadAsync();
				using var processedStream = await ImageProcessor.ProcessImageAsync(
					originalStream, maximumWidth, maximumHeight, compressionQuality, originalResult.FileName, rotateImage, preserveMetaData);

				// If ImageProcessor returns null (e.g., on .NET Standard), return original file
				if (processedStream is null)
				{
					return originalResult;
				}

				// Read processed stream into memory
				var memoryStream = new MemoryStream();
				await processedStream.CopyToAsync(memoryStream);
				memoryStream.Position = 0;

				return new ProcessedImageFileResult(memoryStream, originalResult.FileName);
			}
			catch
			{
				// If compression fails, return original
			}

			return originalResult;
		}

		internal CompressedUIImageFileResult(UIImage image, string originalFileName = null, int? maximumWidth = null, int? maximumHeight = null, int compressionQuality = 100)
			: base()
		{
			uiImage = image;
			this.originalFileName = originalFileName;
			this.maximumWidth = maximumWidth;
			this.maximumHeight = maximumHeight;
			this.compressionQuality = Math.Max(0, Math.Min(100, compressionQuality));

			// Determine output format: preserve PNG when appropriate, otherwise use JPEG
			var extension = ShouldUsePngFormat() ? FileExtensions.Png : FileExtensions.Jpg;
			FullPath = Guid.NewGuid().ToString() + extension;
			FileName = FullPath;
		}

		bool ShouldUsePngFormat()
		{
			// Use PNG if:
			// 1. Original file was PNG
			// 2. High quality (>=90) and no resizing needed (preserves original format)

			bool originalWasPng = !string.IsNullOrEmpty(originalFileName) &&
									Path.GetExtension(originalFileName).Equals(".png", StringComparison.OrdinalIgnoreCase);

			return originalWasPng || (compressionQuality >= 90 && !maximumWidth.HasValue && !maximumHeight.HasValue);
		}

		internal override Task<Stream> PlatformOpenReadAsync()
		{
			if (data == null)
			{
				var normalizedImage = uiImage.NormalizeOrientation();

				// First, apply resizing if needed
				var workingImage = normalizedImage;
				var originalSize = normalizedImage.Size;
				var newSize = CalculateResizedDimensions(originalSize.Width, originalSize.Height, maximumWidth, maximumHeight);

				if (newSize.Width != originalSize.Width || newSize.Height != originalSize.Height)
				{
					// Resize the image
					UIGraphics.BeginImageContextWithOptions(newSize, false, normalizedImage.CurrentScale);
					normalizedImage.Draw(new CoreGraphics.CGRect(CoreGraphics.CGPoint.Empty, newSize));
					workingImage = UIGraphics.GetImageFromCurrentImageContext();
					UIGraphics.EndImageContext();
				}

				// Then determine output format and apply compression
				bool usePng = ShouldUsePngFormat();

				if (usePng)
				{
					// Use PNG format - lossless compression, supports transparency
					data = workingImage.AsPNG();
				}
				else
				{
					// Use JPEG with quality-based compression
					if (compressionQuality < 90)
					{
						// Use JPEG compression with quality setting for aggressive compression
						var qualityFloat = compressionQuality / 100.0f;
						data = workingImage.AsJPEG(qualityFloat);
					}
					else if (compressionQuality < 100)
					{
						// Use JPEG with high quality
						data = workingImage.AsJPEG(0.9f);
					}
					else
					{
						// Use JPEG with maximum quality
						data = workingImage.AsJPEG(0.95f);
					}
				}
			}

			return Task.FromResult(data.AsStream());
		}

		static CoreGraphics.CGSize CalculateResizedDimensions(nfloat originalWidth, nfloat originalHeight, int? maxWidth, int? maxHeight)
		{
			if (!maxWidth.HasValue && !maxHeight.HasValue)
				return new CoreGraphics.CGSize(originalWidth, originalHeight);

			nfloat scaleWidth = maxWidth.HasValue ? (nfloat)maxWidth.Value / originalWidth : nfloat.MaxValue;
			nfloat scaleHeight = maxHeight.HasValue ? (nfloat)maxHeight.Value / originalHeight : nfloat.MaxValue;

			// Use the smaller scale to ensure both constraints are respected
			nfloat scale = (nfloat)Math.Min(Math.Min((double)scaleWidth, (double)scaleHeight), 1.0); // Don't scale up

			return new CoreGraphics.CGSize(originalWidth * scale, originalHeight * scale);
		}
	}

	/// <summary>
	/// FileResult implementation for processed images using MAUI Graphics
	/// </summary>
	internal class ProcessedImageFileResult : FileResult, IDisposable
	{
		readonly MemoryStream imageData;
		readonly string originalFileName;

		internal ProcessedImageFileResult(MemoryStream imageData, string originalFileName = null)
			: base()
		{
			this.imageData = imageData;
			this.originalFileName = originalFileName;

			// Determine output format extension using ImageProcessor's improved logic
			var extension = ImageProcessor.DetermineOutputExtension(imageData, 75, originalFileName);
			FullPath = Guid.NewGuid().ToString() + extension;
			FileName = FullPath;
		}

		internal override Task<Stream> PlatformOpenReadAsync()
		{
			// Reset position and return a copy of the stream
			imageData.Position = 0;
			var copyStream = new MemoryStream(imageData.ToArray());
			return Task.FromResult<Stream>(copyStream);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				imageData?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}

	class PHPickerProcessedFileResult : FileResult, IDisposable
	{
		readonly FileResult _originalResult;
		readonly int? _maximumWidth;
		readonly int? _maximumHeight;
		readonly int _compressionQuality;
		readonly bool _rotateImage;
		readonly bool _preserveMetaData;
		readonly object _processLock = new();
		Task _processFileTask;
		bool _isProcessed;
		bool _disposed;

		internal PHPickerProcessedFileResult(FileResult originalResult, int? maximumWidth, int? maximumHeight, int compressionQuality, bool rotateImage, bool preserveMetaData)
			: base()
		{
			_originalResult = originalResult;
			_maximumWidth = maximumWidth;
			_maximumHeight = maximumHeight;
			_compressionQuality = compressionQuality;
			_rotateImage = rotateImage;
			_preserveMetaData = preserveMetaData;

			FileName = originalResult.FileName;
			FullPath = originalResult.FullPath;
			ContentType = originalResult.ContentType;
		}

		internal override async Task<Stream> PlatformOpenReadAsync()
		{
			await LoadProcessedFileAsync().ConfigureAwait(false);

			return File.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

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
			byte[] originalData;
			using (var originalStream = await _originalResult.OpenReadAsync())
			using (var buffer = new MemoryStream())
			{
				await originalStream.CopyToAsync(buffer);
				originalData = buffer.ToArray();
			}

			Stream processedStream = null;
			try
			{
				// processedStream is always an independent MemoryStream from ImageProcessor.ProcessImageAsync;
				// it does not reference or depend on originalStream after this call completes.
				using var originalForProcessing = new MemoryStream(originalData, writable: false);
				processedStream = await ImageProcessor.ProcessImageAsync(
					originalForProcessing,
					_maximumWidth,
					_maximumHeight,
					_compressionQuality,
					_originalResult.FileName,
					_rotateImage,
					_preserveMetaData);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Unable to process picked image '{_originalResult.FileName}': {ex}");
				processedStream = null;
			}

			if (processedStream is null)
			{
				lock (_processLock)
				{
					ThrowIfDisposedNoLock();
					_isProcessed = true;
				}

				return;
			}

			using (processedStream)
			{
				var outputExtension = ImageProcessor.DetermineOutputExtension(processedStream, _compressionQuality, _originalResult.FileName);
				var processedFileName = !string.IsNullOrEmpty(_originalResult.FileName) && !string.IsNullOrEmpty(outputExtension)
					? Path.ChangeExtension(_originalResult.FileName, outputExtension)
					: _originalResult.FileName;

				var processedContentType = string.Equals(outputExtension, ".png", StringComparison.OrdinalIgnoreCase)
					? "image/png"
					: string.Equals(outputExtension, ".jpg", StringComparison.OrdinalIgnoreCase) || string.Equals(outputExtension, ".jpeg", StringComparison.OrdinalIgnoreCase)
						? "image/jpeg"
						: _originalResult.ContentType;

				var processedPath = PHPickerFileResult.CreateTemporaryFilePath(outputExtension);

				try
				{
					if (processedStream.CanSeek)
					{
						processedStream.Position = 0;
					}

					using (var fileStream = File.Create(processedPath))
					{
						await processedStream.CopyToAsync(fileStream);
					}

					File.SetLastWriteTimeUtc(processedPath, DateTime.UtcNow);

					lock (_processLock)
					{
						if (_disposed)
						{
							PHPickerFileResult.TryDeleteTemporaryFile(processedPath);
							throw new ObjectDisposedException(nameof(PHPickerProcessedFileResult));
						}

						FileName = processedFileName;
						ContentType = processedContentType;
						FullPath = processedPath;
						_isProcessed = true;
					}

					PHPickerFileResult.TryDeleteTemporaryFile(_originalResult.FullPath);
					(_originalResult as IDisposable)?.Dispose();
				}
				catch
				{
					PHPickerFileResult.TryDeleteTemporaryFile(processedPath);
					throw;
				}
			}
		}

		public void Dispose()
		{
			string fullPath;

			lock (_processLock)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;
				fullPath = FullPath;
			}

			PHPickerFileResult.TryDeleteTemporaryFile(fullPath);
			PHPickerFileResult.TryDeleteTemporaryFile(_originalResult.FullPath);
			(_originalResult as IDisposable)?.Dispose();
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
