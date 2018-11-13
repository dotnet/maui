using System;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class SnapPointsGallery : ContentPage
	{
		public SnapPointsGallery()
		{
			var descriptionLabel =
				new Label { Text = "Snap Points Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Snap Points Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Snap Points (Code, Horizontal List)", () =>
							new SnapPointsCodeGallery(ListItemsLayout.HorizontalList as ItemsLayout), Navigation),
						GalleryBuilder.NavButton("Snap Points (Code, Vertical List)", () =>
							new SnapPointsCodeGallery(ListItemsLayout.VerticalList as ItemsLayout), Navigation),
						GalleryBuilder.NavButton("Snap Points (Code, Horizontal Grid)", () =>
							new SnapPointsCodeGallery(new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal)), Navigation),
						GalleryBuilder.NavButton("Snap Points (Code, Vertical Grid)", () =>
							new SnapPointsCodeGallery(new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)), Navigation),
					}
				}
			};
		}
	}
}