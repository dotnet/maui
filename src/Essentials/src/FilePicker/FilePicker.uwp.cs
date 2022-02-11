using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FilePickerImplementation : IFilePicker
	{
		public async Task<IEnumerable<FileResult>> PickAsync(PickOptions options, bool allowMultiple = false)
		{
			var picker = new FileOpenPicker
			{
				ViewMode = PickerViewMode.List,
				SuggestedStartLocation = PickerLocationId.DocumentsLibrary
			};

			var hwnd = Platform.CurrentWindowHandle;
			WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

			SetFileTypes(options, picker);

			var resultList = new List<StorageFile>();

			if (allowMultiple)
			{
				var fileList = await picker.PickMultipleFilesAsync();
				if (fileList != null)
					resultList.AddRange(fileList);
			}
			else
			{
				var file = await picker.PickSingleFileAsync();
				if (file != null)
					resultList.Add(file);
			}

			foreach (var file in resultList)
				StorageApplicationPermissions.FutureAccessList.Add(file);

			return resultList.Select(storageFile => new FileResult(storageFile));
		}

		public async Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options)
		{
			return await PickAsync(options, allowMultiple = true);
		}

		void SetFileTypes(PickOptions options, FileOpenPicker picker)
		{
			var hasAtLeastOneType = false;

			if (options?.FileTypes?.Value != null)
			{
				foreach (var type in options.FileTypes.Value)
				{
					var ext = FileSystem.Extensions.Clean(type);
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

	public partial class FilePickerFileTypeImplementation : IFilePickerFileType
	{
		static FilePickerFileTypeImplementation ImageFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.UWP, FileSystem.Extensions.AllImage }
			});

		static FilePickerFileTypeImplementation PngFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.UWP, new[] { FileSystem.Extensions.Png } }
			});

		static FilePickerFileTypeImplementation JpegFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.UWP, FileSystem.Extensions.AllJpeg }
			});

		static FilePickerFileTypeImplementation VideoFileType() =>
		   new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
		   {
				{ DevicePlatform.UWP, FileSystem.Extensions.AllVideo }
		   });

		static FilePickerFileTypeImplementation PdfFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.UWP, new[] { FileSystem.Extensions.Pdf } }
			});
	}
}
