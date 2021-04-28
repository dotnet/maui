#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService : ImageSourceService, IImageSourceService<IUriImageSource>
	{
		public UriImageSourceService()
			: this(null)
		{
		}

		public UriImageSourceService(ILogger<UriImageSourceService>? logger = null)
			: base(logger)
		{
		}
	}
}