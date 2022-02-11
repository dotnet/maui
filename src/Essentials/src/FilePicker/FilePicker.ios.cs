using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using MobileCoreServices;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FilePickerImplementation : IFilePicker
	{
		public async Task<IEnumerable<FileResult>> PickAsync(PickOptions options, bool allowMultiple = false)
		{
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
			if (Platform.HasOSVersion(11, 0))
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

		public async Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options)
		{
			return await PickAsync(options, true);
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

	public partial class FilePickerFileTypeImplementation : IFilePickerFileType
	{
		readonly IDictionary<DevicePlatform, IEnumerable<string>> fileTypes;

		public FilePickerFileTypeImplementation(IDictionary<DevicePlatform, IEnumerable<string>> fileTypes) =>
			this.fileTypes = fileTypes;

		public IFilePickerFileType ImageFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.Image } }
			});

		public IFilePickerFileType PngFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.PNG } }
			});

		public IFilePickerFileType JpegFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.JPEG } }
			});

		public IFilePickerFileType VideoFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new string[] { UTType.MPEG4, UTType.Video, UTType.AVIMovie, UTType.AppleProtectedMPEG4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } }
			});

		public IFilePickerFileType PdfFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { (string)UTType.PDF } }
			});
	}
}
