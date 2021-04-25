using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FontImageSourceService : ImageSourceService, IImageSourceService<IFontImageSource>
	{
		public FontImageSourceService(IFontManager fontManager, ILogger<FontImageSourceService>? logger = null)
			: base(logger)
		{
			FontManager = fontManager;
		}

		public IFontManager FontManager { get; }
	}
}