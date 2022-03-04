#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService : ImageSourceService, IImageSourceService<IFileImageSource>
	{
		public FileImageSourceService()
			: this(null)
		{
		}

		public FileImageSourceService(ILogger<FileImageSourceService>? logger = null)
			: base(logger)
		{
		}
	}
}