using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public readonly struct Stroke : IEquatable<Stroke>
	{
		public Stroke(Paint? brush, double thickness) : this()
		{
			Brush = brush;
			Thickness = thickness;
		}

		public Stroke(Paint? brush, double thickness, LineCap lineCap, LineJoin lineJoin, float[]? dashPattern, float miterLimit) : this()
		{
			Brush = brush;
			Thickness = thickness;
			LineCap = lineCap;
			LineJoin = lineJoin;
			DashPattern = dashPattern;
			MiterLimit = miterLimit;
		}

		public Paint? Brush { get; }

		public double Thickness { get; }

		public LineCap LineCap { get; }

		public LineJoin LineJoin { get; }

		public float[]? DashPattern { get; }

		public float MiterLimit { get; }

		public Stroke WithBrush(Paint? brush)
		{
			return new Stroke(brush, Thickness, LineCap, LineJoin, DashPattern, MiterLimit);
		}

		public Stroke WithThickness(double thickness)
		{
			return new Stroke(Brush, thickness, LineCap, LineJoin, DashPattern, MiterLimit);
		}

		public Stroke WithLineCap(LineCap lineCap)
		{
			return new Stroke(Brush, Thickness, lineCap, LineJoin, DashPattern, MiterLimit);
		}

		public Stroke WithLineJoin(LineJoin lineJoin)
		{
			return new Stroke(Brush, Thickness, LineCap, lineJoin, DashPattern, MiterLimit);
		}

		public Stroke WithDashPattern(float[]? dashPattern)
		{
			return new Stroke(Brush, Thickness, LineCap, LineJoin, dashPattern, MiterLimit);
		}

		public Stroke WithMiterLimit(float miterLimit)
		{
			return new Stroke(Brush, Thickness, LineCap, LineJoin, DashPattern, miterLimit);
		}

		public bool Equals(Stroke other)
		{
			return Equals(Thickness, other.Thickness)
				&& LineCap.Equals(other.LineCap)
				&& LineJoin == other.LineJoin
				&& MiterLimit == other.MiterLimit;
		}
	}

	public readonly struct BorderStroke : IEquatable<BorderStroke>
	{
		public BorderStroke(IShape? shape, Paint? brush, double thickness) : this()
		{
			Shape = shape;
			Brush = brush;
			Thickness = thickness;
		}

		public BorderStroke(IShape? shape, Paint? brush, double thickness, LineCap lineCap, LineJoin lineJoin, float[]? dashPattern, float miterLimit) : this()
		{
			Shape = shape;
			Brush = brush;
			Thickness = thickness;
			LineCap = lineCap;
			LineJoin = lineJoin;
			DashPattern = dashPattern;
			MiterLimit = miterLimit;
		}

		public IShape? Shape { get; }

		public Paint? Brush { get; }

		public double Thickness { get; }

		public LineCap LineCap { get; }

		public LineJoin LineJoin { get; }

		public float[]? DashPattern { get; }

		public float MiterLimit { get; }

		public BorderStroke WithShape(IShape? shape)
		{
			return new BorderStroke(shape, Brush, Thickness, LineCap, LineJoin, DashPattern, MiterLimit);
		}

		public BorderStroke WithBrush(Paint? brush)
		{
			return new BorderStroke(Shape, brush, Thickness, LineCap, LineJoin, DashPattern, MiterLimit);
		}

		public BorderStroke WithThickness(double thickness)
		{
			return new BorderStroke(Shape, Brush, thickness, LineCap, LineJoin, DashPattern, MiterLimit);
		}

		public BorderStroke WithLineCap(LineCap lineCap)
		{
			return new BorderStroke(Shape, Brush, Thickness, lineCap, LineJoin, DashPattern, MiterLimit);
		}

		public BorderStroke WithLineJoin(LineJoin lineJoin)
		{
			return new BorderStroke(Shape, Brush, Thickness, LineCap, lineJoin, DashPattern, MiterLimit);
		}

		public BorderStroke WithDashPattern(float[]? dashPattern)
		{
			return new BorderStroke(Shape, Brush, Thickness, LineCap, LineJoin, dashPattern, MiterLimit);
		}

		public BorderStroke WithMiterLimit(float miterLimit)
		{
			return new BorderStroke(Shape, Brush, Thickness, LineCap, LineJoin, DashPattern, miterLimit);
		}

		public bool Equals(BorderStroke other)
		{
			return Equals(Thickness, other.Thickness)
				&& LineCap.Equals(other.LineCap)
				&& LineJoin == other.LineJoin
				&& MiterLimit == other.MiterLimit;
		}
	}
}