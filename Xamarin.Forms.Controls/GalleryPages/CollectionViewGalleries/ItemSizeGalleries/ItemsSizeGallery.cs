namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.ItemSizeGalleries
{
	internal class ItemsSizeGallery : ContentPage
	{
		public ItemsSizeGallery()
		{
			var descriptionLabel =
				new Label { Text = "Item Size Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Item Size Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Expanding Text (Vertical List)", () =>
							new DynamicItemSizeGallery(ListItemsLayout.VerticalList), Navigation),
						GalleryBuilder.NavButton("Expanding Text (Horizontal List)", () =>
							new DynamicItemSizeGallery(ListItemsLayout.HorizontalList), Navigation),
						GalleryBuilder.NavButton("ItemSizing Strategy", () =>
							new VariableSizeTemplateGridGallery (ItemsLayoutOrientation.Horizontal), Navigation)
					}
				}
			};
		}
	}
}
