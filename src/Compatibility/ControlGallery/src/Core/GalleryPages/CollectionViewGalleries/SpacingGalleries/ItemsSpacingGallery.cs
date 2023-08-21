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
