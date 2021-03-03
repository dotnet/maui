using SkiaSharp;
using FormsRectangle = Microsoft.Maui.Controls.Compatibility.Shapes.Rectangle;


namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp
{
	public class RectangleRenderer : ShapeRenderer<FormsRectangle, RectView>
	{
		public RectangleRenderer() : base()
		{
			RegisterPropertyHandler(FormsRectangle.RadiusXProperty, UpdateRadiusX);
			RegisterPropertyHandler(FormsRectangle.RadiusYProperty, UpdateRadiusY);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<FormsRectangle> e)
		{
			if (Control == null)
			{
				SetNativeControl(new RectView());
			}

			base.OnElementChanged(e);
		}

		void UpdateRadiusX()
		{
			if (Element.Width > 0)
				Control.UpdateRadiusX(Element.RadiusX / Element.Width);
		}

		void UpdateRadiusY()
		{
			if (Element.Height > 0)
				Control.UpdateRadiusY(Element.RadiusY / Element.Height);
		}
	}

	public class RectView : ShapeView
	{
		public RectView() : base()
		{
			UpdateShape();
		}

		public float RadiusX { set; get; }

		public float RadiusY { set; get; }

		void UpdateShape()
		{
			var path = new SKPath();
			path.AddRoundRect(new SKRect(0, 0, 1, 1), RadiusX, RadiusY, SKPathDirection.Clockwise);
			UpdateShape(path);
		}

		public void UpdateRadiusX(double radiusX)
		{
			RadiusX = (float)radiusX;
			UpdateShape();
		}

		public void UpdateRadiusY(double radiusY)
		{
			RadiusY = (float)radiusY;
			UpdateShape();
		}
	}
}
