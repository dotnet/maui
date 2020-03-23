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
							new CarouselItemsGallery(false,false,false), Navigation),
						GalleryBuilder.NavButton("CarouselView (Indicators Default (Native))", () =>
							new CarouselItemsGallery(false,false,true), Navigation),
						GalleryBuilder.NavButton("CarouselView Async", () =>
							new CarouselItemsGallery(false,true,true), Navigation),
	  					GalleryBuilder.NavButton("CarouselView Snap", () =>
 							new CarouselSnapGallery(), Navigation),
						GalleryBuilder.NavButton("ObservableCollection and CarouselView", () =>
 							new CollectionCarouselViewGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView EmptyView", () =>
  							new EmptyCarouselGallery(), Navigation),
						GalleryBuilder.NavButton("IndicatorView", () =>
  							new IndicatorCodeGallery(), Navigation)
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

			Device.SetFlags(new[] { ExperimentalFlags.CarouselViewExperimental, ExperimentalFlags.IndicatorViewExperimental });
		}
	}
}