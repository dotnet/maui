using System;
using System.Numerics;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// Conversions between .NET MAUI Graphics types and Cairo types.
	/// </summary>
	public static class CairoExtensions
	{
		// Operator values that CairoSharp's Cairo.Operator enum does not name (cairo_operator_t, cairo 1.10+).
		private const int OperatorMultiply = 14;
		private const int OperatorScreen = 15;
		private const int OperatorOverlay = 16;
		private const int OperatorDarken = 17;
		private const int OperatorLighten = 18;
		private const int OperatorColorDodge = 19;
		private const int OperatorColorBurn = 20;
		private const int OperatorHardLight = 21;
		private const int OperatorSoftLight = 22;
		private const int OperatorDifference = 23;
		private const int OperatorExclusion = 24;
		private const int OperatorHslHue = 25;
		private const int OperatorHslSaturation = 26;
		private const int OperatorHslColor = 27;
		private const int OperatorHslLuminosity = 28;

		/// <summary>
		/// Converts a <see cref="Color"/> to a <see cref="global::Cairo.Color"/>.
		/// </summary>
		/// <param name="color">The color; <see langword="null"/> converts to opaque black.</param>
		/// <param name="alpha">An extra alpha multiplier (0..1).</param>
		/// <returns>The Cairo color (not premultiplied).</returns>
		public static global::Cairo.Color AsCairoColor(this Color color, float alpha = 1f)
		{
			if (color == null)
				return new global::Cairo.Color(0, 0, 0, alpha);

			return new global::Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha * alpha);
		}

		/// <summary>
		/// Converts a <see cref="Matrix3x2"/> to a <see cref="CairoMatrix"/> (same row-vector convention: <c>x' = xx*x + xy*y + x0</c>).
		/// </summary>
		/// <param name="matrix">The matrix.</param>
		/// <returns>The Cairo matrix.</returns>
		public static CairoMatrix AsCairoMatrix(this Matrix3x2 matrix) =>
			new CairoMatrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32);

		/// <summary>
		/// Maps a <see cref="BlendMode"/> to the matching Cairo compositing operator.
		/// </summary>
		/// <param name="blendMode">The blend mode.</param>
		/// <returns>The Cairo operator. <see cref="BlendMode.PlusDarker"/> has no Cairo equivalent and maps to <see cref="CairoOperator.Over"/>.</returns>
		public static CairoOperator AsCairoOperator(this BlendMode blendMode)
		{
			switch (blendMode)
			{
				case BlendMode.Multiply:
					return (CairoOperator)OperatorMultiply;
				case BlendMode.Screen:
					return (CairoOperator)OperatorScreen;
				case BlendMode.Overlay:
					return (CairoOperator)OperatorOverlay;
				case BlendMode.Darken:
					return (CairoOperator)OperatorDarken;
				case BlendMode.Lighten:
					return (CairoOperator)OperatorLighten;
				case BlendMode.ColorDodge:
					return (CairoOperator)OperatorColorDodge;
				case BlendMode.ColorBurn:
					return (CairoOperator)OperatorColorBurn;
				case BlendMode.SoftLight:
					return (CairoOperator)OperatorSoftLight;
				case BlendMode.HardLight:
					return (CairoOperator)OperatorHardLight;
				case BlendMode.Difference:
					return (CairoOperator)OperatorDifference;
				case BlendMode.Exclusion:
					return (CairoOperator)OperatorExclusion;
				case BlendMode.Hue:
					return (CairoOperator)OperatorHslHue;
				case BlendMode.Saturation:
					return (CairoOperator)OperatorHslSaturation;
				case BlendMode.Color:
					return (CairoOperator)OperatorHslColor;
				case BlendMode.Luminosity:
					return (CairoOperator)OperatorHslLuminosity;
				case BlendMode.Clear:
					return CairoOperator.Clear;
				case BlendMode.Copy:
					return CairoOperator.Source;
				case BlendMode.SourceIn:
					return CairoOperator.In;
				case BlendMode.SourceOut:
					return CairoOperator.Out;
				case BlendMode.SourceAtop:
					return CairoOperator.Atop;
				case BlendMode.DestinationOver:
					return CairoOperator.DestOver;
				case BlendMode.DestinationIn:
					return CairoOperator.DestIn;
				case BlendMode.DestinationOut:
					return CairoOperator.DestOut;
				case BlendMode.DestinationAtop:
					return CairoOperator.DestAtop;
				case BlendMode.Xor:
					return CairoOperator.Xor;
				case BlendMode.PlusLighter:
					return CairoOperator.Add;
				default:
					return CairoOperator.Over;
			}
		}

		/// <summary>
		/// Appends <paramref name="path"/> to the current path of <paramref name="context"/>, in the context's user space.
		/// </summary>
		/// <param name="context">The Cairo context.</param>
		/// <param name="path">The path to append.</param>
		/// <remarks>
		/// Arc segments start a new sub-path (like the Skia canvas does), they are never joined to the previous point.
		/// </remarks>
		public static void AddPath(this CairoContext context, PathF path)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			var pointIndex = 0;
			var arcAngleIndex = 0;
			var arcClockwiseIndex = 0;

			foreach (var type in path.SegmentTypes)
			{
				switch (type)
				{
					case PathOperation.Move:
						{
							var p = path[pointIndex++];
							context.MoveTo(p.X, p.Y);
							break;
						}
					case PathOperation.Line:
						{
							var p = path[pointIndex++];
							context.LineTo(p.X, p.Y);
							break;
						}
					case PathOperation.Quad:
						{
							// Cairo only knows cubic curves: raise the quadratic to a cubic.
							var c = path[pointIndex++];
							var p = path[pointIndex++];
							var p0 = context.HasCurrentPoint ? context.CurrentPoint : new global::Cairo.PointD(c.X, c.Y);
							context.CurveTo(
								p0.X + 2d / 3d * (c.X - p0.X), p0.Y + 2d / 3d * (c.Y - p0.Y),
								p.X + 2d / 3d * (c.X - p.X), p.Y + 2d / 3d * (c.Y - p.Y),
								p.X, p.Y);
							break;
						}
					case PathOperation.Cubic:
						{
							var c1 = path[pointIndex++];
							var c2 = path[pointIndex++];
							var p = path[pointIndex++];
							context.CurveTo(c1.X, c1.Y, c2.X, c2.Y, p.X, p.Y);
							break;
						}
					case PathOperation.Arc:
						{
							var topLeft = path[pointIndex++];
							var bottomRight = path[pointIndex++];
							var startAngle = path.GetArcAngle(arcAngleIndex++);
							var endAngle = path.GetArcAngle(arcAngleIndex++);
							var clockwise = path.GetArcClockwise(arcClockwiseIndex++);
							context.AddArc(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, startAngle, endAngle, clockwise, true);
							break;
						}
					case PathOperation.Close:
						context.ClosePath();
						break;
				}
			}
		}

		/// <summary>
		/// Appends an elliptical arc to the current path using the .NET MAUI Graphics angle convention
		/// (degrees, counter-clockwise from the 3 o'clock position).
		/// </summary>
		/// <param name="context">The Cairo context.</param>
		/// <param name="x">Left of the bounding box of the full ellipse.</param>
		/// <param name="y">Top of the bounding box of the full ellipse.</param>
		/// <param name="width">Width of the bounding box.</param>
		/// <param name="height">Height of the bounding box.</param>
		/// <param name="startAngle">Start angle in degrees.</param>
		/// <param name="endAngle">End angle in degrees.</param>
		/// <param name="clockwise">Whether the arc runs clockwise from <paramref name="startAngle"/> to <paramref name="endAngle"/>.</param>
		/// <param name="newSubPath">When <see langword="true"/> the arc starts a new sub-path instead of being connected to the current point.</param>
		public static void AddArc(this CairoContext context, double x, double y, double width, double height, double startAngle, double endAngle, bool clockwise, bool newSubPath)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			var rx = Math.Abs(width) / 2;
			var ry = Math.Abs(height) / 2;
			if (rx <= 0 || ry <= 0)
				return;

			while (startAngle < 0)
				startAngle += 360;
			while (endAngle < 0)
				endAngle += 360;

			var sweep = GeometryUtil.GetSweep((float)startAngle, (float)endAngle, clockwise);

			// Cairo angles grow clockwise on screen (y is down), MAUI angles grow counter-clockwise.
			var a1 = -startAngle * Math.PI / 180;
			var delta = sweep * Math.PI / 180;

			var saved = context.Matrix;
			context.Translate(x + width / 2, y + height / 2);
			context.Scale(rx, ry);

			if (newSubPath)
				context.NewSubPath();

			if (clockwise)
				context.Arc(0, 0, 1, a1, a1 + delta);
			else
				context.ArcNegative(0, 0, 1, a1, a1 - delta);

			context.Matrix = saved;
		}

		/// <summary>
		/// Appends a rounded rectangle to the current path.
		/// </summary>
		/// <param name="context">The Cairo context.</param>
		/// <param name="x">Left.</param>
		/// <param name="y">Top.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="cornerRadius">The corner radius, clamped to half of the smaller side.</param>
		public static void AddRoundedRectangle(this CairoContext context, double x, double y, double width, double height, double cornerRadius)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (width < 0)
			{
				x += width;
				width = -width;
			}

			if (height < 0)
			{
				y += height;
				height = -height;
			}

			var r = Math.Min(Math.Max(cornerRadius, 0), Math.Min(width, height) / 2);
			if (r <= 0)
			{
				context.Rectangle(x, y, width, height);
				return;
			}

			const double Quarter = Math.PI / 2;
			context.NewSubPath();
			context.Arc(x + width - r, y + r, r, -Quarter, 0);
			context.Arc(x + width - r, y + height - r, r, 0, Quarter);
			context.Arc(x + r, y + height - r, r, Quarter, 2 * Quarter);
			context.Arc(x + r, y + r, r, 2 * Quarter, 3 * Quarter);
			context.ClosePath();
		}

		/// <summary>
		/// Appends a full ellipse inscribed in the given rectangle to the current path.
		/// </summary>
		/// <param name="context">The Cairo context.</param>
		/// <param name="x">Left.</param>
		/// <param name="y">Top.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static void AddEllipse(this CairoContext context, double x, double y, double width, double height)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			var rx = Math.Abs(width) / 2;
			var ry = Math.Abs(height) / 2;
			if (rx <= 0 || ry <= 0)
				return;

			var saved = context.Matrix;
			context.Translate(x + width / 2, y + height / 2);
			context.Scale(rx, ry);
			context.NewSubPath();
			context.Arc(0, 0, 1, 0, 2 * Math.PI);
			context.ClosePath();
			context.Matrix = saved;
		}
	}
}
