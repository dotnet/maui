namespace Microsoft.Maui
{
	public partial class FontImageSourceService : IImageSourceService<IFontImageSource>
	{
		public FontImageSourceService(IFontManager fontManager)
		{
			FontManager = fontManager;
		}

		public IFontManager FontManager { get; }
	}
}