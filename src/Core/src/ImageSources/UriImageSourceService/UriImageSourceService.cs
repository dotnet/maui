using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService : ImageSourceService, IImageSourceService<IUriImageSource>
	{
		public UriImageSourceService(ILogger<UriImageSourceService>? logger = null)
			: base(logger)
		{
		}
	}
}