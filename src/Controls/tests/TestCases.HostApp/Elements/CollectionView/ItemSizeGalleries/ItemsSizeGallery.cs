using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Controls.Sample.UITests;

namespace Maui.Controls.Sample.CollectionViewGalleries.ItemSizeGalleries
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
						TestBuilder.NavButton("Expanding Text (Vertical List)", () =>
							new DynamicItemSizeGallery(LinearItemsLayout.Vertical), Navigation),
						TestBuilder.NavButton("Expanding Text (Horizontal List)", () =>
							new DynamicItemSizeGallery(LinearItemsLayout.Horizontal), Navigation),
						TestBuilder.NavButton("ItemSizing Strategy", () =>
							new VariableSizeTemplateGridGallery (ItemsLayoutOrientation.Horizontal), Navigation),
						TestBuilder.NavButton("Chat Example (Randomly Sized Items)", () =>
							new ChatExample(), Navigation)
					}
				}
			};
		}
	}
}
