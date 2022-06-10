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
					}
				}
			};
		}
	}
}
