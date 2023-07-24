using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using MobileCoreServices;
using Photos;
using PhotosUI;
using UIKit;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{
		static UIViewController Picker;

		public bool IsCaptureSupported
			=> UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> PhotoAsync(options, true, true);

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options)
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return PhotoAsync(options, true, false);
		}

		public Task<FileResult> PickVideoAsync(MediaPickerOptions options)
			=> PhotoAsync(options, false, true);

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options)
		{
			if (!IsCaptureSupported)
				throw new FeatureNotSupportedException();

			return PhotoAsync(options, false, false);
		}

		public async Task<FileResult> PhotoAsync(MediaPickerOptions options, bool photo, bool pickExisting)
		{
			var vc = WindowStateManager.Default.GetCurrentUIViewController(true);
			
			var tcs = new TaskCompletionSource<FileResult>(Picker);

			if (OperatingSystem.IsIOSVersionAtLeast(14))
			{
				var config = new PHPickerConfiguration
				{
					Filter = photo
					? PHPickerFilter.ImagesFilter
					: PHPickerFilter.VideosFilter
				};

				var pHPickerViewController = new PHPickerViewController(config)
				{
					Delegate = new PHPickerViewDelegate
					{
						CompletedHandler = async info =>
						{
							GetPHPFileResult(info, tcs);
							await vc.DismissViewControllerAsync(true);
						}
					}
				};

				Picker = pHPickerViewController;
			}
			else
			{
#pragma warning disable CA1416 // TODO: UIImagePickerControllerSourceType.PhotoLibrary, UTType.Image, UTType.Movie is supported on ios version 14 and above
#pragma warning disable CA1422 // Validate platform compatibility
				var sourceType = pickExisting ? UIImagePickerControllerSourceType.PhotoLibrary : UIImagePickerControllerSourceType.Camera;
				var mediaType = photo ? UTType.Image : UTType.Movie;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416

				if (!UIImagePickerController.IsSourceTypeAvailable(sourceType))
					throw new FeatureNotSupportedException();
				if (!UIImagePickerController.AvailableMediaTypes(sourceType).Contains(mediaType))
					throw new FeatureNotSupportedException();

				if (!photo && !pickExisting)
					await Permissions.EnsureGrantedAsync<Permissions.Microphone>();

				// Check if picking existing or not and ensure permission accordingly as they can be set independently from each other
				if (pickExisting && !OperatingSystem.IsIOSVersionAtLeast(11, 0))
#pragma warning disable CA1416 // TODO: Permissions.Photos is supported on ios version 14 and above
					await Permissions.EnsureGrantedAsync<Permissions.Photos>();
#pragma warning restore CA1416

				if (!pickExisting)
					await Permissions.EnsureGrantedAsync<Permissions.Camera>();

				var imagePickerController = new UIImagePickerController
				{
					SourceType = sourceType,
					MediaTypes = new string[] { mediaType },
					AllowsEditing = false,

					Delegate = new PhotoPickerDelegate
					{
						CompletedHandler = async info =>
						{
							GetFileResult(info, tcs);
							await vc.DismissViewControllerAsync(true);
						}
					}
				};

				if (!photo && !pickExisting)
					imagePickerController.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Video;

				if (imagePickerController.PresentationController != null)
				{
					imagePickerController.PresentationController.Delegate =
						new UIPresentationControllerDelegate(() => GetFileResult(null, tcs));
				}

				Picker = imagePickerController;
			}

			if (!string.IsNullOrWhiteSpace(options?.Title))
				Picker.Title = options.Title;

			if (DeviceInfo.Current.Idiom == DeviceIdiom.Tablet && Picker.PopoverPresentationController != null && vc.View != null)
				Picker.PopoverPresentationController.SourceRect = vc.View.Bounds;

			await vc.PresentViewControllerAsync(Picker, true);

			var result = await tcs.Task;

			await vc.DismissViewControllerAsync(true);

			Picker?.Dispose();
			Picker = null;

			return result;
		}

		static void GetFileResult(NSDictionary info, TaskCompletionSource<FileResult> tcs)
		{
			try
			{
				tcs.TrySetResult(DictionaryToMediaFile(info));
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		static FileResult DictionaryToMediaFile(NSDictionary info)
		{
			if (info == null)
				return null;

			PHAsset phAsset = null;
			NSUrl assetUrl = null;

			if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
			{
				assetUrl = info[UIImagePickerController.ImageUrl] as NSUrl;

				// Try the MediaURL sometimes used for videos
				if (assetUrl == null)
					assetUrl = info[UIImagePickerController.MediaURL] as NSUrl;

				if (assetUrl != null)
				{
					if (!assetUrl.Scheme.Equals("assets-library", StringComparison.OrdinalIgnoreCase))
						return new UIDocumentFileResult(assetUrl);
#pragma warning disable CA1416 // TODO: 'UIImagePickerController.PHAsset' is only supported on: 'ios' from version 11.0 to 14.0
#pragma warning disable CA1422 // Validate platform compatibility
					phAsset = info.ValueForKey(UIImagePickerController.PHAsset) as PHAsset;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
				}
			}

#if !MACCATALYST
#pragma warning disable CA1416 // TODO: 'UIImagePickerController.ReferenceUrl' is unsupported on 'ios' 11.0 and later
#pragma warning disable CA1422 // Validate platform compatibility
			if (phAsset == null)
			{
				assetUrl = info[UIImagePickerController.ReferenceUrl] as NSUrl;

				if (assetUrl != null)
					phAsset = PHAsset.FetchAssets(new NSUrl[] { assetUrl }, null)?.LastObject as PHAsset;
			}
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // 'PHAsset.FetchAssets(NSUrl[], PHFetchOptions?)' is unsupported on 'ios' 11.0 and later
#endif

			if (phAsset == null || assetUrl == null)
			{
				var img = info.ValueForKey(UIImagePickerController.OriginalImage) as UIImage;

				if (img != null)
					return new UIImageFileResult(img);
			}

			if (phAsset == null || assetUrl == null)
				return null;
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
#pragma warning disable CA1422 // Validate platform compatibility
			string originalFilename = PHAssetResource.GetAssetResources(phAsset).FirstOrDefault()?.OriginalFilename;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
			return new PHAssetFileResult(assetUrl, phAsset, originalFilename);
		}

		static void GetPHPFileResult(PHPickerResult[] info, TaskCompletionSource<FileResult> tcs)
		{
			try
			{
				tcs.TrySetResult(PHPickerResultToMediaFile(info));
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		static FileResult PHPickerResultToMediaFile(PHPickerResult[] results)
		{
			if (results is null || results.Length == 0)
				return null;

			var result = results[0];

			return result is null ? null : throw new NotImplementedException();
		}

		class PhotoPickerDelegate : UIImagePickerControllerDelegate
		{
			public Action<NSDictionary> CompletedHandler { get; set; }

			public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info) =>
				CompletedHandler?.Invoke(info);

			public override void Canceled(UIImagePickerController picker) =>
				CompletedHandler?.Invoke(null);
		}

		class PHPickerViewDelegate : PHPickerViewControllerDelegate
		{
			public Action<PHPickerResult[]> CompletedHandler { get; set; }

			public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results) =>
				CompletedHandler?.Invoke(results?.Length > 0 ? results : null);
		}
	}
}
