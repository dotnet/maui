namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.LayoutGalleries
{
	public class LayoutGallery : ContentPage
	{
		public LayoutGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						GalleryBuilder.NavButton("VerticalStackLayout Gallery", () => new VerticalStackLayoutGallery(), Navigation),
						GalleryBuilder.NavButton("HorizontalStackLayout Gallery", () => new HorizontalStackLayoutGallery(), Navigation),
					}
				}
			};
		}
	}
}
