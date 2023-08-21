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

using System;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
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
							new SnapPointsCodeGallery(LinearItemsLayout.Horizontal as ItemsLayout), Navigation),
						GalleryBuilder.NavButton("Snap Points (Code, Vertical List)", () =>
							new SnapPointsCodeGallery(LinearItemsLayout.Vertical as ItemsLayout), Navigation),
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