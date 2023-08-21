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

using Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.ScrollToGalleries;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class ScrollToGallery : ContentPage
	{
		public ScrollToGallery()
		{
			var descriptionLabel =
				new Label { Text = "ScrollTo Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "ScrollTo Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("ScrollTo Index (Code, Horizontal List)", () =>
							new ScrollToCodeGallery(LinearItemsLayout.Horizontal), Navigation),
						GalleryBuilder.NavButton("ScrollTo Index (Code, Vertical List)", () =>
							new ScrollToCodeGallery(LinearItemsLayout.Vertical), Navigation),
						GalleryBuilder.NavButton("ScrollTo Index (Code, Horizontal Grid)", () =>
								new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal)),
							Navigation),
						GalleryBuilder.NavButton("ScrollTo Index (Code, Vertical Grid)", () =>
								new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)),
							Navigation),

						GalleryBuilder.NavButton("ScrollTo Item (Code, Horizontal List)", () =>
							new ScrollToCodeGallery(LinearItemsLayout.Horizontal, ScrollToMode.Element,
								ExampleTemplates.ScrollToItemTemplate), Navigation),
						GalleryBuilder.NavButton("ScrollTo Item (Code, Vertical List)", () =>
							new ScrollToCodeGallery(LinearItemsLayout.Vertical, ScrollToMode.Element,
								ExampleTemplates.ScrollToItemTemplate), Navigation),
						GalleryBuilder.NavButton("ScrollTo Item (Code, Horizontal Grid)", () =>
							new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal),
								ScrollToMode.Element, ExampleTemplates.ScrollToItemTemplate), Navigation),
						GalleryBuilder.NavButton("ScrollTo Item (Code, Vertical Grid)", () =>
							new ScrollToCodeGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Vertical),
								ScrollToMode.Element, ExampleTemplates.ScrollToItemTemplate), Navigation),


						GalleryBuilder.NavButton("ScrollTo Index (Grouped)", () =>
							new ScrollToGroup(), Navigation)


					}
				}
			};
		}
	}
}