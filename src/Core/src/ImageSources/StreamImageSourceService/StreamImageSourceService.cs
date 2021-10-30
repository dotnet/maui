#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService : ImageSourceService, IImageSourceService<IStreamImageSource>
	{
		public StreamImageSourceService()
			: this(null)
		{
		}

		public StreamImageSourceService(ILogger<StreamImageSourceService>? logger = null)
			: base(logger)
		{
		}
	}
}