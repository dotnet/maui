using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Media
{
	partial class MediaGalleryImplementation
	{
		public bool IsSupported => false;

		public bool CheckCaptureSupport(MediaFileType type) => throw new NotImplementedInReferenceAssemblyException();

		public Task<IEnumerable<MediaFileResult>> PlatformCaptureAsync(MediaFileType type, CancellationToken token = default)
			=> throw new NotImplementedInReferenceAssemblyException();

		public Task<IEnumerable<MediaFileResult>> PlatformPickAsync(MediaPickRequest request, CancellationToken token = default)
			=> throw new NotImplementedInReferenceAssemblyException();
		
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
