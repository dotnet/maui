using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public interface IFontManager
	{
		FontFamily DefaultFontFamily { get; }

		double DefaultFontSize { get; }

		FontFamily GetFontFamily(Font font);

		double GetFontSize(Font font, double defaultFontSize = 0);
	}
}