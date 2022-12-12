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
			var label = sender as Label;

			if (label == null)
				return;

			var rnd = new System.Random();
			GestureSpan.TextColor = Color.FromRgb((byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254));
		}

		void ChangeFormattedString_Clicked(object sender, System.EventArgs e)
		{
			labelFormattedString.FormattedText = new FormattedString
			{
				Spans =
				{
					new Span
					{
						Text = "Testing"
					}
				}
			};
		}
	}
}