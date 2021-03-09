using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;

namespace Microsoft.Maui.Essentials
{
	public static partial class FilePicker
	{
		static async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
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
	}

	public partial class FilePickerFileType
	{
		static FilePickerFileType PlatformImageFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.ImagePng, FileSystem.MimeTypes.ImageJpg } }
			});

		static FilePickerFileType PlatformPngFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.ImagePng } }
			});

		static FilePickerFileType PlatformJpegFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.ImageJpg } }
			});

		static FilePickerFileType PlatformVideoFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.VideoAll } }
			});

		static FilePickerFileType PlatformPdfFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileSystem.MimeTypes.Pdf } }
			});
	}
}
