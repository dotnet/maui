using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Controls.Sample.UITests;

namespace Maui.Controls.Sample.CollectionViewGalleries.SpacingGalleries
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
						TestBuilder.NavButton("Vertical List Spacing", () =>
							new SpacingGallery (LinearItemsLayout.Vertical), Navigation),
						TestBuilder.NavButton("Horizontal List Spacing", () =>
							new SpacingGallery (LinearItemsLayout.Horizontal), Navigation),
						TestBuilder.NavButton("Vertical Grid Spacing", () =>
							new SpacingGallery (new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)), Navigation),
						TestBuilder.NavButton("Horizontal Grid Spacing", () =>
							new SpacingGallery (new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal)), Navigation)
					}
				}
			};
		}
	}
}
