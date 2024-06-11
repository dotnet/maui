using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class DoubleTapGallery : Microsoft.Maui.Controls.ContentView
	{
		public DoubleTapGallery()
		{
			AutomationId = "DoubleTapGallery";

			var layout = new VerticalStackLayout() { Margin = 10, Spacing = 10 };

			var result = new Label() { AutomationId = "DoubleTapResults" };

			var tapSurface = new Grid()
			{
				HeightRequest = 200,
				WidthRequest = 200,
				BackgroundColor = Microsoft.Maui.Graphics.Colors.AliceBlue
			};

			var dtLabel = new Label { Text = "DoubleTapSurface", AutomationId = "DoubleTapSurface" };
			tapSurface.Add(dtLabel);

			var doubleTapRecognizer = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };
			doubleTapRecognizer.Tapped += (sender, args) => { result.Text = "Success"; };

			tapSurface.GestureRecognizers.Add(doubleTapRecognizer);

			layout.Add(tapSurface);
			layout.Add(result);

			Content = layout;
		}
	}
}

