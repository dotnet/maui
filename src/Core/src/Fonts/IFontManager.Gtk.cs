using Pango;

namespace Microsoft.Maui
{
	public interface IFontManager
	{
	
		FontDescription DefaultFontFamily { get; }

		double DefaultFontSize { get; }

		FontDescription GetFontFamily(Font font);

		double GetFontSize(Font font);
	}
}