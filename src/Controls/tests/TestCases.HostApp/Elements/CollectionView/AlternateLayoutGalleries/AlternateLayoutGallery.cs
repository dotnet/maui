namespace Maui.Controls.Sample.CollectionViewGalleries.AlternateLayoutGalleries
{
	internal class AlternateLayoutGallery : ContentPage
	{
		public AlternateLayoutGallery()
		{
			var descriptionLabel =
					new Label { Text = "Alternate Layout Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Alternate Layout Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,

						//TestBuilder.NavButton("Staggered Grid [Android only]", () =>
						//	new StaggeredLayout(), Navigation),

						//TestBuilder.NavButton("ScrollTo Item (Staggered Grid, [Android only])", () =>
						//	new ScrollToCodeGallery(new StaggeredGridItemsLayout(3, ItemsLayoutOrientation.Vertical),
						//		ScrollToMode.Element, ExampleTemplates.RandomSizeTemplate, () => new StaggeredCollectionView()), Navigation),

						//TestBuilder.NavButton("Scroll Mode (Staggered Grid, [Android only])", () =>
						//	new ScrollModeTestGallery(new StaggeredGridItemsLayout(3, ItemsLayoutOrientation.Vertical),
						//	ExampleTemplates.RandomSizeTemplate, () => new StaggeredCollectionView()), Navigation)
					}
				}
			};
		}
	}
}
