using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using MobileCoreServices;

namespace Xamarin.Essentials
{
	public static partial class FilePicker
	{
		static Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
		{
			var openPanel = new NSOpenPanel
			{
				CanChooseFiles = true,
				AllowsMultipleSelection = allowMultiple,
				CanChooseDirectories = false
			};

			if (options.PickerTitle != null)
				openPanel.Title = options.PickerTitle;

			SetFileTypes(options, openPanel);

			var resultList = new List<FileResult>();
			var panelResult = openPanel.RunModal();
			if (panelResult == (nint)(long)NSModalResponse.OK)
			{
				foreach (var url in openPanel.Urls)
					resultList.Add(new FileResult(url.Path));
			}

			return Task.FromResult<IEnumerable<FileResult>>(resultList);
		}

		static void SetFileTypes(PickOptions options, NSOpenPanel panel)
		{
			var allowedFileTypes = new List<string>();

			if (options?.FileTypes?.Value != null)
			{
				foreach (var type in options.FileTypes.Value)
				{
					allowedFileTypes.Add(type.TrimStart('*', '.'));
				}
			}

			panel.AllowedFileTypes = allowedFileTypes.ToArray();
		}
	}

	public partial class FilePickerFileType
	{
		static FilePickerFileType PlatformImageFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.macOS, new string[] { UTType.PNG, UTType.JPEG, "jpeg" } }
			});

		static FilePickerFileType PlatformPngFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.macOS, new string[] { UTType.PNG } }
			});

		static FilePickerFileType PlatformJpegFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.macOS, new string[] { UTType.JPEG } }
			});

		static FilePickerFileType PlatformVideoFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.macOS, new string[] { UTType.MPEG4, UTType.Video, UTType.AVIMovie, UTType.AppleProtectedMPEG4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt" } }
			});

		static FilePickerFileType PlatformPdfFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.macOS, new string[] { UTType.PDF } }
			});
	}
}
