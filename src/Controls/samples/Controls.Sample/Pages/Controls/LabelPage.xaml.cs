using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class LabelPage
	{
		public LabelPage()
		{
			InitializeComponent();

			BindingContext = new LabelViewModel();
		}

		void ClickGestureRecognizer_Clicked(object sender, System.EventArgs e)
		{
			var rnd = new System.Random();

			if (sender is Span span)
				span.TextColor = Color.FromRgb((byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254), (byte)rnd.Next(0, 254));
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