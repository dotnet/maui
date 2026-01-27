#nullable disable
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents the geometry of an ellipse or circle.
	/// </summary>
	public class EllipseGeometry : Geometry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EllipseGeometry"/> class.
		/// </summary>
		public EllipseGeometry()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EllipseGeometry"/> class with the specified center point and radii.
		/// </summary>
		/// <param name="center">The center point of the ellipse.</param>
		/// <param name="radiusX">The x-radius of the ellipse.</param>
		/// <param name="radiusY">The y-radius of the ellipse.</param>
		public EllipseGeometry(Point center, double radiusX, double radiusY)
		{
			Center = center;
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		/// <summary>Bindable property for <see cref="Center"/>.</summary>
		public static readonly BindableProperty CenterProperty =
			BindableProperty.Create(nameof(Center), typeof(Point), typeof(EllipseGeometry), new Point());

		/// <summary>Bindable property for <see cref="RadiusX"/>.</summary>
		public static readonly BindableProperty RadiusXProperty =
			BindableProperty.Create(nameof(RadiusX), typeof(double), typeof(EllipseGeometry), 0.0);

		/// <summary>Bindable property for <see cref="RadiusY"/>.</summary>
		public static readonly BindableProperty RadiusYProperty =
			BindableProperty.Create(nameof(RadiusY), typeof(double), typeof(EllipseGeometry), 0.0);

		/// <summary>
		/// Gets or sets the center point of the ellipse. This is a bindable property.
		/// </summary>
		public Point Center
		{
			set { SetValue(CenterProperty, value); }
			get { return (Point)GetValue(CenterProperty); }
		}

		/// <summary>
		/// Gets or sets the x-radius of the ellipse. This is a bindable property.
		/// </summary>
		public double RadiusX
		{
			set { SetValue(RadiusXProperty, value); }
			get { return (double)GetValue(RadiusXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-radius of the ellipse. This is a bindable property.
		/// </summary>
		public double RadiusY
		{
			set { SetValue(RadiusYProperty, value); }
			get { return (double)GetValue(RadiusYProperty); }
		}

		public override void AppendPath(PathF path)
		{
			var centerX = (float)Center.X;
			var centerY = (float)Center.Y;

			var radiusX = (float)RadiusX;
			var radiusY = (float)RadiusY;

			path.AppendEllipse(centerX - radiusX, centerY - radiusY, radiusX * 2f, radiusY * 2f);
		}
	}
}
