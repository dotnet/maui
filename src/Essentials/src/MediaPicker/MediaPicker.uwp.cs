using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Windows.Media.Capture;
using Windows.Storage.Pickers;

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
			if (result == null)
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
			var captureUi = new CameraCaptureUI();

			if (photo)
				captureUi.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
			else
				captureUi.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;

			var file = await captureUi.CaptureFileAsync(photo ? CameraCaptureUIMode.Photo : CameraCaptureUIMode.Video);

			if (file != null)
				return new FileResult(file);

			return null;
		}
	}
}
