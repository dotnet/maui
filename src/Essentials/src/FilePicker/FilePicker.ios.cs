using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using MobileCoreServices;
using UIKit;

namespace Microsoft.Maui.Storage
{
	partial class FilePickerImplementation : IFilePicker
	{
		async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
		{
#pragma warning disable CA1416 // TODO: UTType has [UnsupportedOSPlatform("ios14.0")]
#pragma warning disable CA1422 // Validate platform compatibility
			var allowedUtis = options?.FileTypes?.Value?.ToArray() ?? new string[]
			{
				UTType.Content,
				UTType.Item,
				"public.data"
			};

			var tcs = new TaskCompletionSource<IEnumerable<FileResult>>();

			// Use Open instead of Import so that we can attempt to use the original file.
			// If the file is from an external provider, then it will be downloaded.

			using var documentPicker = new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Open);
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Constructor UIDocumentPickerViewController  has [UnsupportedOSPlatform("ios14.0")]
			if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
				documentPicker.AllowsMultipleSelection = allowMultiple;
			documentPicker.Delegate = new PickerDelegate
			{
				PickHandler = urls => GetFileResults(urls, tcs)
			};

#if !MACCATALYST
			if (documentPicker.PresentationController != null)
			{
				documentPicker.PresentationController.Delegate =
					new UIPresentationControllerDelegate(() => GetFileResults(null, tcs));
			}
#endif

			var parentController = WindowStateManager.Default.GetCurrentUIViewController(true);

			parentController.PresentViewController(documentPicker, true, null);

			return await tcs.Task;
		}

		static async void GetFileResults(NSUrl[] urls, TaskCompletionSource<IEnumerable<FileResult>> tcs)
		{
			try
			{
				var results = await FileSystemUtils.EnsurePhysicalFileResultsAsync(urls);

				tcs.TrySetResult(results);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}

		class PickerDelegate : UIDocumentPickerDelegate
		{
			public Action<NSUrl[]> PickHandler { get; set; }

			public override void WasCancelled(UIDocumentPickerViewController controller)
				=> PickHandler?.Invoke(null);

			public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl[] urls)
				=> PickHandler?.Invoke(urls);

			public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl url)
				=> PickHandler?.Invoke(new NSUrl[] { url });
		}
	}

	public partial class FilePickerFileType
	{
#pragma warning disable CA1416 // TODO: UTType has [UnsupportedOSPlatform("ios14.0")]
#pragma warning disable CA1422 // Validate platform compatibility
		static FilePickerFileType PlatformImageFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.Image } },
				{ DevicePlatform.MacCatalyst, new[] { (string)UTType.Image } }
			});

		static FilePickerFileType PlatformPngFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.PNG } },
				{ DevicePlatform.MacCatalyst, new[] { (string)UTType.PNG } }
			});

		static FilePickerFileType PlatformJpegFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.JPEG } },
				{ DevicePlatform.MacCatalyst, new[] { (string)UTType.JPEG } }
			});

		static FilePickerFileType PlatformVideoFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new string[] { UTType.MPEG4, UTType.Video, UTType.AVIMovie, UTType.AppleProtectedMPEG4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } },
				{ DevicePlatform.MacCatalyst, new string[] { UTType.MPEG4, UTType.Video, UTType.AVIMovie, UTType.AppleProtectedMPEG4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } }
			});

		static FilePickerFileType PlatformPdfFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.PDF } },
				{ DevicePlatform.MacCatalyst, new[] { (string)UTType.PDF } }
			});
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
	}
}
