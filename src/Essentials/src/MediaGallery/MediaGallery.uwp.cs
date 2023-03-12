// Using code based on https://github.com/richardrigutins/maui-sample-windows-camera-workaround/blob/4e8ab1eb2fe36e9d8253773705df508b7979a968/src/MauiSampleCamera/Platforms/Windows/CustomMediaPicker.uwp.cs
// Authors:
//  - https://github.com/GiampaoloGabba
//  - https://github.com/richardrigutins

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
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
	partial class MediaGalleryImplementation
	{
		public bool IsSupported => true;

		public bool CheckCaptureSupport(MediaFileType type) => true;

		public async Task<IEnumerable<MediaFileResult>> PlatformCaptureAsync(MediaFileType type, CancellationToken token = default)
		{
			var captureUi = new WinUICameraCaptureUI();
			var photo = type == MediaFileType.Image;

			if (photo)
				captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
			else
				captureUi.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

			var file = await captureUi.CaptureFileAsync(photo ? CameraCaptureUIMode.Photo : CameraCaptureUIMode.Video);

			if (file != null)
				return new[] { new MediaFileResult(file) };
			return null;
		}

		public async Task<IEnumerable<MediaFileResult>> PlatformPickAsync(MediaPickRequest request, CancellationToken token = default)
		{
			var picker = new FileOpenPicker();

			var hwnd = WindowStateManager.Default.GetActiveWindowHandle(true);
			WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

			List<string> defaultTypes = new();

			if (request.Types.Contains(MediaFileType.Image))
				defaultTypes.AddRange(FilePickerFileType.Images.Value);
			if (request.Types.Contains(MediaFileType.Video))
				defaultTypes.AddRange(FilePickerFileType.Videos.Value);

			// set picker properties
			foreach (var filter in defaultTypes.Select(t => t.TrimStart('*')))
				picker.FileTypeFilter.Add(filter);
			picker.SuggestedStartLocation = request.Types.Contains(MediaFileType.Image) ? PickerLocationId.PicturesLibrary : PickerLocationId.VideosLibrary;
			picker.ViewMode = PickerViewMode.Thumbnail;

			if (request.SelectionLimit > 1)
			{
				var result = await picker.PickMultipleFilesAsync();
				return result?.Select(a => new MediaFileResult(a));
			}
			else
			{
				var result = await picker.PickSingleFileAsync();

				if (result == null)
					return null;

				return new[] { new MediaFileResult(result) };
			}
		}
		
		public MultiPickingBehaviour GetMultiPickingBehaviour()
			=> MultiPickingBehaviour.UnLimit;

		public async Task PlatformSaveAsync(MediaFileType type, Stream fileStream, string fileName)
		{
			var file = await GetStorageFile(type, fileName);
			using var stream = await file.OpenStreamForWriteAsync();
			await fileStream.CopyToAsync(stream);
			stream.Close();
		}

		public async Task PlatformSaveAsync(MediaFileType type, byte[] data, string fileName)
		{
			var file = await GetStorageFile(type, fileName);
			var buffer = WindowsRuntimeBuffer.Create(data, 0, data.Length, data.Length);
			await FileIO.WriteBufferAsync(file, buffer);
		}

		public async Task PlatformSaveAsync(MediaFileType type, string filePath)
		{
			using var fileStream = File.OpenRead(filePath);
			await PlatformSaveAsync(type, fileStream, Path.GetFileName(filePath));
		}

		static async Task<StorageFile> GetStorageFile(MediaFileType type, string fileName)
		{
			var albumFolder = await GetAlbumFolder(type, AppInfo.Name);
			return await albumFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
		}

		static async Task<StorageFolder> GetAlbumFolder(MediaFileType type, string albumName)
		{
			var mediaFolder = type == MediaFileType.Image
					? KnownFolders.PicturesLibrary
					: KnownFolders.VideosLibrary;

			var folder = (await mediaFolder.GetFoldersAsync())?.FirstOrDefault(a => a.Name == albumName);
			return folder ?? await mediaFolder.CreateFolderAsync(albumName);
		}

		class WinUICameraCaptureUI
		{
			const string WindowsCameraAppPackageName = "Microsoft.WindowsCamera_8wekyb3d8bbwe";
			const string WindowsCameraAppUri = "microsoft.windows.camera.picker:";

			const string CacheFolderName = ".Microsoft.Maui.Media.MediaPicker";
			const string CacheFileName = "capture";

			public WinUICameraCaptureUIPhotoCaptureSettings PhotoSettings { get; } = new();

			public WinUICameraCaptureUIVideoCaptureSettings VideoSettings { get; } = new();

			public async Task<StorageFile> CaptureFileAsync(CameraCaptureUIMode mode)
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
}
