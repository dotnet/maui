using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Environment = Android.OS.Environment;

namespace Microsoft.Maui.Storage
{
	partial class FilePickerImplementation : IFilePicker
	{
		async Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions options, bool allowMultiple = false)
		{
			// Essentials supports >= API 19 where this action is available
			var action = Intent.ActionOpenDocument;

			var intent = new Intent(action);
			intent.SetType(FileMimeTypes.All);
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
					bool requireExtendedAccess = !(OperatingSystem.IsAndroidVersionAtLeast(30) && Environment.IsExternalStorageManager);

					if (intent.ClipData == null)
					{
						var path = FileSystemUtils.EnsurePhysicalPath(intent.Data, requireExtendedAccess);
						resultList.Add(new FileResult(path));
					}
					else
					{
						for (var i = 0; i < intent.ClipData.ItemCount; i++)
						{
							var uri = intent.ClipData.GetItemAt(i).Uri;
							var path = FileSystemUtils.EnsurePhysicalPath(uri, requireExtendedAccess);
							resultList.Add(new FileResult(path));
						}
					}
				}

				await IntermediateActivity.StartAsync(pickerIntent, PlatformUtils.requestCodeFilePicker, onResult: OnResult);

				return resultList;
			}
			catch (OperationCanceledException)
			{
				return [];
			}
		}
	}

	public partial class FilePickerFileType
	{
		static FilePickerFileType PlatformImageFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileMimeTypes.ImagePng, FileMimeTypes.ImageJpg } }
			});

		static FilePickerFileType PlatformPngFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileMimeTypes.ImagePng } }
			});

		static FilePickerFileType PlatformJpegFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileMimeTypes.ImageJpg } }
			});

		static FilePickerFileType PlatformVideoFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileMimeTypes.VideoAll } }
			});

		static FilePickerFileType PlatformPdfFileType() =>
			new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Android, new[] { FileMimeTypes.Pdf } }
			});
	}
}
