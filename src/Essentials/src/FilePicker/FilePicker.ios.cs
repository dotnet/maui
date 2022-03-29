using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class FilePicker
	{
		static async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
		{
			Debug.Assert(!OperatingSystem.IsIOSVersionAtLeast(14));
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
			if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
				documentPicker.AllowsMultipleSelection = allowMultiple;
			documentPicker.Delegate = new PickerDelegate
			{
				PickHandler = urls => GetFileResults(urls, tcs)
			};

			if (documentPicker.PresentationController != null)
			{
				documentPicker.PresentationController.Delegate =
					new Platform.UIPresentationControllerDelegate(() => GetFileResults(null, tcs));
			}

			var parentController = Platform.GetCurrentViewController();

			parentController.PresentViewController(documentPicker, true, null);

			return await tcs.Task;
		}

		static async void GetFileResults(NSUrl[] urls, TaskCompletionSource<IEnumerable<FileResult>> tcs)
		{
			try
			{
				var results = await FileSystem.EnsurePhysicalFileResultsAsync(urls);

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

	[UnsupportedOSPlatform("ios14.0")]
	public partial class FilePickerFileType
	{
		static FilePickerFileType PlatformImageFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.Image } }
			});

		static FilePickerFileType PlatformPngFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.PNG } }
			});

		static FilePickerFileType PlatformJpegFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.JPEG } }
			});

		static FilePickerFileType PlatformVideoFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new string[] { UTType.MPEG4, UTType.Video, UTType.AVIMovie, UTType.AppleProtectedMPEG4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } }
			});

		static FilePickerFileType PlatformPdfFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.PDF } }
			});
	}
}
