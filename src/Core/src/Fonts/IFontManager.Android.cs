using Android.Graphics;

namespace Microsoft.Maui
{
	public interface IFontManager
	{
		Typeface DefaultTypeface { get; }

		Typeface? GetTypeface(Font font);

		float GetFontSize(Font font, float defaultFontSize = 0);
	}
}