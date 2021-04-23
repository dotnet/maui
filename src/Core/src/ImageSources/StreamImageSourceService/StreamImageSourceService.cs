using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService : ImageSourceService, IImageSourceService<IStreamImageSource>
	{
		public StreamImageSourceService(ILogger? logger = null)
			: base(logger)
		{
		}
	}
}