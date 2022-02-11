using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tizen;
using Tizen.Applications;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class FilePickerImplementation : IFilePicker
	{
		public async Task<IEnumerable<FileResult>> PickAsync(PickOptions options, bool allowMultiple = false)
		{
			Permissions.EnsureDeclared<Permissions.LaunchApp>();
			await Permissions.EnsureGrantedAsync<Permissions.StorageRead>();

			var tcs = new TaskCompletionSource<IEnumerable<FileResult>>();

			var appControl = new AppControl();
			appControl.Operation = AppControlOperations.Pick;
			appControl.ExtraData.Add(AppControlData.SectionMode, allowMultiple ? "multiple" : "single");
			appControl.LaunchMode = AppControlLaunchMode.Single;

			var fileType = options?.FileTypes?.Value?.FirstOrDefault();
			appControl.Mime = fileType ?? FileSystem.MimeTypes.All;

			var fileResults = new List<FileResult>();

			AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
			{
				if (result == AppControlReplyResult.Succeeded)
				{
					if (reply.ExtraData.Count() > 0)
					{
						var selectedFiles = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected).ToList();
						fileResults.AddRange(selectedFiles.Select(f => new FileResult(f)));
					}
				}

				tcs.TrySetResult(fileResults);
			});

			return await tcs.Task;
		}

		public async Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions options)
		{
			return await PickAsync(options, true);
		}
	}

	public partial class FilePickerFileTypeImplementation : IFilePickerFileType
	{
		static FilePickerFileTypeImplementation ImageFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.ImageAll } },
			});

		static FilePickerFileTypeImplementation PngFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.ImagePng } }
			});

		static FilePickerFileTypeImplementation JpegFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.ImageJpg } }
			});

		static FilePickerFileTypeImplementation VideoFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.VideoAll } }
			});

		static FilePickerFileTypeImplementation PdfFileType() =>
			new FilePickerFileTypeImplementation(new Dictionary<DevicePlatform, IEnumerable<string>>
			{
				{ DevicePlatform.Tizen, new[] { FileSystem.MimeTypes.Pdf } }
			});
	}
}
