// Using code based on https://github.com/richardrigutins/maui-sample-windows-camera-workaround/blob/4e8ab1eb2fe36e9d8253773705df508b7979a968/src/MauiSampleCamera/Platforms/Windows/CustomMediaPicker.uwp.cs
// Authors:
//  - https://github.com/GiampaoloGabba
//  - https://github.com/richardrigutins

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Storage;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;
using WinLauncher = Windows.System.Launcher;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{
		public bool IsCaptureSupported
			=> true;

		[Obsolete("Switch to PickPhotoAsync which also allows multiple selections.")]
		public Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null)
			=> PickAsync(options, true);

		public Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions? options = null)
			=> PickMultipleAsync(options, true);

		[Obsolete("Switch to PickVideosAsync which also allows multiple selections.")]
		public Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null)
			=> PickAsync(options, false);

		public Task<List<FileResult>> PickVideosAsync(MediaPickerOptions? options = null)
			=> PickMultipleAsync(options, false);

		public async Task<FileResult?> PickAsync(MediaPickerOptions? options, bool photo)
		{
			var picker = new FileOpenPicker();

			var hwnd = WindowStateManager.Default.GetActiveWindowHandle(true);
			WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

			var defaultTypes = photo ? FilePickerFileType.Images.Value : FilePickerFileType.Videos.Value;

			// set picker properties
			foreach (var filter in defaultTypes.Select(t => t.TrimStart('*')))
				picker.FileTypeFilter.Add(filter);
			picker.SuggestedStartLocation = photo ? PickerLocationId.PicturesLibrary : PickerLocationId.VideosLibrary;
			picker.ViewMode = PickerViewMode.Thumbnail;

			// show the picker
			var result = await picker.PickSingleFileAsync();

			// cancelled
			if (result is null)
			{
				return null;
			}

			var fileResult = new FileResult(result);

			// Apply rotation if needed for photos
			if (photo && ImageProcessor.IsRotationNeeded(options) && result != null)
			{
				try
				{
					using var originalStream = await result.OpenStreamForReadAsync();
					using var rotatedStream = await ImageProcessor.RotateImageAsync(originalStream, result.Name);

					// Save rotated image to temporary file
					var tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(result.Name)}");
					using (var fileStream = File.Create(tempFileName))
					{
						rotatedStream.Position = 0;
						await rotatedStream.CopyToAsync(fileStream);
					}

					fileResult = new FileResult(tempFileName);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to rotate image: {ex.Message}");
					// If rotation fails, continue with the original file
				}
			}

			// Apply compression/resizing if specified for photos
			if (photo && ImageProcessor.IsProcessingNeeded(options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100))
			{
				using var originalStream = await result.OpenStreamForReadAsync();
				var processedStream = await ImageProcessor.ProcessImageAsync(
					originalStream,
					options?.MaximumWidth,
					options?.MaximumHeight,
					options?.CompressionQuality ?? 100,
					result!.Name,
					options?.RotateImage ?? false,
					options?.PreserveMetaData ?? true);

				if (processedStream != null)
				{
					// Convert to MemoryStream for ProcessedImageFileResult
					var memoryStream = new MemoryStream();
					await processedStream.CopyToAsync(memoryStream);
					processedStream.Dispose();

					return new ProcessedImageFileResult(memoryStream, result.Name);
				}
			}

			return fileResult;
		}

		public async Task<List<FileResult>> PickMultipleAsync(MediaPickerOptions? options, bool photo)
		{
			if (options?.SelectionLimit == 1)
			{
				var singleResult = await PickAsync(options, photo);
				return singleResult is null ? [] : [singleResult];
			}

			var picker = new FileOpenPicker();

			var hwnd = WindowStateManager.Default.GetActiveWindowHandle(true);
			InitializeWithWindow.Initialize(picker, hwnd);

			var defaultTypes = photo ? FilePickerFileType.Images.Value : FilePickerFileType.Videos.Value;

			// set picker properties
			foreach (var filter in defaultTypes.Select(t => t.TrimStart('*')))
			{
				picker.FileTypeFilter.Add(filter);
			}

			picker.SuggestedStartLocation = photo ? PickerLocationId.PicturesLibrary : PickerLocationId.VideosLibrary;
			picker.ViewMode = PickerViewMode.Thumbnail;

			// show the picker
			var result = await picker.PickMultipleFilesAsync();

			// cancelled
			if (result is null)
			{
				return [];
			}

			var fileResults = result.Select(file => new FileResult(file)).ToList();

			// Apply rotation if needed for photos
			if (photo && ImageProcessor.IsRotationNeeded(options))
			{
				var rotatedResults = new List<FileResult>();
				for (int i = 0; i < result.Count; i++)
				{
					var originalFile = result[i];
					var fileResult = fileResults[i];

					try
					{
						using var originalStream = await originalFile.OpenStreamForReadAsync();
						using var rotatedStream = await ImageProcessor.RotateImageAsync(originalStream, originalFile.Name);

						// Save rotated image to temporary file
						var tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(originalFile.Name)}");
						using (var fileStream = File.Create(tempFileName))
						{
							rotatedStream.Position = 0;
							await rotatedStream.CopyToAsync(fileStream);
						}

						rotatedResults.Add(new FileResult(tempFileName));
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Failed to rotate image: {ex.Message}");
						// If rotation fails, use original file
						rotatedResults.Add(fileResult);
					}
				}
				fileResults = rotatedResults;
			}

			// Apply compression/resizing if specified for photos
			if (photo && ImageProcessor.IsProcessingNeeded(options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100))
			{
				var compressedResults = new List<FileResult>();
				for (int i = 0; i < result.Count; i++)
				{
					var originalFile = result[i];
					var fileResult = fileResults[i];

					using var originalStream = await originalFile.OpenStreamForReadAsync();
					var processedStream = await ImageProcessor.ProcessImageAsync(
						originalStream,
						options?.MaximumWidth,
						options?.MaximumHeight,
						options?.CompressionQuality ?? 100,
						originalFile.Name,
						options?.RotateImage ?? false,
						options?.PreserveMetaData ?? true);

					if (processedStream != null)
					{
						// Convert to MemoryStream for ProcessedImageFileResult
						var memoryStream = new MemoryStream();
						await processedStream.CopyToAsync(memoryStream);
						processedStream.Dispose();

						compressedResults.Add(new ProcessedImageFileResult(memoryStream, originalFile.Name));
					}
					else
					{
						compressedResults.Add(fileResult);
					}
				}
				return compressedResults;
			}

			return fileResults;
		}

		public Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null)
			=> CaptureAsync(options, true);

		public Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null)
			=> CaptureAsync(options, false);

		public async Task<FileResult?> CaptureAsync(MediaPickerOptions? options, bool photo)
		{
			var captureUi = new WinUICameraCaptureUI();

			if (photo)
			{
				captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
			}
			else
			{
				captureUi.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;
			}

			var file = await captureUi.CaptureFileAsync(photo ? CameraCaptureUIMode.Photo : CameraCaptureUIMode.Video);

			if (file is not null)
			{
				var fileResult = new FileResult(file);

				// Apply rotation if needed for photos
				if (photo && ImageProcessor.IsRotationNeeded(options) && file is not null)
				{
					try
					{
						using var originalStream = await file.OpenStreamForReadAsync();
						using var rotatedStream = await ImageProcessor.RotateImageAsync(originalStream, file.Name);

						// Save rotated image to temporary file
						var tempFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}");
						using (var fileStream = File.Create(tempFileName))
						{
							rotatedStream.Position = 0;
							await rotatedStream.CopyToAsync(fileStream);
						}

						fileResult = new FileResult(tempFileName);
					}
					catch (Exception ex)
					{
						// If rotation fails, continue with the original file
						System.Diagnostics.Debug.WriteLine($"Failed to rotate image: {ex.Message}");
					}
				}

				// Apply compression/resizing if specified for photos
				if (photo && ImageProcessor.IsProcessingNeeded(options?.MaximumWidth, options?.MaximumHeight, options?.CompressionQuality ?? 100))
				{
					using var originalStream = await file.OpenStreamForReadAsync();
					var processedStream = await ImageProcessor.ProcessImageAsync(
						originalStream,
						options?.MaximumWidth,
						options?.MaximumHeight,
						options?.CompressionQuality ?? 100,
						file!.Name,
						options?.RotateImage ?? false,
						options?.PreserveMetaData ?? true);

					if (processedStream is not null)
					{
						// Convert to MemoryStream for ProcessedImageFileResult
						var memoryStream = new MemoryStream();
						await processedStream.CopyToAsync(memoryStream);
						processedStream.Dispose();

						return new ProcessedImageFileResult(memoryStream, file.Name);
					}
				}

				return fileResult;
			}

			return null;
		}

		class WinUICameraCaptureUI
		{
			const string WindowsCameraAppPackageName = "Microsoft.WindowsCamera_8wekyb3d8bbwe";
			const string WindowsCameraAppUri = "microsoft.windows.camera.picker:";

			const string CacheFolderName = ".Microsoft.Maui.Media.MediaPicker";
			const string CacheFileName = "capture";

			public WinUICameraCaptureUIPhotoCaptureSettings PhotoSettings { get; } = new();

			public WinUICameraCaptureUIVideoCaptureSettings VideoSettings { get; } = new();

			public async Task<StorageFile?> CaptureFileAsync(CameraCaptureUIMode mode)
			{
				var hwnd = WindowStateManager.Default.GetActiveWindowHandle(true);

				var options = new LauncherOptions();
				InitializeWithWindow.Initialize(options, hwnd);

				options.TreatAsUntrusted = false;
				options.DisplayApplicationPicker = false;
				options.TargetApplicationPackageFamilyName = WindowsCameraAppPackageName;

				var extension = mode == CameraCaptureUIMode.Photo
					? PhotoSettings.GetFormatExtension()
					: VideoSettings.GetFormatExtension();

				var tempLocation = await StorageFolder.GetFolderFromPathAsync(FileSystem.CacheDirectory);
				var tempFolder = await tempLocation.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists);
				var tempFile = await tempFolder.CreateFileAsync($"{CacheFileName}{extension}", CreationCollisionOption.GenerateUniqueName);
				var token = global::Windows.ApplicationModel.DataTransfer.SharedStorageAccessManager.AddFile(tempFile);

				var set = new ValueSet();
				if (mode == CameraCaptureUIMode.Photo)
				{
					set.Add("MediaType", "photo");
					set.Add("PhotoFileToken", token);
				}
				else
				{
					set.Add("MediaType", "video");
					set.Add("VideoFileToken", token);
				}

				var uri = new Uri(WindowsCameraAppUri);
				var result = await WinLauncher.LaunchUriForResultsAsync(uri, options, set);

				global::Windows.ApplicationModel.DataTransfer.SharedStorageAccessManager.RemoveFile(token);

				if (result.Status == LaunchUriStatus.Success && result.Result is not null)
					return tempFile;

				return null;
			}
		}

		class WinUICameraCaptureUIPhotoCaptureSettings
		{
			public CameraCaptureUIPhotoFormat Format { get; set; }

			public string GetFormatExtension() =>
				Format switch
				{
					CameraCaptureUIPhotoFormat.Jpeg => ".jpg",
					CameraCaptureUIPhotoFormat.Png => ".png",
					CameraCaptureUIPhotoFormat.JpegXR => ".jpg",
					_ => ".jpg",
				};
		}

		class WinUICameraCaptureUIVideoCaptureSettings
		{
			public CameraCaptureUIVideoFormat Format { get; set; }

			public string GetFormatExtension() =>
				Format switch
				{
					CameraCaptureUIVideoFormat.Mp4 => ".mp4",
					CameraCaptureUIVideoFormat.Wmv => ".wmv",
					_ => ".mp4",
				};
		}
	}
	/// <summary>
	/// FileResult implementation for processed images using MAUI Graphics
	/// </summary>
	internal class ProcessedImageFileResult : FileResult, IDisposable
	{
		readonly MemoryStream imageData;

		internal ProcessedImageFileResult(MemoryStream imageData, string? originalFileName = null)
			: base()
		{
			this.imageData = imageData;

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
