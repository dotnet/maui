using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public class EllipseGeometry : Geometry
	{
		public EllipseGeometry()
		{

		}

		public EllipseGeometry(Point center, double radiusX, double radiusY)
		{
			Center = center;
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		public static readonly BindableProperty CenterProperty =
			BindableProperty.Create(nameof(Center), typeof(Point), typeof(EllipseGeometry), new Point());

		public static readonly BindableProperty RadiusXProperty =
			BindableProperty.Create(nameof(RadiusX), typeof(double), typeof(EllipseGeometry), 0.0);

		public static readonly BindableProperty RadiusYProperty =
			BindableProperty.Create(nameof(RadiusY), typeof(double), typeof(EllipseGeometry), 0.0);

		public Point Center
		{
			set { SetValue(CenterProperty, value); }
			get { return (Point)GetValue(CenterProperty); }
		}

		public double RadiusX
		{
			set { SetValue(RadiusXProperty, value); }
			get { return (double)GetValue(RadiusXProperty); }
		}

		public double RadiusY
		{
			set { SetValue(RadiusYProperty, value); }
			get { return (double)GetValue(RadiusYProperty); }
		}

		public override void AppendPath(PathF path)
		{
			var radiusX = (float)RadiusX;
			var radiusY = (float)RadiusY;

			path.AppendEllipse((float)Center.X - radiusX, (float)Center.Y - radiusY, radiusX * 2f, radiusY * 2f);
		}
	}
}