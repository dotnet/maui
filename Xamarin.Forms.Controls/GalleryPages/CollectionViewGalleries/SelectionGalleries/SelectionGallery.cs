namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	internal class SelectionGallery : ContentPage
	{
		public SelectionGallery()
		{
			var descriptionLabel =
				new Label { Text = "Selection Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Selection Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Selection Modes", () =>
							new SelectionModeGallery(), Navigation),
						GalleryBuilder.NavButton("Preselected Item", () =>
							new PreselectedItemGallery(), Navigation),
						GalleryBuilder.NavButton("Preselected Items", () =>
							new PreselectedItemsGallery(), Navigation),
						GalleryBuilder.NavButton("Single Selection, Bound", () =>
							new SingleBoundSelection(), Navigation),
						GalleryBuilder.NavButton("Multiple Selection, Bound", () =>
							new MultipleBoundSelection(), Navigation),
					}
				}
			};
		}
	}
}