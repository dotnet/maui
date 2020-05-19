using System;

namespace System.Maui.Controls.GalleryPages
{
	public class IndicatorGalleries : ContentPage
	{
		public IndicatorGalleries()
		{
			var descriptionLabel =
				   new Label { Text = "IndicatorView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "IndicatorView Galleries";

			var button = new Button
			{
				Text = "Enable IndicatorView",
				AutomationId = "EnableIndicator"
			};
			button.Clicked += ButtonClicked;

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						button,
						GalleryBuilder.NavButton("IndicatorView Gallery", () =>
							new IndicatorsSample(), Navigation),
						GalleryBuilder.NavButton("Indicator MaxVisible Gallery", () =>
							new IndicatorsSampleMaximumVisible(), Navigation)
					}
				}
			};
		}

		void ButtonClicked(object sender, EventArgs e)
		{
			var button = sender as Button;

			button.Text = "IndicatorView Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.IndicatorViewExperimental });
		}
	}
}