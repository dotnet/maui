using Microsoft.Maui.Graphics;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PathGeometry']/Docs" />
	[ContentProperty("Figures")]
	public sealed class PathGeometry : Geometry
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public PathGeometry()
		{
			Figures = new PathFigureCollection();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public PathGeometry(PathFigureCollection figures)
		{
			Figures = figures;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public PathGeometry(PathFigureCollection figures, FillRule fillRule)
		{
			Figures = figures;
			FillRule = fillRule;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='FiguresProperty']/Docs" />
		public static readonly BindableProperty FiguresProperty =
			BindableProperty.Create(nameof(Figures), typeof(PathFigureCollection), typeof(PathGeometry), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='FillRuleProperty']/Docs" />
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(PathGeometry), FillRule.EvenOdd);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='Figures']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(PathFigureCollectionConverter))]
		public PathFigureCollection Figures
		{
			set { SetValue(FiguresProperty, value); }
			get { return (PathFigureCollection)GetValue(FiguresProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='FillRule']/Docs" />
		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='AppendPath']/Docs" />
		public override void AppendPath(PathF path)
		{
			double density = 1.0d;
#if ANDROID
			density = DeviceDisplay.MainDisplayInfo.Density;
#endif

			foreach (var figure in Figures)
			{
				float startPointX = (float)(density * figure.StartPoint.X);
				float startPointY = (float)(density * figure.StartPoint.Y);

				path.MoveTo(startPointX, startPointY);

				foreach (var segment in figure.Segments)
				{
					if (segment is ArcSegment arcSegment)
						AddArc(path, arcSegment, density);
					else if (segment is BezierSegment bezierSegment)
						AddBezier(path, bezierSegment, density);
					else if (segment is LineSegment lineSegment)
						AddLine(path, lineSegment, density);
					else if (segment is PolyBezierSegment polyBezierSegment)
						AddPolyBezier(path, polyBezierSegment, density);
					else if (segment is PolyLineSegment polyLineSegment)
						AddPolyLine(path, polyLineSegment, density);
					else if (segment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
						AddPolyQuad(path, polyQuadraticBezierSegment, density);
					else if (segment is QuadraticBezierSegment quadraticBezierSegment)
						AddQuad(path, quadraticBezierSegment, density);
				}

				if (figure.IsClosed)
					path.Close();
			}
		}

		void AddArc(PathF path, ArcSegment arcSegment, double density)
		{
			path.AddArc(
				(float)(density * arcSegment.Point.X),
				(float)(density * arcSegment.Point.Y),
				(float)(density * arcSegment.Point.X + density * arcSegment.Size.Width),
				(float)(density * arcSegment.Point.Y + density * arcSegment.Size.Height),
				(float)(density * arcSegment.RotationAngle),
				(float)(density * arcSegment.RotationAngle),
				arcSegment.SweepDirection == SweepDirection.Clockwise);
		}

		void AddLine(PathF path, LineSegment lineSegment, double density)
		{
			path.LineTo(
				(float)(density * lineSegment.Point.X), 
				(float)(density * lineSegment.Point.Y));
		}

		void AddPolyLine(PathF path, PolyLineSegment polyLineSegment, double density)
		{
			foreach (var p in polyLineSegment.Points)
				path.LineTo(
					(float)(density * p.X), 
					(float)(density * p.Y));
		}

		void AddBezier(PathF path, BezierSegment bezierSegment, double density)
		{
			path.CurveTo(	
				(float)(density * bezierSegment.Point1.X), (float)(density * bezierSegment.Point1.Y),
				(float)(density * bezierSegment.Point2.X), (float)(density * bezierSegment.Point2.Y),
				(float)(density * bezierSegment.Point3.X), (float)(density * bezierSegment.Point3.Y));
		}

		void AddPolyBezier(PathF path, PolyBezierSegment polyBezierSegment, double density)
		{
			for (int bez = 0; bez < polyBezierSegment.Points.Count; bez += 3)
			{
				if (bez + 2 > polyBezierSegment.Points.Count - 1)
					break;

				var pt1 = new PointF((float)(density * polyBezierSegment.Points[bez].X), (float)(density * polyBezierSegment.Points[bez].Y));
				var pt2 = new PointF((float)(density * polyBezierSegment.Points[bez + 1].X), (float)(density * polyBezierSegment.Points[bez + 1].Y));
				var pt3 = new PointF((float)(density * polyBezierSegment.Points[bez + 2].X), (float)(density * polyBezierSegment.Points[bez + 2].Y));

				path.CurveTo(pt1, pt2, pt3);
			}
		}

		void AddQuad(PathF path, QuadraticBezierSegment quadraticBezierSegment, double density)
		{
			path.QuadTo(
				(float)(density * quadraticBezierSegment.Point1.X), (float)(density * quadraticBezierSegment.Point1.Y),
				(float)(density * quadraticBezierSegment.Point2.X), (float)(density * quadraticBezierSegment.Point2.Y));
		}

		void AddPolyQuad(PathF path, PolyQuadraticBezierSegment polyQuadraticBezierSegment, double density)
		{
			var points = polyQuadraticBezierSegment.Points;

			if (points.Count >= 2)
			{
				for (int i = 0; i < polyQuadraticBezierSegment.Points.Count; i += 2)
				{
					if (i + 1 > polyQuadraticBezierSegment.Points.Count - 1)
						break;

					var pt1 = new PointF((float)(density * points[i].X), (float)(density * points[i].Y));
					var pt2 = new PointF((float)(density * points[i + 1].X), (float)(density * points[i + 1].Y));

					path.QuadTo(pt1, pt2);
				}
			}
		}
	}
}