using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using UniformTypeIdentifiers;
using UIKit;

namespace Microsoft.Maui.Storage
{
	partial class FilePickerImplementation : IFilePicker
	{
		async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
		{
			var allowedUtis = options?.FileTypes?.Value?.ToArray() ?? new string[]
			{
				FilePickerUTType.Content,
				FilePickerUTType.Item,
				FilePickerUTType.Data
			};

			var tcs = new TaskCompletionSource<IEnumerable<FileResult>>();

			// Use Open instead of Import so that we can attempt to use the original file.
			// If the file is from an external provider, then it will be downloaded.

			using var documentPicker = CreateDocumentPicker(allowedUtis);
			documentPicker.AllowsMultipleSelection = allowMultiple;

			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
			{
				EventHandler<UIDocumentPickedAtUrlsEventArgs> pickHandler = null;
				EventHandler cancelHandler = null;

				pickHandler = (_, e) =>
				{
					documentPicker.DidPickDocumentAtUrls -= pickHandler;
					documentPicker.WasCancelled -= cancelHandler;

					GetFileResults(e.Urls, tcs);
				};

				cancelHandler = (_, e) =>
				{
					documentPicker.DidPickDocumentAtUrls -= pickHandler;
					documentPicker.WasCancelled -= cancelHandler;

					GetFileResults(null, tcs);
				};

				documentPicker.DidPickDocumentAtUrls += pickHandler;
				documentPicker.WasCancelled += cancelHandler;
			}
			else
			{
				documentPicker.Delegate = new PickerDelegate
				{
					PickHandler = urls => GetFileResults(urls, tcs)
				};
			}

#if !MACCATALYST
			if (documentPicker.PresentationController != null && !(OperatingSystem.IsIOSVersionAtLeast(14, 0) && NSProcessInfo.ProcessInfo.IsiOSApplicationOnMac))
			{
				documentPicker.PresentationController.Delegate =
					new UIPresentationControllerDelegate(() => GetFileResults(null, tcs));
			}
#endif

			var parentController = WindowStateManager.Default.GetCurrentUIViewController(true);
			parentController.PresentViewController(documentPicker, true, null);

			return await tcs.Task;
		}

		static UIDocumentPickerViewController CreateDocumentPicker(string[] allowedUtis)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsMacCatalystVersionAtLeast(14))
				return CreateModernDocumentPicker(allowedUtis);

#pragma warning disable CA1416, CA1422 // The string-based constructor is required for MAUI's iOS 13 support.
			return new UIDocumentPickerViewController(allowedUtis, UIDocumentPickerMode.Open);
#pragma warning restore CA1416, CA1422
		}

		[SupportedOSPlatform("ios14.0")]
		[SupportedOSPlatform("maccatalyst14.0")]
		static UIDocumentPickerViewController CreateModernDocumentPicker(string[] allowedUtis)
		{
			var contentTypes = new List<UTType>(allowedUtis.Length);

			foreach (var allowedUti in allowedUtis)
			{
				var contentType = UTType.CreateFromIdentifier(allowedUti);

				if (contentType is null)
					contentType = UTType.GetType(allowedUti.TrimStart('.'), UTTagClass.FilenameExtension, null);

				contentTypes.Add(contentType ?? UTType.CreateImportedType(allowedUti));
			}

			return new UIDocumentPickerViewController(contentTypes.ToArray(), asCopy: false);
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
		static FilePickerFileType PlatformImageFileType() =>
			new(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { FilePickerUTType.Image } },
				{ DevicePlatform.MacCatalyst, new[] { FilePickerUTType.Image } }
			});

		static FilePickerFileType PlatformPngFileType() =>
			new(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { FilePickerUTType.Png } },
				{ DevicePlatform.MacCatalyst, new[] { FilePickerUTType.Png } }
			});

		static FilePickerFileType PlatformJpegFileType() =>
			new(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { FilePickerUTType.Jpeg } },
				{ DevicePlatform.MacCatalyst, new[] { FilePickerUTType.Jpeg } }
			});

		static FilePickerFileType PlatformVideoFileType() =>
			new(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new string[] { FilePickerUTType.Mpeg4, FilePickerUTType.Video, FilePickerUTType.AviMovie, FilePickerUTType.AppleProtectedMpeg4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } },
				{ DevicePlatform.MacCatalyst, new string[] { FilePickerUTType.Mpeg4, FilePickerUTType.Video, FilePickerUTType.AviMovie, FilePickerUTType.AppleProtectedMpeg4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } }
			});

		static FilePickerFileType PlatformPdfFileType() =>
			new(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.iOS, new[] { FilePickerUTType.Pdf } },
				{ DevicePlatform.MacCatalyst, new[] { FilePickerUTType.Pdf } }
			});
	}

	static class FilePickerUTType
	{
		public const string Content = "public.content";
		public const string Item = "public.item";
		public const string Data = "public.data";
		public const string Image = "public.image";
		public const string Png = "public.png";
		public const string Jpeg = "public.jpeg";
		public const string Mpeg4 = "public.mpeg-4";
		public const string Video = "public.video";
		public const string AviMovie = "public.avi";
		public const string AppleProtectedMpeg4Video = "com.apple.protected-mpeg-4-video";
		public const string Pdf = "com.adobe.pdf";
	}
}
