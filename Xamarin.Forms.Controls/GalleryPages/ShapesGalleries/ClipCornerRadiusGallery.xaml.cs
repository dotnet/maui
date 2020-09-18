namespace Xamarin.Forms.Controls.GalleryPages.ShapesGalleries
{
	public partial class ClipCornerRadiusGallery : ContentPage
	{
		public ClipCornerRadiusGallery()
		{
			InitializeComponent();
		}

		void OnCornerChanged(object sender, ValueChangedEventArgs e)
		{
			RoundRectangleGeometry.CornerRadius = new CornerRadius(
				TopLeftCorner.Value,
				TopRightCorner.Value,
				BottomLeftCorner.Value,
				BottomRightCorner.Value);
		}
	}
}