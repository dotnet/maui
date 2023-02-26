using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	partial class MediaGalleryImplementation
	{
		public bool IsSupported => false;

		public bool CheckCaptureSupport(MediaFileType type) => true;

		public Task<IEnumerable<MediaFileResult>> PlatformCaptureAsync(MediaFileType type, CancellationToken token = default)
		{
			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			await Permissions.EnsureGrantedAsync<Permissions.StorageRead>();

			var tcs = new TaskCompletionSource<FileResult>();

			var appControl = new AppControl();
			appControl.Operation = type == MediaFileType.Image ? AppControlOperations.ImageCapture : AppControlOperations.VideoCapture;
			appControl.LaunchMode = AppControlLaunchMode.Group;

			var appId = AppControl.GetMatchedApplicationIds(appControl)?.FirstOrDefault();

			if (!string.IsNullOrEmpty(appId))
				appControl.ApplicationId = appId;

			AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
			{
				if (result == AppControlReplyResult.Succeeded && reply.ExtraData.Count() > 0)
				{
					var file = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected)?.FirstOrDefault();
					tcs.TrySetResult(new FileResult(file));
				}
				else
				{
					tcs.TrySetCanceled();
				}
			});

			var res = await tcs.Task
			return res == null ? null : new [] { new MediaFileResult(res) };;
		}

		public Task<IEnumerable<MediaFileResult>> PlatformPickAsync(MediaPickRequest request, CancellationToken token = default)
		{
			List<string> defaultTypes = new();

			if (request.Types.Contains(MediaFileType.Image))
				defaultTypes.AddRange(FilePickerFileType.Images.Value);
			if (request.Types.Contains(MediaFileType.Video))
				defaultTypes.AddRange(FilePickerFileType.Videos.Value);
			
			var res = await FilePicker.PickAsync(new PickOptions
			{
				PickerTitle = options?.Title,
				FileTypes = new FilePickerFileType(
					new Dictionary<DevicePlatform, IEnumerable<string>>() {DevicePlatform.Tizen, defaultTypes})
			});
			return res == null ? null : new [] { new MediaFileResult(res) };
		}
		
		public MultiPickingBehaviour GetMultiPickingBehaviour()
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task PlatformSaveAsync(MediaFileType type, Stream fileStream, string fileName)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task PlatformSaveAsync(MediaFileType type, byte[] data, string fileName)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task PlatformSaveAsync(MediaFileType type, string filePath)
			=> throw new NotImplementedInReferenceAssemblyException();
		
	}
}
