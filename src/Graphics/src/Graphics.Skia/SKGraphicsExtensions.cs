using System;
using System.Numerics;

using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides extension methods for working with SkiaSharp graphics objects.
	/// </summary>
	public static class SKGraphicsExtensions
	{
		/// <summary>
		/// Converts a .NET MAUI Graphics color to a SkiaSharp color with a multiplied alpha value.
		/// </summary>
		/// <param name="target">The color to convert.</param>
		/// <param name="alpha">The alpha multiplier to apply (0-1).</param>
		/// <returns>A SkiaSharp color with the alpha value multiplied by the specified factor.</returns>
		public static SKColor AsSKColorMultiplyAlpha(this Color target, float alpha)
		{
			var r = (byte)(target.Red * 255f);
			var g = (byte)(target.Green * 255f);
			var b = (byte)(target.Blue * 255f);
			var a = (byte)(target.Alpha * alpha * 255f);

			if (a > 255)
				a = 255;

			var color = new SKColor(r, g, b, a);
			return color;
		}

		/// <summary>
		/// Converts a .NET MAUI Graphics color to an ARGB integer representation.
		/// </summary>
		/// <param name="target">The color to convert.</param>
		/// <returns>The color as a 32-bit ARGB integer.</returns>
		public static int ToArgb(this Color target)
		{
			var a = (int)(target.Alpha * 255f);
			var r = (int)(target.Red * 255f);
			var g = (int)(target.Green * 255f);
			var b = (int)(target.Blue * 255f);

			var argb = a << 24 | r << 16 | g << 8 | b;
			return argb;
		}

		/// <summary>
		/// Converts a .NET MAUI Graphics color to an ARGB integer representation with a modified alpha value.
		/// </summary>
		/// <param name="target">The color to convert.</param>
		/// <param name="alpha">The alpha multiplier to apply (0-1).</param>
		/// <returns>The color as a 32-bit ARGB integer with the modified alpha value.</returns>
		public static int ToArgb(this Color target, float alpha)
		{
			var a = (int)(target.Alpha * 255f * alpha);
			var r = (int)(target.Red * 255f);
			var g = (int)(target.Green * 255f);
			var b = (int)(target.Blue * 255f);

			var argb = a << 24 | r << 16 | g << 8 | b;
			return argb;
		}

		/// <summary>
		/// Converts a .NET MAUI Graphics color to a SkiaSharp color.
		/// </summary>
		/// <param name="target">The color to convert.</param>
		/// <returns>A SkiaSharp color that corresponds to the specified color.</returns>
		public static SKColor AsSKColor(this Color target)
		{
			var r = (byte)(target.Red * 255f);
			var g = (byte)(target.Green * 255f);
			var b = (byte)(target.Blue * 255f);
			var a = (byte)(target.Alpha * 255f);
			return new SKColor(r, g, b, a);
		}

		/// <summary>
		/// Converts a SkiaSharp color to a .NET MAUI Graphics color.
		/// </summary>
		/// <param name="target">The SkiaSharp color to convert.</param>
		/// <returns>A .NET MAUI Graphics color that corresponds to the specified SkiaSharp color.</returns>
		public static Color AsColor(this SKColor target)
		{
			var r = (int)target.Red;
			var g = (int)target.Green;
			var b = (int)target.Blue;
			var a = (int)target.Alpha;
			return new Color(r, g, b, a);
		}

		/// <summary>
		/// Converts a RectF to a SkiaSharp SKRect.
		/// </summary>
		/// <param name="target">The RectF to convert.</param>
		/// <returns>A SkiaSharp SKRect that corresponds to the specified RectF.</returns>
		public static SKRect AsSKRect(this RectF target)
		{
			return new SKRect(target.Left, target.Top, target.Right, target.Bottom);
		}

		/// <summary>
		/// Converts a SkiaSharp SKRect to a RectF.
		/// </summary>
		/// <param name="target">The SKRect to convert.</param>
		/// <returns>A RectF that corresponds to the specified SKRect.</returns>
		public static RectF AsRectangleF(this SKRect target)
		{
			return new RectF(target.Left, target.Top, MathF.Abs(target.Right - target.Left), MathF.Abs(target.Bottom - target.Top));
		}

		/// <summary>
		/// Converts a PointF to a SkiaSharp SKPoint.
		/// </summary>
		/// <param name="target">The PointF to convert.</param>
		/// <returns>A SkiaSharp SKPoint that corresponds to the specified PointF.</returns>
		public static SKPoint ToSKPoint(this PointF target)
		{
			return new SKPoint(target.X, target.Y);
		}

		/// <summary>
		/// Converts a Matrix3x2 to a SkiaSharp SKMatrix.
		/// </summary>
		/// <param name="transform">The Matrix3x2 to convert.</param>
		/// <returns>A SkiaSharp SKMatrix that corresponds to the specified Matrix3x2.</returns>
		public static SKMatrix AsMatrix(this in Matrix3x2 transform)
		{
			var matrix = new SKMatrix
			{
				ScaleX = transform.M11,
				SkewX = transform.M21,
				TransX = transform.M31,
				SkewY = transform.M12,
				ScaleY = transform.M22,
				TransY = transform.M32,
				Persp0 = 0,
				Persp1 = 0,
				Persp2 = 1
			};
			return matrix;
		}

		/// <summary>
		/// Converts a PathF to a SkiaSharp SKPath.
		/// </summary>
		/// <param name="target">The PathF to convert.</param>
		/// <returns>A SkiaSharp SKPath that corresponds to the specified PathF.</returns>
		public static SKPath AsSkiaPath(this PathF target)
		{
			return AsSkiaPath(target, 1);
		}

		/// <summary>
		/// Converts a PathF to a SkiaSharp SKPath with a specified pixels-per-unit value.
		/// </summary>
		/// <param name="path">The PathF to convert.</param>
		/// <param name="ppu">The pixels-per-unit value.</param>
		/// <returns>A SkiaSharp SKPath that corresponds to the specified PathF, scaled by the specified pixels-per-unit value.</returns>
		public static SKPath AsSkiaPath(this PathF path, float ppu)
		{
			return AsSkiaPath(path, ppu, 0, 0, 1, 1);
		}

		/// <summary>
		/// Converts a PathF to a SkiaSharp SKPath with specified transformation parameters.
		/// </summary>
		/// <param name="path">The PathF to convert.</param>
		/// <param name="ppu">The pixels-per-unit value.</param>
		/// <param name="ox">The x-offset.</param>
		/// <param name="oy">The y-offset.</param>
		/// <param name="fx">The x-scale factor.</param>
		/// <param name="fy">The y-scale factor.</param>
		/// <returns>A SkiaSharp SKPath that corresponds to the specified PathF, transformed according to the specified parameters.</returns>
		public static SKPath AsSkiaPath(
			this PathF path,
			float ppu,
			float ox,
			float oy,
			float fx,
			float fy)
		{
			var platformPath = new SKPath();

			var ppux = ppu * fx;
			var ppuy = ppu * fy;

			var pointIndex = 0;
			var arcAngleIndex = 0;
			var arcClockwiseIndex = 0;

			foreach (var type in path.SegmentTypes)
			{
				if (type == PathOperation.Move)
				{
					var point = path[pointIndex++];
					platformPath.MoveTo((ox + point.X * ppux), (oy + point.Y * ppuy));
				}
				else if (type == PathOperation.Line)
				{
					var point = path[pointIndex++];
					platformPath.LineTo((ox + point.X * ppux), (oy + point.Y * ppuy));
				}

				else if (type == PathOperation.Quad)
				{
					var controlPoint = path[pointIndex++];
					var point = path[pointIndex++];
					platformPath.QuadTo((ox + controlPoint.X * ppux), (oy + controlPoint.Y * ppuy), (ox + point.X * ppux), (oy + point.Y * ppuy));
				}
				else if (type == PathOperation.Cubic)
				{
					var controlPoint1 = path[pointIndex++];
					var controlPoint2 = path[pointIndex++];
					var point = path[pointIndex++];
					platformPath.CubicTo((ox + controlPoint1.X * ppux), (oy + controlPoint1.Y * ppuy), (ox + controlPoint2.X * ppux), (oy + controlPoint2.Y * ppuy), (ox + point.X * ppux),
						(oy + point.Y * ppuy));
				}
				else if (type == PathOperation.Arc)
				{
					var topLeft = path[pointIndex++];
					var bottomRight = path[pointIndex++];
					var startAngle = path.GetArcAngle(arcAngleIndex++);
					var endAngle = path.GetArcAngle(arcAngleIndex++);
					var clockwise = path.GetArcClockwise(arcClockwiseIndex++);

					while (startAngle < 0)
					{
						startAngle += 360;
					}

					while (endAngle < 0)
					{
						endAngle += 360;
					}

					var rect = new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
					var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);

					startAngle *= -1;
					if (!clockwise)
						sweep *= -1;

					platformPath.AddArc(rect, startAngle, sweep);
				}
				else if (type == PathOperation.Close)
				{
					platformPath.Close();
				}
			}

			return platformPath;
		}

		/// <summary>
		/// Converts a PathF to a SkiaSharp SKPath with a specified pixels-per-unit value and zoom level.
		/// </summary>
		/// <param name="path">The PathF to convert.</param>
		/// <param name="ppu">The pixels-per-unit value.</param>
		/// <param name="zoom">The zoom level.</param>
		/// <returns>A SkiaSharp SKPath that corresponds to the specified PathF, scaled by the specified pixels-per-unit value and zoom level.</returns>
		public static SKPath AsSkiaPath(this PathF path, float ppu, float zoom)
		{
			return AsSkiaPath(path, ppu * zoom);
		}

		/// <summary>
		/// Converts a segment of a PathF to a SkiaSharp SKPath with a specified pixels-per-unit value and zoom level.
		/// </summary>
		/// <param name="target">The PathF containing the segment to convert.</param>
		/// <param name="segmentIndex">The index of the segment to convert.</param>
		/// <param name="ppu">The pixels-per-unit value.</param>
		/// <param name="zoom">The zoom level.</param>
		/// <returns>A SkiaSharp SKPath that corresponds to the specified segment of the PathF, scaled by the specified pixels-per-unit value and zoom level.</returns>
		public static SKPath AsSkiaPathFromSegment(this PathF target, int segmentIndex, float ppu, float zoom)
		{
			ppu = zoom * ppu;

			var path = new SKPath();

			var type = target.GetSegmentType(segmentIndex);
			if (type == PathOperation.Line)
			{
				var pointIndex = target.GetSegmentPointIndex(segmentIndex);
				var startPoint = target[pointIndex - 1];
				path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

				var endPoint = target[pointIndex];
				path.LineTo(endPoint.X * ppu, endPoint.Y * ppu);
			}
			else if (type == PathOperation.Quad)
			{
				var pointIndex = target.GetSegmentPointIndex(segmentIndex);
				var startPoint = target[pointIndex - 1];
				path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

				var controlPoint = target[pointIndex++];
				var endPoint = target[pointIndex];
				path.QuadTo(controlPoint.X * ppu, controlPoint.Y * ppu, endPoint.X * ppu, endPoint.Y * ppu);
			}
			else if (type == PathOperation.Cubic)
			{
				var pointIndex = target.GetSegmentPointIndex(segmentIndex);
				var startPoint = target[pointIndex - 1];
				path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

				var controlPoint1 = target[pointIndex++];
				var controlPoint2 = target[pointIndex++];
				var endPoint = target[pointIndex];
				path.CubicTo(controlPoint1.X * ppu, controlPoint1.Y * ppu, controlPoint2.X * ppu, controlPoint2.Y * ppu, endPoint.X * ppu, endPoint.Y * ppu);
			}
			else if (type == PathOperation.Arc)
			{
				target.GetSegmentInfo(segmentIndex, out var pointIndex, out var arcAngleIndex, out var arcClockwiseIndex);

				var topLeft = target[pointIndex++];
				var bottomRight = target[pointIndex];
				var startAngle = target.GetArcAngle(arcAngleIndex++);
				var endAngle = target.GetArcAngle(arcAngleIndex);
				var clockwise = target.GetArcClockwise(arcClockwiseIndex);

				while (startAngle < 0)
				{
					startAngle += 360;
				}

				while (endAngle < 0)
				{
					endAngle += 360;
				}

				var rect = new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
				var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);

				startAngle *= -1;
				if (!clockwise)
					sweep *= -1;

				path.AddArc(rect, startAngle, sweep);
			}

			return path;
		}

		/// <summary>
		/// Converts a PathF to a SkiaSharp SKPath with a specified pixels-per-unit value, zoom level, and rotation angle.
		/// </summary>
		/// <param name="target">The PathF to convert.</param>
		/// <param name="center">The center point around which to rotate the path.</param>
		/// <param name="ppu">The pixels-per-unit value.</param>
		/// <param name="zoom">The zoom level.</param>
		/// <param name="angle">The rotation angle in degrees.</param>
		/// <returns>A SkiaSharp SKPath that corresponds to the specified PathF, scaled by the specified pixels-per-unit value and zoom level, and rotated around the specified center point.</returns>
		public static SKPath AsRotatedAndroidPath(this PathF target, PointF center, float ppu, float zoom, float angle)
		{
			ppu = zoom * ppu;

			var path = new SKPath();

			var pointIndex = 0;
			var arcAngleIndex = 0;
			var arcClockwiseIndex = 0;

			foreach (var type in target.SegmentTypes)
			{
				if (type == PathOperation.Move)
				{
					var point = target.GetRotatedPoint(pointIndex++, center, angle);
					path.MoveTo(point.X * ppu, point.Y * ppu);
				}
				else if (type == PathOperation.Line)
				{
					var endPoint = target.GetRotatedPoint(pointIndex++, center, angle);
					path.LineTo(endPoint.X * ppu, endPoint.Y * ppu);
				}
				else if (type == PathOperation.Quad)
				{
					var controlPoint1 = target.GetRotatedPoint(pointIndex++, center, angle);
					var endPoint = target.GetRotatedPoint(pointIndex++, center, angle);
					path.QuadTo(
						controlPoint1.X * ppu,
						controlPoint1.Y * ppu,
						endPoint.X * ppu,
						endPoint.Y * ppu);
				}
				else if (type == PathOperation.Cubic)
				{
					var controlPoint1 = target.GetRotatedPoint(pointIndex++, center, angle);
					var controlPoint2 = target.GetRotatedPoint(pointIndex++, center, angle);
					var endPoint = target.GetRotatedPoint(pointIndex++, center, angle);
					path.CubicTo(
						controlPoint1.X * ppu,
						controlPoint1.Y * ppu,
						controlPoint2.X * ppu,
						controlPoint2.Y * ppu,
						endPoint.X * ppu,
						endPoint.Y * ppu);
				}
				else if (type == PathOperation.Arc)
				{
					var topLeft = target[pointIndex++];
					var bottomRight = target[pointIndex++];
					var startAngle = target.GetArcAngle(arcAngleIndex++);
					var endAngle = target.GetArcAngle(arcAngleIndex++);
					var clockwise = target.GetArcClockwise(arcClockwiseIndex++);

					while (startAngle < 0)
					{
						startAngle += 360;
					}

					while (endAngle < 0)
					{
						endAngle += 360;
					}

					var rect = new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
					var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);

					startAngle *= -1;
					if (!clockwise)
						sweep *= -1;

					path.AddArc(rect, startAngle, sweep);
				}
				else if (type == PathOperation.Close)
				{
					path.Close();
				}
			}

			return path;
		}

		/// <summary>
		/// Converts a SkiaSharp SKSize to a .NET MAUI Graphics SizeF.
		/// </summary>
		/// <param name="target">The SKSize to convert.</param>
		/// <returns>A .NET MAUI Graphics SizeF that corresponds to the specified SKSize.</returns>
		public static SizeF AsSize(this SKSize target)
		{
			return new SizeF(target.Width, target.Height);
		}

		/// <summary>
		/// Converts a .NET MAUI Graphics SizeF to a SkiaSharp SKSize.
		/// </summary>
		/// <param name="target">The SizeF to convert.</param>
		/// <returns>A SkiaSharp SKSize that corresponds to the specified SizeF.</returns>
		public static SKSize AsSizeF(this SizeF target)
		{
			return new SKSize(target.Width, target.Height);
		}

		/// <summary>
		/// Converts a SkiaSharp SKPoint to a .NET MAUI Graphics PointF.
		/// </summary>
		/// <param name="target">The SKPoint to convert.</param>
		/// <returns>A .NET MAUI Graphics PointF that corresponds to the specified SKPoint.</returns>
		public static PointF AsPointF(this SKPoint target)
		{
			return new PointF(target.X, target.Y);
		}

		/// <summary>
		/// Retrieves the bitmap pattern from a PatternPaint object.
		/// </summary>
		/// <param name="patternPaint">The PatternPaint object.</param>
		/// <param name="scale">The scale factor for the bitmap.</param>
		/// <returns>The bitmap pattern.</returns>
		public static SKBitmap GetPatternBitmap(this PatternPaint patternPaint, float scale = 1)
		{
			var pattern = patternPaint?.Pattern;
			if (pattern == null)
				return null;

			using (var context = new SkiaBitmapExportContext((int)(pattern.Width * scale), (int)(pattern.Height * scale), scale, disposeBitmap: false))
			{
				var canvas = context.Canvas;

				canvas.Scale(scale, scale);
				pattern.Draw(canvas);

				return context.Bitmap;
			}
		}

		/// <summary>
		/// Retrieves the bitmap pattern from a PatternPaint object with specified scale factors.
		/// </summary>
		/// <param name="patternPaint">The PatternPaint object.</param>
		/// <param name="scaleX">The scale factor for the X dimension.</param>
		/// <param name="scaleY">The scale factor for the Y dimension.</param>
		/// <param name="currentFigure">The current figure (optional).</param>
		/// <returns>The bitmap pattern.</returns>
		public static SKBitmap GetPatternBitmap(this PatternPaint patternPaint, float scaleX, float scaleY, object currentFigure)
		{
			var pattern = patternPaint?.Pattern;
			if (pattern == null)
				return null;

			using (var context = new SkiaBitmapExportContext((int)(pattern.Width * scaleX), (int)(pattern.Height * scaleY), 1, disposeBitmap: false))
			{
				var canvas = context.Canvas;

				if (currentFigure != null)
				{
				}

				canvas.Scale(scaleX, scaleY);
				pattern.Draw(canvas);

				if (currentFigure != null)
				{
				}

				//var filename = "/storage/emulated/0/" + pattern.GetType().Name + ".png";
				//System.Console.WriteLine("Writing to :{0}",filename);
				//context.WriteToFile (filename);
				return context.Bitmap;
			}
		}
	}
}
