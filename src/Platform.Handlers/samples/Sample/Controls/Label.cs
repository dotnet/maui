using Xamarin.Forms;
using Xamarin.Platform;

namespace Sample
{
	public class Label : Xamarin.Forms.View, ILabel
	{
		public string Text { get; set; }

		public Color TextColor { get; set; }

		public Font Font { get; set; }

		public string FontFamily { get; set; }

		public double FontSize { get; set; }

		public FontAttributes FontAttributes { get; set; }

		public TextTransform TextTransform { get; set; }

		public TextAlignment HorizontalTextAlignment { get; set; }

		public TextAlignment VerticalTextAlignment { get; set; }

		public double CharacterSpacing { get; set; }
	}
}