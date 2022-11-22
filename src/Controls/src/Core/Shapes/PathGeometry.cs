using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PathGeometry']/Docs/*" />
	[ContentProperty("Figures")]
	public sealed class PathGeometry : Geometry
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public PathGeometry()
		{
			Figures = new PathFigureCollection();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public PathGeometry(PathFigureCollection figures)
		{
			Figures = figures;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
		public PathGeometry(PathFigureCollection figures, FillRule fillRule)
		{
			Figures = figures;
			FillRule = fillRule;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='FiguresProperty']/Docs/*" />
		public static readonly BindableProperty FiguresProperty =
			BindableProperty.Create(nameof(Figures), typeof(PathFigureCollection), typeof(PathGeometry), null,
				propertyChanged: OnPathFigureCollectionChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='FillRuleProperty']/Docs/*" />
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(PathGeometry), FillRule.EvenOdd);

		static void OnPathFigureCollectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as PathGeometry)?.UpdatePathFigureCollection(oldValue as PathFigureCollection, newValue as PathFigureCollection);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='Figures']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(PathFigureCollectionConverter))]
		public PathFigureCollection Figures
		{
			set { SetValue(FiguresProperty, value); }
			get { return (PathFigureCollection)GetValue(FiguresProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometry.xml" path="//Member[@MemberName='FillRule']/Docs/*" />
		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }
		}

		internal event EventHandler InvalidatePathGeometryRequested;

		public override void AppendPath(PathF path)
		{
			foreach (var figure in Figures)
			{
				float startPointX = (float)figure.StartPoint.X;
				float startPointY = (float)figure.StartPoint.Y;

				path.MoveTo(startPointX, startPointY);

				foreach (var segment in figure.Segments)
				{
					if (segment is ArcSegment arcSegment)
						AddArc(path, arcSegment);
					else if (segment is BezierSegment bezierSegment)
						AddBezier(path, bezierSegment);
					else if (segment is LineSegment lineSegment)
						AddLine(path, lineSegment);
					else if (segment is PolyBezierSegment polyBezierSegment)
						AddPolyBezier(path, polyBezierSegment);
					else if (segment is PolyLineSegment polyLineSegment)
						AddPolyLine(path, polyLineSegment);
					else if (segment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
						AddPolyQuad(path, polyQuadraticBezierSegment);
					else if (segment is QuadraticBezierSegment quadraticBezierSegment)
						AddQuad(path, quadraticBezierSegment);
				}

				if (figure.IsClosed)
					path.Close();
			}
		}

		void AddArc(PathF path, ArcSegment arcSegment)
		{
			List<Point> points = new List<Point>();

			GeometryHelper.FlattenArc(
				points,
				path.LastPoint,
				arcSegment.Point,
				arcSegment.Size.Width,
				arcSegment.Size.Height,
				arcSegment.RotationAngle,
				arcSegment.IsLargeArc,
				arcSegment.SweepDirection == SweepDirection.CounterClockwise,
				1);

			for (int i = 0; i < points.Count; i++)
			{
				path.LineTo(
					(float)points[i].X,
					(float)points[i].Y);
			}
		}

		void AddLine(PathF path, LineSegment lineSegment)
		{
			path.LineTo(
				(float)(lineSegment.Point.X),
				(float)(lineSegment.Point.Y));
		}

		void AddPolyLine(PathF path, PolyLineSegment polyLineSegment)
		{
			foreach (var p in polyLineSegment.Points)
				path.LineTo(
					(float)(p.X),
					(float)(p.Y));
		}

		void AddBezier(PathF path, BezierSegment bezierSegment)
		{
			path.CurveTo(
				(float)(bezierSegment.Point1.X), (float)(bezierSegment.Point1.Y),
				(float)(bezierSegment.Point2.X), (float)(bezierSegment.Point2.Y),
				(float)(bezierSegment.Point3.X), (float)(bezierSegment.Point3.Y));
		}

		void AddPolyBezier(PathF path, PolyBezierSegment polyBezierSegment)
		{
			for (int bez = 0; bez < polyBezierSegment.Points.Count; bez += 3)
			{
				if (bez + 2 > polyBezierSegment.Points.Count - 1)
					break;

				var pt1 = new PointF((float)(polyBezierSegment.Points[bez].X), (float)(polyBezierSegment.Points[bez].Y));
				var pt2 = new PointF((float)(polyBezierSegment.Points[bez + 1].X), (float)(polyBezierSegment.Points[bez + 1].Y));
				var pt3 = new PointF((float)(polyBezierSegment.Points[bez + 2].X), (float)(polyBezierSegment.Points[bez + 2].Y));

				path.CurveTo(pt1, pt2, pt3);
			}
		}

		void AddQuad(PathF path, QuadraticBezierSegment quadraticBezierSegment)
		{
			path.QuadTo(
				(float)(quadraticBezierSegment.Point1.X), (float)(quadraticBezierSegment.Point1.Y),
				(float)(quadraticBezierSegment.Point2.X), (float)(quadraticBezierSegment.Point2.Y));
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

					var pt1 = new PointF((float)(points[i].X), (float)(points[i].Y));
					var pt2 = new PointF((float)(points[i + 1].X), (float)(points[i + 1].Y));

					path.QuadTo(pt1, pt2);
				}
			}
		}

		void UpdatePathFigureCollection(PathFigureCollection oldCollection, PathFigureCollection newCollection)
		{
			if (oldCollection != null)
			{
				oldCollection.CollectionChanged -= OnPathFigureCollectionChanged;

				foreach (var oldPathFigure in oldCollection)
				{
					oldPathFigure.PropertyChanged -= OnPathFigurePropertyChanged;
					oldPathFigure.InvalidatePathSegmentRequested -= OnInvalidatePathSegmentRequested;
				}
			}

			if (newCollection == null)
				return;

			newCollection.CollectionChanged += OnPathFigureCollectionChanged;

			foreach (var newPathFigure in newCollection)
			{
				newPathFigure.PropertyChanged += OnPathFigurePropertyChanged;
				newPathFigure.InvalidatePathSegmentRequested += OnInvalidatePathSegmentRequested;
			}
		}

		void OnPathFigureCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (var oldItem in e.OldItems)
				{
					if (!(oldItem is PathFigure oldPathFigure))
						continue;

					oldPathFigure.PropertyChanged -= OnPathFigurePropertyChanged;
					oldPathFigure.InvalidatePathSegmentRequested -= OnInvalidatePathSegmentRequested;
				}
			}

			if (e.NewItems != null)
			{
				foreach (var newItem in e.NewItems)
				{
					if (!(newItem is PathFigure newPathFigure))
						continue;

					newPathFigure.PropertyChanged += OnPathFigurePropertyChanged;
					newPathFigure.InvalidatePathSegmentRequested += OnInvalidatePathSegmentRequested;
				}
			}

			Invalidate();
		}

		void OnPathFigurePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Invalidate();
		}

		void OnInvalidatePathSegmentRequested(object sender, EventArgs e)
		{
			Invalidate();
		}

		void Invalidate()
		{
			InvalidatePathGeometryRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}
