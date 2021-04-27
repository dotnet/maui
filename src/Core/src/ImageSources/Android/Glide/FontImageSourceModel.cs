#nullable enable
using Android.Graphics;

namespace Microsoft.Maui.BumptechGlide
{
	// TODO: make this public and do it better
	class FontImageSourceModel : Java.Lang.Object, FontImageSourceService.IModel
	{
		public FontImageSourceModel(string glyph, float textSize, Typeface? typeface, Color color)
		{
			Glyph = glyph;
			TextSize = textSize;
			Typeface = typeface;
			Color = color;
		}

		public string Glyph { get; }

		public float TextSize { get; }

		public Typeface? Typeface { get; }

		public Color Color { get; }

		public override string ToString() =>
			$"FontImageSourceModel{{Glyph={Glyph}, TextSize={TextSize}, Typeface={Typeface}, Color={Color}}}";
	}
}