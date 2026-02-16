#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a complex geometry composed of <see cref="PathFigure"/> objects.
	/// </summary>
	[ContentProperty("Figures")]
	public sealed class PathGeometry : Geometry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PathGeometry"/> class.
		/// </summary>
		public PathGeometry()
		{
			Figures = new PathFigureCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PathGeometry"/> class with the specified figures.
		/// </summary>
		/// <param name="figures">The collection of path figures that define this geometry.</param>
		public PathGeometry(PathFigureCollection figures)
		{
			Figures = figures;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PathGeometry"/> class with the specified figures and fill rule.
		/// </summary>
		/// <param name="figures">The collection of path figures that define this geometry.</param>
		/// <param name="fillRule">The rule that determines how the interior of this geometry is filled.</param>
		public PathGeometry(PathFigureCollection figures, FillRule fillRule)
		{
			Figures = figures;
			FillRule = fillRule;
		}

		/// <summary>Bindable property for <see cref="Figures"/>.</summary>
		public static readonly BindableProperty FiguresProperty =
			BindableProperty.Create(nameof(Figures), typeof(PathFigureCollection), typeof(PathGeometry), null,
				propertyChanged: OnPathFigureCollectionChanged);

		/// <summary>Bindable property for <see cref="FillRule"/>.</summary>
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(PathGeometry), FillRule.EvenOdd);

		static void OnPathFigureCollectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as PathGeometry)?.UpdatePathFigureCollection(oldValue as PathFigureCollection, newValue as PathFigureCollection);
		}

		/// <summary>
		/// Gets or sets the collection of <see cref="PathFigure"/> objects that describe the contents of this path.
		/// This is a bindable property.
		/// </summary>
		[System.ComponentModel.TypeConverter(typeof(PathFigureCollectionConverter))]
		public PathFigureCollection Figures
		{
			set { SetValue(FiguresProperty, value); }
			get { return (PathFigureCollection)GetValue(FiguresProperty); }
		}

		/// <summary>
		/// Gets or sets a value that determines how the intersecting areas contained in this geometry are combined.
		/// This is a bindable property.
		/// </summary>
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
