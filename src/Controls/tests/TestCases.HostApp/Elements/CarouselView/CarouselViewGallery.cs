using Controls.Sample.UITests;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.CollectionViewGalleries.CarouselViewGalleries
{
	public class CarouselViewGallery : NavigationPage
	{
		public CarouselViewGallery() : base(new CarouselViewGalleries())
		{
		}
		
		internal class CarouselViewGalleries : ContentPage
		{
			public CarouselViewGalleries()
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
							TestBuilder.NavButton("CarouselView", () =>
								new CarouselViewPage(), Navigation),
							TestBuilder.NavButton("CarouselView With Images", () =>
								new CarouselViewPageWithImages(), Navigation),
							TestBuilder.NavButton("CarouselView (Code, Horizontal)", () =>
								new CarouselCodeGallery(ItemsLayoutOrientation.Horizontal), Navigation),
							TestBuilder.NavButton("CarouselView (Code, Vertical)", () =>
								new CarouselCodeGallery(ItemsLayoutOrientation.Vertical), Navigation),
							TestBuilder.NavButton("CarouselView (XAML, Horizontal)", () =>
								new CarouselXamlGallery(false), Navigation),
							TestBuilder.NavButton("CarouselView (XAML, Horizontal, Loop)", () =>
								new CarouselXamlGallery(true), Navigation),
							TestBuilder.NavButton("CarouselView (Indicators Forms)", () =>
								new CarouselItemsGallery(), Navigation),
							TestBuilder.NavButton("CarouselView (Indicators Default (Native))", () =>
								new CarouselItemsGallery(useNativeIndicators: true), Navigation),
							TestBuilder.NavButton("CarouselView Async", () =>
								new CarouselItemsGallery(setCollectionWithAsync:true, useNativeIndicators: true), Navigation),
							TestBuilder.NavButton("CarouselView Snap", () =>
								new CarouselSnapGallery(), Navigation),
							TestBuilder.NavButton("ObservableCollection and CarouselView", () =>
								new CollectionCarouselViewGallery(), Navigation),
							TestBuilder.NavButton("CarouselView EmptyView", () =>
								new EmptyCarouselGallery(), Navigation),
							TestBuilder.NavButton("IndicatorView", () =>
								new IndicatorCodeGallery(), Navigation),
							TestBuilder.NavButton("CarouselView SetPosition Ctor", () =>
								new CarouselItemsGallery(useNativeIndicators: true, setPositionOnConstructor: true), Navigation),
							TestBuilder.NavButton("CarouselView SetPosition Appearing", () =>
								new CarouselItemsGallery(useNativeIndicators: true, setPositionOnAppearing: true), Navigation),
							TestBuilder.NavButton("CarouselView loop", () =>
								new CarouselXamlGallery(true), Navigation),
							TestBuilder.NavButton("CarouselView Set CurrentItem", () =>
								new CarouselXamlGallery(false, 3), Navigation),
							TestBuilder.NavButton("CarouselView Set CurrentItem Loop", () =>
								new CarouselXamlGallery(true, 3), Navigation),
						}
					}
				};
			}
		}
	}
	
}