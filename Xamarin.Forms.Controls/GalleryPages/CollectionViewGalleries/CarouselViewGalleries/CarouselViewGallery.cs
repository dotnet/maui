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

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						new Button { Text ="Enable CollectionView", AutomationId = "EnableCollectionView", Command = new Command(() => Device.SetFlags(new[] { ExperimentalFlags.CollectionViewExperimental })) },
						GalleryBuilder.NavButton("CarouselView (Code, Horizontal)", () =>
							new CarouselCodeGallery(ItemsLayoutOrientation.Horizontal), Navigation),
						GalleryBuilder.NavButton("CarouselView (Code, Vertical)", () =>
							new CarouselCodeGallery(ItemsLayoutOrientation.Vertical), Navigation),
						GalleryBuilder.NavButton("CarouselView (XAML, Horizontal)", () =>
							new CarouselXamlGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView (Items)", () =>
							new CarouselItemsGallery(), Navigation),
					}
				}
			};
		}
	}
}