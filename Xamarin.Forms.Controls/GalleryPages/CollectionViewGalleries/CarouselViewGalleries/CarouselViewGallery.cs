using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	internal class CarouselViewGallery : ContentPage
	{
		public CarouselViewGallery()
		{
			var descriptionLabel =
				new Label { Text = "CarouselView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "CarouselView Galleries";

			var button = new Button
			{
				Text = "Enable IndicatorView",
				AutomationId = "EnableIndicatorView"
			};
			button.Clicked += ButtonClicked;

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						button,
						GalleryBuilder.NavButton("CarouselView (Code, Horizontal)", () =>
							new CarouselCodeGallery(ItemsLayoutOrientation.Horizontal), Navigation),
						GalleryBuilder.NavButton("CarouselView (Code, Vertical)", () =>
							new CarouselCodeGallery(ItemsLayoutOrientation.Vertical), Navigation),
						GalleryBuilder.NavButton("CarouselView (XAML, Horizontal)", () =>
							new CarouselXamlGallery(), Navigation),
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
						GalleryBuilder.NavButton("CarouselView SetPosition Ctor No Animation", () =>
							new CarouselItemsGallery(useNativeIndicators: true, setPositionOnConstructor: true, useScrollAnimated: false), Navigation),
					}
				}
			};
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "IndicatorView Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.CarouselViewExperimental });
		}
	}
}