namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.LayoutGalleries
{
	public class VerticalStackLayoutGallery : ContentPage
	{
		public VerticalStackLayoutGallery()
		{
			var layout = new VerticalStackLayout();

			for (int n = 0; n < 10; n++)
			{
				layout.Add(new Label { Text = $"Label {n} in a vertical stack" });
			}

			Content = layout;
		}
	}
}
