using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.EllipseGeometry']/Docs" />
	public class EllipseGeometry : Geometry
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public EllipseGeometry()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public EllipseGeometry(Point center, double radiusX, double radiusY)
		{
			Center = center;
			RadiusX = radiusX;
			RadiusY = radiusY;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='CenterProperty']/Docs" />
		public static readonly BindableProperty CenterProperty =
			BindableProperty.Create(nameof(Center), typeof(Point), typeof(EllipseGeometry), new Point());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='RadiusXProperty']/Docs" />
		public static readonly BindableProperty RadiusXProperty =
			BindableProperty.Create(nameof(RadiusX), typeof(double), typeof(EllipseGeometry), 0.0);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='RadiusYProperty']/Docs" />
		public static readonly BindableProperty RadiusYProperty =
			BindableProperty.Create(nameof(RadiusY), typeof(double), typeof(EllipseGeometry), 0.0);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='Center']/Docs" />
		public Point Center
		{
			set { SetValue(CenterProperty, value); }
			get { return (Point)GetValue(CenterProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='RadiusX']/Docs" />
		public double RadiusX
		{
			set { SetValue(RadiusXProperty, value); }
			get { return (double)GetValue(RadiusXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='RadiusY']/Docs" />
		public double RadiusY
		{
			set { SetValue(RadiusYProperty, value); }
			get { return (double)GetValue(RadiusYProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/EllipseGeometry.xml" path="//Member[@MemberName='AppendPath']/Docs" />
		public override void AppendPath(PathF path)
		{
			var radiusX = (float)RadiusX;
			var radiusY = (float)RadiusY;

			path.AppendEllipse((float)Center.X - radiusX, (float)Center.Y - radiusY, radiusX * 2f, radiusY * 2f);
		}
	}
}