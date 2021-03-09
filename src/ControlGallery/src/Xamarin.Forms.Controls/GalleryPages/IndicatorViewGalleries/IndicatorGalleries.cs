namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	public class IndicatorGalleries : ContentPage
	{
		public IndicatorGalleries()
		{
			var descriptionLabel =
				   new Label { Text = "IndicatorView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "IndicatorView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("IndicatorView Gallery", () =>
							new IndicatorsSample(), Navigation),
						GalleryBuilder.NavButton("Indicator MaxVisible Gallery", () =>
							new IndicatorsSampleMaximumVisible(), Navigation),
						GalleryBuilder.NavButton("Indicator VisualState", () =>
							new IndicatorsTemplateSample(), Navigation)
					}
				}
			};
		}
	}
}