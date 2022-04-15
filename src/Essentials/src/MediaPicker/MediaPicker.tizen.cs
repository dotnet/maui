using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Tizen.Applications;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{
		public bool IsCaptureSupported
			=> true;

		public async Task<FileResult> PickPhotoAsync(MediaPickerOptions options)
			=> await FilePicker.PickAsync(new PickOptions
			{
				PickerTitle = options?.Title,
				FileTypes = FilePickerFileType.Images
			});

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options)
			=> MediaAsync(options, true);

		public async Task<FileResult> PickVideoAsync(MediaPickerOptions options)
			=> await FilePicker.PickAsync(new PickOptions
			{
				PickerTitle = options?.Title,
				FileTypes = FilePickerFileType.Videos
			});

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options)
			=> MediaAsync(options, false);

		public async Task<FileResult> MediaAsync(MediaPickerOptions options, bool photo)
		{
			Permissions.EnsureDeclared<Permissions.LaunchApp>();

			await Permissions.EnsureGrantedAsync<Permissions.StorageRead>();

			var tcs = new TaskCompletionSource<FileResult>();

			var appControl = new AppControl();
			appControl.Operation = photo ? AppControlOperations.ImageCapture : AppControlOperations.VideoCapture;
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

			return await tcs.Task;
		}
	}
}
