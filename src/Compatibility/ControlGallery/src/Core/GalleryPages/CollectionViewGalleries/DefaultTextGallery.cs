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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class DefaultTextGallery : ContentPage
	{
		public DefaultTextGallery()
		{
			var descriptionLabel = new Label
			{
				Text = "No DataTemplates; just using the ToString() of the objects in the source.",
				Margin = new Thickness(2, 2, 2, 2)
			};

			Title = "Default Text Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						// TODO hartez 2018-06-05 10:43 AM Need a gallery page which allows layout selection
						// so we can demonstrate switching between them
						descriptionLabel,
						GalleryBuilder.NavButton("Vertical List (Code)", () =>
							new TextCodeCollectionViewGallery(LinearItemsLayout.Vertical), Navigation),
						GalleryBuilder.NavButton("Horizontal List (Code)", () =>
							new TextCodeCollectionViewGallery(LinearItemsLayout.Horizontal), Navigation),
						GalleryBuilder.NavButton("Vertical Grid (Code)", () =>
							new TextCodeCollectionViewGridGallery(), Navigation),
						GalleryBuilder.NavButton("Horizontal Grid (Code)", () =>
							new TextCodeCollectionViewGridGallery(ItemsLayoutOrientation.Horizontal), Navigation),
					}
				}
			};
		}
	}
}