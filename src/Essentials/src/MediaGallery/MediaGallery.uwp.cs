using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Microsoft.Maui.Media
{
	partial class MediaGalleryImplementation
	{
		public bool IsSupported => true;

		public bool CheckCaptureSupport(MediaFileType type) => true;

		public async Task<IEnumerable<MediaFileResult>> PlatformCaptureAsync(MediaFileType type, CancellationToken token = default)
		{
			var captureUi = new CameraCaptureUI();
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
	}
}
