using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService : ImageSourceService, IImageSourceService<IFileImageSource>
	{
		public FileImageSourceService(ILogger? logger = null)
			: base(logger)
		{
		}
	}
}