namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.SpacingGalleries
{
	internal class ItemsSpacingGallery : ContentPage
	{
		public ItemsSpacingGallery()
		{
			var descriptionLabel =
				new Label { Text = "Item Spacing Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Item Spacing Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Vertical List Spacing", () =>
							new SpacingGallery (LinearItemsLayout.Vertical), Navigation),
						GalleryBuilder.NavButton("Horizontal List Spacing", () =>
							new SpacingGallery (LinearItemsLayout.Horizontal), Navigation),
						GalleryBuilder.NavButton("Vertical Grid Spacing", () =>
							new SpacingGallery (new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)), Navigation),
						GalleryBuilder.NavButton("Horizontal Grid Spacing", () =>
							new SpacingGallery (new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal)), Navigation)
					}
				}
			};
		}
	}
}
