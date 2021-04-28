#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService : ImageSourceService, IImageSourceService<IFileImageSource>
	{
		public FileImageSourceService()
			: this(null, null)
		{
		}

		public FileImageSourceService(IImageSourceServiceConfiguration? configuration = null, ILogger<FileImageSourceService>? logger = null)
			: base(logger)
		{
			Configuration = configuration;
		}

		public IImageSourceServiceConfiguration? Configuration { get; }
	}
}