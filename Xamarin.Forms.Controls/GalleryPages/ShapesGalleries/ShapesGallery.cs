using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.ShapesGalleries
{
	[Preserve(AllMembers = true)]
	public class ShapesGallery : ContentPage
	{
		public ShapesGallery()
		{
			Title = "Shapes Gallery";

			var button = new Button
			{
				Text = "Enable Shapes",
				AutomationId = "EnableShapes"
			};
			button.Clicked += ButtonClicked;

			Content = new StackLayout
			{
				Children =
				{
					button,
					GalleryBuilder.NavButton("Ellipse Gallery", () => new EllipseGallery(), Navigation),
					GalleryBuilder.NavButton("Line Gallery", () => new LineGallery(), Navigation),
					GalleryBuilder.NavButton("Polygon Gallery", () => new PolygonGallery(), Navigation),
					GalleryBuilder.NavButton("Polyline Gallery", () => new PolylineGallery(), Navigation),
					GalleryBuilder.NavButton("Rectangle Gallery", () => new RectangleGallery(), Navigation),
					GalleryBuilder.NavButton("LineCap Gallery", () => new LineCapGallery(), Navigation),
					GalleryBuilder.NavButton("LineJoin Gallery", () => new LineJoinGallery(), Navigation),
					GalleryBuilder.NavButton("AutoSize Shapes Gallery", () => new AutoSizeShapesGallery(), Navigation)
				}
			};
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "Shapes Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.ShapesExperimental });
		}
	}
}