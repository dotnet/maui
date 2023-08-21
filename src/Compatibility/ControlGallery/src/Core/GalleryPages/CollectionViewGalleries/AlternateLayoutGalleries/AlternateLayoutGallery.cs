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
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.ScrollModeGalleries;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.AlternateLayoutGalleries
{
	internal class AlternateLayoutGallery : ContentPage
	{
		public AlternateLayoutGallery()
		{
			var descriptionLabel =
					new Label { Text = "Alternate Layout Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Alternate Layout Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,

						GalleryBuilder.NavButton("Staggered Grid [Android only]", () =>
							new StaggeredLayout(), Navigation),

						GalleryBuilder.NavButton("ScrollTo Item (Staggered Grid, [Android only])", () =>
							new ScrollToCodeGallery(new StaggeredGridItemsLayout(3, ItemsLayoutOrientation.Vertical),
								ScrollToMode.Element, ExampleTemplates.RandomSizeTemplate, () => new StaggeredCollectionView()), Navigation),

						GalleryBuilder.NavButton("Scroll Mode (Staggered Grid, [Android only])", () =>
							new ScrollModeTestGallery(new StaggeredGridItemsLayout(3, ItemsLayoutOrientation.Vertical),
							ExampleTemplates.RandomSizeTemplate, () => new StaggeredCollectionView()), Navigation)
					}
				}
			};
		}
	}
}
