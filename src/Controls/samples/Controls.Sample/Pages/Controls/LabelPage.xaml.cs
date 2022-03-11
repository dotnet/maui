using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class LabelPage
	{
		public LabelPage()
		{
			InitializeComponent();
		}
		void ClickGestureRecognizer_Clicked(System.Object sender, System.EventArgs e)
		{
			var rnd = new System.Random();

			var span = sender as Span;
			if (span != null)
				span.TextColor = Color.FromRgb((byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254));
		}
	}
}