using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public class MapsGallery : ContentPage
	{
		public MapsGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Spacing = 5,
					Children =
					{
						GalleryBuilder.NavButton("Basic Map", () => new BasicMapGallery(), Navigation),
						GalleryBuilder.NavButton("Map Type", () => new MapTypeGallery(), Navigation),
						GalleryBuilder.NavButton("Map Pins", () => new MapPinsGallery(), Navigation),
						GalleryBuilder.NavButton("Pins ItemsSource", () => new PinItemsSourceGallery(), Navigation),
						GalleryBuilder.NavButton("Pin Clustering", () => new ClusteringGallery(), Navigation),
						GalleryBuilder.NavButton("Circle", () => new CircleGallery(), Navigation),
						GalleryBuilder.NavButton("Polygon", () => new PolygonsGallery(), Navigation),
					}
				}
			};
		}
	}
}
