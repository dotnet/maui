using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public partial interface IFontManager
	{
		FontFamily DefaultFontFamily { get; }

		FontFamily GetFontFamily(Font font);

		double GetFontSize(Font font, double defaultFontSize = 0);
	}
}