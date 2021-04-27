#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IFontImageSource : IImageSource
	{
		Color Color { get; }

		Font Font { get; }

		string Glyph { get; }
	}
}