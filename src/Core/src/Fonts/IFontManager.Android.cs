using Android.Graphics;

namespace Microsoft.Maui
{
	public partial interface IFontManager
	{
		Typeface DefaultTypeface { get; }

		Typeface? GetTypeface(Font font);

		FontSize GetFontSize(Font font, float defaultFontSize = 0);
	}
}