using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class MediaPicker
	{
		static bool PlatformIsCaptureSupported
			=> false;

		static async Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options)
			=> new FileResult(await FilePicker.PickAsync(new PickOptions
			{
				FileTypes = FilePickerFileType.Images
			}));

		static Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
			=> PlatformPickPhotoAsync(options);

		static async Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options)
			=> new FileResult(await FilePicker.PickAsync(new PickOptions
			{
				FileTypes = FilePickerFileType.Videos
			}));

		static Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
			=> PlatformPickVideoAsync(options);
	}
}
