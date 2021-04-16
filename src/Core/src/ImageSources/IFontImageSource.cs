using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IFontImageSource : IImageSource
	{
		Color Color { get; }

		string FontFamily { get; }

		string Glyph { get; }

		double Size { get; }
	}
}