namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		readonly IFontRegistrar _fontRegistrar;

		public FontManager(IFontRegistrar fontRegistrar)
		{
			_fontRegistrar = fontRegistrar;
		}
	}
}