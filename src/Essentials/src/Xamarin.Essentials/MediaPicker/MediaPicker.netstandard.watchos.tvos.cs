using System;
using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
	public static partial class MediaPicker
	{
		static bool PlatformIsCaptureSupported =>
			throw new NotImplementedInReferenceAssemblyException();

		static Task<FileResult> PlatformPickPhotoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();

		static Task<FileResult> PlatformCapturePhotoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();

		static Task<FileResult> PlatformPickVideoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();

		static Task<FileResult> PlatformCaptureVideoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();
	}
}
