using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	partial class MediaPickerImplementation : IMediaPicker
	{
		public bool IsCaptureSupported =>
			throw new NotImplementedInReferenceAssemblyException();

		public Task<FileResult> PickPhotoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();

		public Task<FileResult> CapturePhotoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();

		public Task<FileResult> PickVideoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();

		public Task<FileResult> CaptureVideoAsync(MediaPickerOptions options) =>
			throw new NotImplementedInReferenceAssemblyException();

		public Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions options = null) =>
			throw new NotImplementedInReferenceAssemblyException();

		public Task<List<FileResult>> PickVideosAsync(MediaPickerOptions options = null) =>
			throw new NotImplementedInReferenceAssemblyException();
	}
}
