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
				if (!pickExisting)
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
					CompletedHandler = async info =>
					{
						GetFileResult(info, tcs, options);
						await picker.DismissViewControllerAsync(true);
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

		static async Task<List<FileResult>> PickerResultsToMediaFiles(PHPickerResult[] results, MediaPickerOptions options = null)
		{
			// Handle empty or null results - return empty list instead of null
			if (results == null || results.Length == 0)
				return [];

			var fileResults = results
				.Select(file => (FileResult)new PHPickerFileResult(file.ItemProvider))
				.ToList();

			// Apply rotation if needed for images
			if (ImageProcessor.IsRotationNeeded(options))
			{
				var rotatedResults = new List<FileResult>();
				foreach (var result in fileResults)
				{
					try
					{
						using var originalStream = await result.OpenReadAsync();
						using var rotatedStream = await ImageProcessor.RotateImageAsync(originalStream, result.FileName);
						
						// Create a temp file for the rotated image
						var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(result.FileName)}";
						var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
						
						using (var fileStream = File.Create(tempFilePath))
						{
							rotatedStream.Position = 0;
							await rotatedStream.CopyToAsync(fileStream);
						}
						
						rotatedResults.Add(new FileResult(tempFilePath, result.FileName));
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
			if (ImageProcessor.IsProcessingNeeded(options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100))
			{
				var compressedResults = new List<FileResult>();
				foreach (var result in fileResults)
				{
					var compressedResult = await CompressedUIImageFileResult.CreateCompressedFromFileResult(result, options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100, options?.RotateImage ?? false, options?.PreserveMetaData ?? true);
					compressedResults.Add(compressedResult);
				}
				return compressedResults;
			}

			return fileResults;
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
				using var rotatedStream = await ImageProcessor.RotateImageAsync(originalStream, original.FileName);
				
				// Create a temp file for the rotated image
				var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(original.FileName)}";
				var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
				
				using (var fileStream = File.Create(tempFilePath))
				{
					rotatedStream.Position = 0;
					await rotatedStream.CopyToAsync(fileStream);
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
				picker.DismissViewController(true, null);
				CompletedHandler?.Invoke(info);
			}

			public override void Canceled(UIImagePickerController picker)
			{
				picker.DismissViewController(true, null);
				CompletedHandler?.Invoke(null);
			}
		}
	}

	class PhotoPickerDelegate : PHPickerViewControllerDelegate
	{
		public Action<PHPickerResult[]> CompletedHandler { get; set; }

		public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
		{
			picker.DismissViewController(true, null);
			CompletedHandler?.Invoke(results?.Length > 0 ? results : []);
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
			// 1. High quality (>=90) and no resizing needed (preserves original format)
			// 2. Original file was PNG
			// 3. Image might have transparency (PNG supports alpha channel)

			bool highQualityNoResize = compressionQuality >= 90 && !maximumWidth.HasValue && !maximumHeight.HasValue;
			bool originalWasPng = !string.IsNullOrEmpty(originalFileName) &&
									(originalFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
									 originalFileName.EndsWith(".PNG", StringComparison.OrdinalIgnoreCase));

			// For very high quality or when original was PNG, preserve PNG format
			return (compressionQuality >= 95 && !maximumWidth.HasValue && !maximumHeight.HasValue) || originalWasPng;
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
}
