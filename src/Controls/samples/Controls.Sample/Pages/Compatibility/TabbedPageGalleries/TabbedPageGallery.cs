using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.TabbedPageGalleries
{
	class TabbedPageGallery : ContentPage
	{
		public TabbedPageGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Spacing = 5,
					Children =
					{
						GalleryBuilder.NavButton("Basic TabbedPage", () => new BasicTabbedPageGallery(), Navigation),
						GalleryBuilder.NavButton("TabbedPage scroll conflicts Gallery", () => new TabbedPageScrollConflictsGallery(), Navigation),
					}
				}
			};
		}
	}
}