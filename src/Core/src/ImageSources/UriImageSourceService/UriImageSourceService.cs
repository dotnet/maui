#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService : ImageSourceService, IImageSourceService<IUriImageSource>
	{
		readonly UriImageLoader? _loader;

		public UriImageSourceService()
			: this(null)
		{
		}

		public UriImageSourceService(ILogger<UriImageSourceService>? logger = null)
			: base(logger)
		{
		}

		internal UriImageSourceService(ILogger<UriImageSourceService>? logger, UriImageLoader? loader)
			: base(logger)
		{
			_loader = loader;
		}

		/// <summary>
		/// The loader configured for the app, or the shared default loader.
		/// </summary>
		internal UriImageLoader Loader => _loader ?? UriImageLoader.GetCurrent();
	}
}
