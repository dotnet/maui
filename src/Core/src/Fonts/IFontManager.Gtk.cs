using Pango;

namespace Microsoft.Maui
{
	public partial interface IFontManager
	{
	
		FontDescription DefaultFontFamily { get; }

		FontDescription GetFontFamily(Font font);

		double GetFontSize(Font font);
	}
}