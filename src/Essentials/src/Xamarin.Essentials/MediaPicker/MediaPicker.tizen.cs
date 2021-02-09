using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
	public static partial class MediaPicker
	{
		static bool PlatformIsCaptureSupported
			   => true;

		static async Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options)
			=> await FilePicker.PickAsync(new PickOptions
			{
				PickerTitle = options?.Title,
				FileTypes = FilePickerFileType.Images
			});

		static Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
			=> PlatformMediaAsync(options, true);

		static async Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options)
			=> await FilePicker.PickAsync(new PickOptions
			{
				PickerTitle = options?.Title,
				FileTypes = FilePickerFileType.Videos
			});

		static Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
			=> PlatformMediaAsync(options, false);

		static async Task<FileResult> PlatformMediaAsync(MediaPickerOptions options, bool photo)
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
