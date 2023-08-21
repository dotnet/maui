//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.ItemSizeGalleries
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
							new DynamicItemSizeGallery(LinearItemsLayout.Vertical), Navigation),
						GalleryBuilder.NavButton("Expanding Text (Horizontal List)", () =>
							new DynamicItemSizeGallery(LinearItemsLayout.Horizontal), Navigation),
						GalleryBuilder.NavButton("ItemSizing Strategy", () =>
							new VariableSizeTemplateGridGallery (ItemsLayoutOrientation.Horizontal), Navigation),
						GalleryBuilder.NavButton("Chat Example (Randomly Sized Items)", () =>
							new ChatExample(), Navigation)
					}
				}
			};
		}
	}
}
