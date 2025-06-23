using System;
using System.Collections.Generic;
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
						GetFileResult(info, tcs);
						await vc.DismissViewControllerAsync(true);
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
				return null;
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
						CompletedHandler = res =>
							tcs.TrySetResult(PickerResultsToMediaFiles(res))
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
					Handler = () => tcs.TrySetResult(null)
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

		static List<FileResult> PickerResultsToMediaFiles(PHPickerResult[] results)
		{
			return results?
				.Select(file => (FileResult)new PHPickerFileResult(file.ItemProvider))
				.ToList() ?? [];
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
						return new UIDocumentFileResult(assetUrl);
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
					return new UIImageFileResult(img);
				}
			}

			if (phAsset is null || assetUrl is null)
			{
				return null;
			}

			string originalFilename = PHAssetResource.GetAssetResources(phAsset).FirstOrDefault()?.OriginalFilename;
			return new PHAssetFileResult(assetUrl, phAsset, originalFilename);
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
			CompletedHandler?.Invoke(results?.Length > 0 ? results : null);
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
}
