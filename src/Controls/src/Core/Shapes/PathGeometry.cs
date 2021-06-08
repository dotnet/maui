using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	[ContentProperty("Figures")]
	public sealed class PathGeometry : Geometry
	{
		public PathGeometry()
		{
			Figures = new PathFigureCollection();
		}

		public PathGeometry(PathFigureCollection figures)
		{
			Figures = figures;
		}

		public PathGeometry(PathFigureCollection figures, FillRule fillRule)
		{
			Figures = figures;
			FillRule = fillRule;
		}

		public static readonly BindableProperty FiguresProperty =
			BindableProperty.Create(nameof(Figures), typeof(PathFigureCollection), typeof(PathGeometry), null);

		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(PathGeometry), FillRule.EvenOdd);

		[TypeConverter(typeof(PathFigureCollectionConverter))]
		public PathFigureCollection Figures
		{
			set { SetValue(FiguresProperty, value); }
			get { return (PathFigureCollection)GetValue(FiguresProperty); }
		}

		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }
		}

		public override void AppendPath(PathF path)
		{
			foreach (var f in Figures)
			{
				foreach (var s in f.Segments)
				{
					if (s is ArcSegment arcSegment)
						AddArc(path, arcSegment);
					else if (s is BezierSegment bezierSegment)
						AddBezier(path, bezierSegment);
					else if (s is LineSegment lineSegment)
						AddLine(path, lineSegment);
					else if (s is PolyBezierSegment polyBezierSegment)
						AddPolyBezier(path, polyBezierSegment);
					else if (s is PolyLineSegment polyLineSegment)
						AddPolyLine(path, polyLineSegment);
					else if (s is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
						AddPolyQuad(path, polyQuadraticBezierSegment);
					else if (s is QuadraticBezierSegment quadraticBezierSegment)
						AddQuad(path, quadraticBezierSegment);
				}
			}
		}

		void AddArc(PathF path, ArcSegment arcSegment)
		{
			path.AddArc(
				(float)arcSegment.Point.X,
				(float)arcSegment.Point.Y,
				(float)arcSegment.Point.X + (float)arcSegment.Size.Width,
				(float)arcSegment.Point.Y + (float)arcSegment.Size.Height,
				(float)arcSegment.RotationAngle,
				(float)arcSegment.RotationAngle,
				arcSegment.SweepDirection == SweepDirection.Clockwise);
		}

		void AddLine(PathF path, LineSegment lineSegment)
		{
			path.LineTo((float)lineSegment.Point.X, (float)lineSegment.Point.Y);
		}

		void AddPolyLine(PathF path, PolyLineSegment polyLineSegment)
		{
			foreach (var p in polyLineSegment.Points)
				path.LineTo((float)p.X, (float)p.Y);
		}

		void AddBezier(PathF path, BezierSegment bezierSegment)
		{
			path.CurveTo(
				(float)bezierSegment.Point1.X, (float)bezierSegment.Point1.Y,
				(float)bezierSegment.Point2.X, (float)bezierSegment.Point2.Y,
				(float)bezierSegment.Point3.X, (float)bezierSegment.Point3.Y);
		}

		void AddPolyBezier(PathF path, PolyBezierSegment polyBezierSegment)
		{
			for (int bez = 0; bez < polyBezierSegment.Points.Count; bez += 3)
			{
				if (bez + 2 > polyBezierSegment.Points.Count - 1)
					break;

				var pt1 = new PointF((float)polyBezierSegment.Points[bez].X, (float)polyBezierSegment.Points[bez].Y);
				var pt2 = new PointF((float)polyBezierSegment.Points[bez + 1].X, (float)polyBezierSegment.Points[bez + 1].Y);
				var pt3 = new PointF((float)polyBezierSegment.Points[bez + 2].X, (float)polyBezierSegment.Points[bez + 2].Y);

				path.CurveTo(pt1, pt2, pt3);
			}
		}

		void AddQuad(PathF path, QuadraticBezierSegment quadraticBezierSegment)
		{
			path.QuadTo((float)quadraticBezierSegment.Point1.X, (float)quadraticBezierSegment.Point1.Y,
							(float)quadraticBezierSegment.Point2.X, (float)quadraticBezierSegment.Point2.Y);
		}

		void AddPolyQuad(PathF path, PolyQuadraticBezierSegment polyQuadraticBezierSegment)
		{
			var points = polyQuadraticBezierSegment.Points;

			if (points.Count >= 2)
			{
				for (int i = 0; i < polyQuadraticBezierSegment.Points.Count; i += 2)
				{
					if (i + 1 > polyQuadraticBezierSegment.Points.Count - 1)
						break;

					var pt1 = new PointF((float)points[i].X, (float)points[i].Y);
					var pt2 = new PointF((float)points[i + 1].X, (float)points[i + 1].Y);

					path.QuadTo(pt1, pt2);
				}
			}
		}
	}
}