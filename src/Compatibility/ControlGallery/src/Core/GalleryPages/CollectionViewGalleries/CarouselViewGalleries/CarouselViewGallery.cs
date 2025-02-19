using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	internal class CarouselViewGallery : ContentPage
	{
		public CarouselViewGallery()
		{
			var descriptionLabel =
				new Label { Text = "CarouselView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "CarouselView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("CarouselView (Code, Horizontal)", () =>
							new CarouselCodeGallery(ItemsLayoutOrientation.Horizontal), Navigation),
						GalleryBuilder.NavButton("CarouselView (Code, Vertical)", () =>
							new CarouselCodeGallery(ItemsLayoutOrientation.Vertical), Navigation),
						GalleryBuilder.NavButton("CarouselView (XAML, Horizontal)", () =>
							new CarouselXamlGallery(false), Navigation),
						GalleryBuilder.NavButton("CarouselView (XAML, Horizontal, Loop)", () =>
							new CarouselXamlGallery(true), Navigation),
						GalleryBuilder.NavButton("CarouselView (Indicators Forms)", () =>
							new CarouselItemsGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView (Indicators Default (Native))", () =>
							new CarouselItemsGallery(useNativeIndicators: true), Navigation),
						GalleryBuilder.NavButton("CarouselView Async", () =>
							new CarouselItemsGallery(setCollectionWithAsync:true, useNativeIndicators: true), Navigation),
	  					GalleryBuilder.NavButton("CarouselView Snap", () =>
 							new CarouselSnapGallery(), Navigation),
						GalleryBuilder.NavButton("ObservableCollection and CarouselView", () =>
 							new CollectionCarouselViewGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView EmptyView", () =>
  							new EmptyCarouselGallery(), Navigation),
						GalleryBuilder.NavButton("IndicatorView", () =>
  							new IndicatorCodeGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView SetPosition Ctor", () =>
							new CarouselItemsGallery(useNativeIndicators: true, setPositionOnConstructor: true), Navigation),
						GalleryBuilder.NavButton("CarouselView SetPosition Appearing", () =>
							new CarouselItemsGallery(useNativeIndicators: true, setPositionOnAppearing: true), Navigation),
						GalleryBuilder.NavButton("CarouselView loop", () =>
							new CarouselXamlGallery(true), Navigation),
						GalleryBuilder.NavButton("CarouselView Set CurrentItem", () =>
							new CarouselXamlGallery(false, 3), Navigation),
						GalleryBuilder.NavButton("CarouselView Set CurrentItem Loop", () =>
							new CarouselXamlGallery(true, 3), Navigation),
					}
				}
			};
		}
	}
}