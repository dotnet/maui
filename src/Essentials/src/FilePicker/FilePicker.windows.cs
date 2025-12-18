using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Microsoft.Maui.Storage
{
	partial class FilePickerImplementation : IFilePicker
	{
		async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
		{
			var picker = new FileOpenPicker
			{
				ViewMode = PickerViewMode.List,
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};

			var hwnd = WindowStateManager.Default.GetActiveWindowHandle(true);
			WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

			SetFileTypes(options, picker);

			var resultList = new List<StorageFile>();

			if (allowMultiple)
			{
				var fileList = await picker.PickMultipleFilesAsync();
				if (fileList != null && fileList.Any())
					resultList.AddRange(fileList);
			}
			else
			{
				var file = await picker.PickSingleFileAsync();
				if (file != null)
					resultList.Add(file);
			}

			return resultList.Count == 0 ? [] : resultList.Select(storageFile => new FileResult(storageFile));
		}

		static void SetFileTypes(PickOptions options, FileOpenPicker picker)
		{
			var hasAtLeastOneType = false;

			if (options?.FileTypes?.Value != null)
			{
				foreach (var type in options.FileTypes.Value)
				{
					var ext = FileExtensions.Clean(type);
					if (!string.IsNullOrWhiteSpace(ext))
					{
						picker.FileTypeFilter.Add(ext);
						hasAtLeastOneType = true;
					}
				}
			}

			if (!hasAtLeastOneType)
				picker.FileTypeFilter.Add("*");
		}
	}

	public partial class FilePickerFileType
	{
		static FilePickerFileType PlatformImageFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.WinUI, FileExtensions.AllImage }
			});

		static FilePickerFileType PlatformPngFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.WinUI, new[] { FileExtensions.Png } }
			});

		static FilePickerFileType PlatformJpegFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.WinUI, FileExtensions.AllJpeg }
			});

		static FilePickerFileType PlatformVideoFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.WinUI, FileExtensions.AllVideo }
			});

		static FilePickerFileType PlatformPdfFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.WinUI, new[] { FileExtensions.Pdf } }
			});
	}
}
