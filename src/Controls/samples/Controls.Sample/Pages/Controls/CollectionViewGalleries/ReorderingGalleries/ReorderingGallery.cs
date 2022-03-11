using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.ReorderingGalleries
{
	internal class ReorderingGallery : ContentPage
	{
		public ReorderingGallery()
		{
			var descriptionLabel =
				new Label { Text = "Reordering Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Reordering Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Vertical List Reordering", () =>
							new UngroupedReorderingGallery (LinearItemsLayout.Vertical), Navigation),
						GalleryBuilder.NavButton("Horizontal List Reordering", () =>
							new UngroupedReorderingGallery (LinearItemsLayout.Horizontal), Navigation),
						GalleryBuilder.NavButton("Vertical Grid Reordering", () =>
							new UngroupedReorderingGallery (new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)), Navigation),
						GalleryBuilder.NavButton("Horizontal Grid Reordering", () =>
							new UngroupedReorderingGallery (new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal)), Navigation),
						GalleryBuilder.NavButton("Grouped List Reordering", () =>
							new GroupedReorderingGallery (LinearItemsLayout.Vertical), Navigation),
						GalleryBuilder.NavButton("Grouped Grid Reordering", () =>
							new GroupedReorderingGallery (new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)), Navigation)

					}
				}
			};
		}
	}
}