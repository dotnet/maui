// Using code based on https://github.com/richardrigutins/maui-sample-windows-camera-workaround/blob/4e8ab1eb2fe36e9d8253773705df508b7979a968/src/MauiSampleCamera/Platforms/Windows/CustomMediaPicker.uwp.cs
// Authors:
//  - https://github.com/GiampaoloGabba
//  - https://github.com/richardrigutins

using System;
using System.Linq;
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
	partial class MediaPickerImplementation : IMediaPicker
	{
		public bool IsCaptureSupported
			=> true;

		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> PickAsync(options, true);

		public Task<FileResult> PickVideoAsync(MediaPickerOptions options)
			=> PickAsync(options, false);

		public async Task<FileResult> PickAsync(MediaPickerOptions options, bool photo)
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
				return null;

			// picked
			return new FileResult(result);
		}

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, true);

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options)
			=> CaptureAsync(options, false);

		public async Task<FileResult> CaptureAsync(MediaPickerOptions options, bool photo)
		{
			var captureUi = new WinUICameraCaptureUI();

			if (photo)
				captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
			else
				captureUi.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

			var file = await captureUi.CaptureFileAsync(photo ? CameraCaptureUIMode.Photo : CameraCaptureUIMode.Video);

			if (file is not null)
				return new FileResult(file);

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
