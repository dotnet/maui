using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.ScrollToGalleries;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
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