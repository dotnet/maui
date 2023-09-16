namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.LayoutGalleries
{
	public class HorizontalStackLayoutGallery : ContentPage
	{
		public HorizontalStackLayoutGallery()
		{
			var layout = new HorizontalStackLayout();

			for (int n = 0; n < 3; n++)
			{
				layout.Add(new Label { Text = $"Label {n} in a horizontal stack" });
			}

			Content = layout;
		}
	}
}
