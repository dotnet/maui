using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{
		public bool PlatformIsCaptureSupported
			=> false;

		public async Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options)
			=> new FileResult(await FilePicker.PickAsync(new PickOptions
			{
				FileTypes = FilePickerFileType.Images
			}));

		public Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options)
			=> PlatformPickPhotoAsync(options);

		public async Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options)
			=> new FileResult(await FilePicker.PickAsync(new PickOptions
			{
				FileTypes = FilePickerFileType.Videos
			}));

		public Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options)
			=> PlatformPickVideoAsync(options);
	}
}
