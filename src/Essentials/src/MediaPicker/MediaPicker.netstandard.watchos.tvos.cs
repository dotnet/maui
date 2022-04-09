using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Media
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/MediaPicker.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MediaPicker']/Docs" />
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
	}
}
