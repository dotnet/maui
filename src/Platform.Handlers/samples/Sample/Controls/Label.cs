using Xamarin.Forms;
using Xamarin.Platform;

namespace Sample
{
	public class Label : View, ILabel
	{
		public Label()
		{

		}

		public string Text { get; set; }

		public Color TextColor { get; set; }

		public FontAttributes FontAttributes => throw new System.NotImplementedException();

		public string FontFamily => throw new System.NotImplementedException();

		public double FontSize => throw new System.NotImplementedException();
	}
}