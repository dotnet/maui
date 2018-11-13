using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	public class CollectionViewGallery : ContentPage
	{
		public CollectionViewGallery()
		{
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Default Text Galleries", () => new DefaultTextGallery(), Navigation),
					GalleryBuilder.NavButton("DataTemplate Galleries", () => new DataTemplateGallery(), Navigation),
					GalleryBuilder.NavButton("Observable Collection Galleries", () => new ObservableCollectionGallery(), Navigation),
					GalleryBuilder.NavButton("Snap Points Galleries", () => new SnapPointsGallery(), Navigation),
					GalleryBuilder.NavButton("ScrollTo Galleries", () => new ScrollToGallery(), Navigation),
					GalleryBuilder.NavButton("CarouselView Galleries", () => new CarouselViewGallery(), Navigation),
					GalleryBuilder.NavButton("EmptyView Galleries", () => new EmptyViewGallery(), Navigation),
				}
			};
		}
	}
}
