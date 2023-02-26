using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Photos;
using PhotosUI;
using UIKit;
using MobileCoreServices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Media
{
	partial class MediaGalleryImplementation
	{
		UIViewController pickerRef;
		UIViewController cameraRef;

		public bool IsSupported => OperatingSystem.IsIOSVersionAtLeast(11);

		public bool CheckCaptureSupport(MediaFileType type)
			=> UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);

		public async Task<IEnumerable<MediaFileResult>> PlatformCaptureAsync(MediaFileType type, CancellationToken token = default)
		{
			var tcs = new TaskCompletionSource<IEnumerable<MediaFileResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
			CancelTaskIfRequested(token, tcs, false);

			await MainThread.InvokeOnMainThreadAsync(() =>
			{
				CancelTaskIfRequested(token, tcs, false);

				cameraRef = new UIImagePickerController
				{
					SourceType = UIImagePickerControllerSourceType.Camera,
					AllowsEditing = false,
					Delegate = new PhotoPickerDelegate(tcs),
					CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo,
				};

				CancelTaskIfRequested(token, tcs, false);
				var vc = Platform.GetCurrentUIViewController();

				ConfigureController(cameraRef, tcs);


				CancelTaskIfRequested(token, tcs, false);
				vc.PresentViewController(cameraRef, true, () => PresentViewControllerHandler(cameraRef, token, tcs));
			});

			return await tcs.Task.ConfigureAwait(false);
		}

		public async Task<IEnumerable<MediaFileResult>> PlatformPickAsync(MediaPickRequest request, CancellationToken token = default)
		{
			token.ThrowIfCancellationRequested();

			try
			{
				var isVideo = request.Types.Contains(MediaFileType.Video);
				var isImage = request.Types.Contains(MediaFileType.Image);

				var tcs = new TaskCompletionSource<IEnumerable<MediaFileResult>>(TaskCreationOptions.RunContinuationsAsynchronously);

				CancelTaskIfRequested(token, tcs);

				await MainThread.InvokeOnMainThreadAsync(() =>
				{
					CancelTaskIfRequested(token, tcs);

					if (OperatingSystem.IsIOSVersionAtLeast(14))
					{
						var config = new PHPickerConfiguration();
						config.SelectionLimit = request.SelectionLimit;

						if (!(isVideo && isImage))
							config.Filter = isVideo
								? PHPickerFilter.VideosFilter
								: PHPickerFilter.ImagesFilter;

						pickerRef = new PHPickerViewController(config)
						{
							Delegate = new PHPickerDelegate(tcs),
						};
					}
					else
					{
#pragma warning disable CA1422
						var sourceType = UIImagePickerControllerSourceType.PhotoLibrary;

						if (!UIImagePickerController.IsSourceTypeAvailable(sourceType))
							throw new FeatureNotSupportedException();

						var availableTypes = UIImagePickerController.AvailableMediaTypes(sourceType);
						isVideo = isVideo && availableTypes.Contains(UTType.Movie);
						isImage = isImage && availableTypes.Contains(UTType.Image);
						if (!(isVideo || isImage))
							throw new FeatureNotSupportedException();

						pickerRef = new UIImagePickerController
						{
							SourceType = sourceType,
							AllowsEditing = false,
							Delegate = new PhotoPickerDelegate(tcs),
							MediaTypes = isVideo && isImage
								? new string[] { UTType.Movie, UTType.Image }
								: new string[] { isVideo ? UTType.Movie : UTType.Image },
						};
#pragma warning restore CA1422
					}

					CancelTaskIfRequested(token, tcs);
					var vc = Platform.GetCurrentUIViewController();

					if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
					{
						pickerRef.ModalPresentationStyle = request.PresentationSourceBounds != Graphics.Rect.Zero
							? UIModalPresentationStyle.Popover
							: UIModalPresentationStyle.PageSheet;

						if (pickerRef.PopoverPresentationController != null)
						{
							pickerRef.PopoverPresentationController.SourceView = vc.View!;
							pickerRef.PopoverPresentationController.SourceRect = request.PresentationSourceBounds.AsCGRect();
						}
					}

					ConfigureController(pickerRef, tcs);

					CancelTaskIfRequested(token, tcs);

					vc!.PresentViewController(pickerRef, true, () => PresentViewControllerHandler(pickerRef, token, tcs));
				});

				CancelTaskIfRequested(token, tcs, false);
				return await tcs.Task.ConfigureAwait(false);
			}
			finally
			{
				pickerRef?.Dispose();
				pickerRef = null;
			}
		}

		public async Task PlatformSaveAsync(MediaFileType type, Stream fileStream, string fileName)
		{
			string filePath = null;

			try
			{
				filePath = GetFilePath(fileName);
				using var stream = File.Create(filePath);
				await fileStream.CopyToAsync(stream);
				stream.Close();

				await PlatformSaveAsync(type, filePath).ConfigureAwait(false);
			}
			finally
			{
				DeleteFile(filePath);
			}
		}

		public async Task PlatformSaveAsync(MediaFileType type, byte[] data, string fileName)
		{
			string filePath = null;

			try
			{
				filePath = GetFilePath(fileName);
				await File.WriteAllBytesAsync(filePath, data);

				await PlatformSaveAsync(type, filePath).ConfigureAwait(false);
			}
			finally
			{
				DeleteFile(filePath);
			}
		}

		public Task PlatformSaveAsync(MediaFileType type, string filePath)
		{
			using var fileUri = new NSUrl(filePath);

			return PhotoLibraryPerformChanges(() =>
			{
				using var request = type == MediaFileType.Video
				? PHAssetChangeRequest.FromVideo(fileUri!)
				: PHAssetChangeRequest.FromImage(fileUri!);
			});
		}


		static async Task PhotoLibraryPerformChanges(Action action)
		{
			var tcs = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);

			PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(
				() =>
				{
					try
					{
						action.Invoke();
					}
					catch (Exception ex)
					{
						tcs.TrySetResult(ex);
					}
				},
				(success, error) =>
					tcs.TrySetResult(error != null ? new NSErrorException(error) : null));

			var exception = await tcs.Task;
			if (exception != null)
				throw exception;
		}

		static void ConfigureController(UIViewController controller, TaskCompletionSource<IEnumerable<MediaFileResult>> tcs)
		{
			if (controller.PresentationController != null)
				controller.PresentationController.Delegate = new UIPresentationControllerDelegate(() => tcs?.TrySetResult(null));
		}

		static void CancelTaskIfRequested(CancellationToken token, TaskCompletionSource<IEnumerable<MediaFileResult>> tcs, bool needThrow = true)
		{
			if (!token.IsCancellationRequested)
				return;
			tcs?.TrySetCanceled(token);
			if (needThrow)
				token.ThrowIfCancellationRequested();
		}

		static void PresentViewControllerHandler(UIViewController controller, CancellationToken token, TaskCompletionSource<IEnumerable<MediaFileResult>> tcs)
		{
			if (!token.CanBeCanceled)
				return;

			token.Register(
				() => MainThread.BeginInvokeOnMainThread(
					() => controller?.DismissViewController(true,
						() => tcs?.TrySetCanceled(token))));
		}


		class PHPickerDelegate : PHPickerViewControllerDelegate
		{
			readonly TaskCompletionSource<IEnumerable<MediaFileResult>> tcs;

			internal PHPickerDelegate(TaskCompletionSource<IEnumerable<MediaFileResult>> tcs)
				=> this.tcs = tcs;

			public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
			{
				picker.DismissViewController(true, null);
				tcs?.TrySetResult(results?.Length > 0 ? ConvertPickerResults(results) : null);
			}

			static IEnumerable<MediaFileResult> ConvertPickerResults(PHPickerResult[] results)
				=> results
				.Select(res => res.ItemProvider)
				.Where(provider => provider != null && provider.RegisteredTypeIdentifiers?.Length > 0)
				.Select(provider => new PHPickerFileResult(provider))
				.ToArray();
		}

		class PhotoPickerDelegate : UIImagePickerControllerDelegate
		{
			readonly TaskCompletionSource<IEnumerable<MediaFileResult>> tcs;

			internal PhotoPickerDelegate(TaskCompletionSource<IEnumerable<MediaFileResult>> tcs)
				=> this.tcs = tcs;

			public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
			{
				picker.DismissViewController(true, null);
				var result = ConvertPickerResults(info);
				tcs.TrySetResult(result == null ? null : new MediaFileResult[] { result });
			}

			public override void Canceled(UIImagePickerController picker)
			{
				picker.DismissViewController(true, null);
				tcs?.TrySetResult(null);
			}

			MediaFileResult ConvertPickerResults(NSDictionary info)
			{
				if (info == null)
					return null;

				var assetUrl = (info.ValueForKey(UIImagePickerController.ImageUrl)
					?? info.ValueForKey(UIImagePickerController.MediaURL)) as NSUrl;

				var path = assetUrl?.Path;

				if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
					return new UIDocumentFileResult(assetUrl, GetOriginalName(info));

				assetUrl?.Dispose();
				var img = info.ValueForKey(UIImagePickerController.OriginalImage) as UIImage;
				var meta = info.ValueForKey(UIImagePickerController.MediaMetadata) as NSDictionary;

				if (img != null && meta != null)
					return new UIImageFileResult(img, meta, GetNewImageName());

				return null;
			}

			string GetOriginalName(NSDictionary info)
			{
#pragma warning disable CA1422
				if (PHPhotoLibrary.AuthorizationStatus != PHAuthorizationStatus.Authorized
				    || !info.ContainsKey(UIImagePickerController.PHAsset))
					return null;

				using var asset = info.ValueForKey(UIImagePickerController.PHAsset) as PHAsset;

				return asset != null
					? PHAssetResource.GetAssetResources(asset)?.FirstOrDefault()?.OriginalFilename
					: null;
#pragma warning restore CA1422
			}
		}

	}
}
