using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	partial class MediaGalleryImplementation : IMediaGallery
	{
		public bool IsSupported => false;

		public bool CheckCaptureSupport(MediaFileType type) => throw new NotImplementedInReferenceAssemblyException();

		public Task<FileResult> CaptureAsync(MediaFileType type, CancellationToken token = default)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task<FileResult> PickAsync(int selectionLimit = 1, params MediaFileType[] types)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task<FileResult> PickAsync(MediaPickRequest request, CancellationToken token = default)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task SaveAsync(MediaFileType type, Stream fileStream, string fileName)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task SaveAsync(MediaFileType type, byte[] data, string fileName)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task SaveAsync(MediaFileType type, string filePath)
			=> throw new NotImplementedInReferenceAssemblyException();
	}
}
