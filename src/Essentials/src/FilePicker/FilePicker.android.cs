using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FilePickerImplementation : IFilePicker
	{
		public async Task<IEnumerable<FileResult>> PickAsync(PickOptions options, bool allowMultiple = false)
		{
			// we only need the permission when accessing the file, but it's more natural
			// to ask the user first, then show the picker.
			await Permissions.EnsureGrantedAsync<Permissions.StorageRead>();

			// Essentials supports >= API 19 where this action is available
			var action = Intent.ActionOpenDocument;

			var intent = new Intent(action);
			intent.SetType(FileSystem.MimeTypes.All);
			intent.PutExtra(Intent.ExtraAllowMultiple, allowMultiple);

			var allowedTypes = options?.FileTypes?.Value?.ToArray();
			if (allowedTypes?.Length > 0)
				intent.PutExtra(Intent.ExtraMimeTypes, allowedTypes);

			var pickerIntent = Intent.CreateChooser(intent, options?.PickerTitle ?? "Select file");

			try
			{
				var resultList = new List<FileResult>();
				void OnResult(Intent intent)
				{
					// The uri returned is only temporary and only lives as long as the Activity that requested it,
					// so this means that it will always be cleaned up by the time we need it because we are using
					// an intermediate activity.

					if (intent.ClipData == null)
					{
						var path = FileSystem.EnsurePhysicalPath(intent.Data);
						resultList.Add(new FileResult(path));
					}
					else
					{
						for (var i = 0; i < intent.ClipData.ItemCount; i++)
						{
							var uri = intent.ClipData.GetItemAt(i).Uri;
							var path = FileSystem.EnsurePhysicalPath(uri);
							resultList.Add(new FileResult(path));
						}
					}
				}

				await IntermediateActivity.StartAsync(pickerIntent, Platform.requestCodeFilePicker, onResult: OnResult);

				return resultList;
			}
			catch (OperationCanceledException)
			{
				return null;
			}
		}

		public async Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options)
		{
			return await PickAsync(options, true);
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
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.ImagePng, FileSystem.MimeTypes.ImageJpg } }
			});

		public IFilePickerFileType PngFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.ImagePng } }
			});

		public IFilePickerFileType JpegFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.ImageJpg } }
			});

		public IFilePickerFileType VideoFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.VideoAll } }
			});

		public IFilePickerFileType PdfFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.Pdf } }
			});
	}
}
