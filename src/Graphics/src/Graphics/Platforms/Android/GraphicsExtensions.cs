using System;
using System.Numerics;
using Android.Graphics;
using Android.Text;

namespace Microsoft.Maui.Graphics.Platform
{
	public static class GraphicsExtensions
	{
		public static global::Android.Graphics.Color AsColorMultiplyAlpha(this Color target, float alpha)
		{
			var r = (int)(target.Red * 255f);
			var g = (int)(target.Green * 255f);
			var b = (int)(target.Blue * 255f);
			var a = (int)(target.Alpha * alpha * 255f);

			if (a > 255)
			{
				a = 255;
			}

			var color = new global::Android.Graphics.Color(r, g, b, a);
			return color;
		}

		public static global::Android.Graphics.Color AsColor(this Color target)
		{
			var r = (int)(target.Red * 255f);
			var g = (int)(target.Green * 255f);
			var b = (int)(target.Blue * 255f);
			var a = (int)(target.Alpha * 255f);
			return new global::Android.Graphics.Color(r, g, b, a);
		}

		public static Color AsColor(this global::Android.Graphics.Color target)
		{
			var r = (int)target.R;
			var g = (int)target.G;
			var b = (int)target.B;
			var a = (int)target.A;
			return new Color(r, g, b, a);
		}

		public static global::Android.Graphics.RectF AsRectF(this RectF target)
		{
			return new global::Android.Graphics.RectF(target.Left, target.Top, target.Right, target.Bottom);
		}

		public static RectF AsRectangleF(this global::Android.Graphics.RectF target)
		{
			return new RectF(target.Left, target.Top, MathF.Abs(target.Width()), MathF.Abs(target.Height()));
		}

		public static Rect AsRectangle(this global::Android.Graphics.RectF target)
		{
			return new Rect(target.Left, target.Top, MathF.Abs(target.Width()), MathF.Abs(target.Height()));
		}

		public static global::Android.Graphics.RectF AsRectF(this global::Android.Graphics.Rect target)
		{
			return new global::Android.Graphics.RectF(target);
		}

		public static global::Android.Graphics.PointF ToPointF(this PointF target)
		{
			return new global::Android.Graphics.PointF(target.X, target.Y);
		}

		public static Matrix AsMatrix(this Matrix3x2 transform)
		{
			var values = new float[9];

			values[Matrix.MscaleX] = transform.M11;
			values[Matrix.MskewX] = transform.M21;
			values[Matrix.MtransY] = transform.M31;
			values[Matrix.MskewY] = transform.M12;
			values[Matrix.MscaleY] = transform.M22;
			values[Matrix.MtransX] = transform.M32;
			values[Matrix.Mpersp0] = 0; // 6
			values[Matrix.Mpersp1] = 0; // 7
			values[Matrix.Mpersp2] = 1; // 8

			var matrix = new Matrix();
			matrix.SetValues(values);
			return matrix;
		}

		public static Path AsAndroidPath(
			this PathF path,
			float offsetX = 0,
			float offsetY = 0,
			float scaleX = 1,
			float scaleY = 1)
		{
			var platformPath = new Path();

			int pointIndex = 0;
			int arcAngleIndex = 0;
			int arcClockwiseIndex = 0;

			foreach (PathOperation vType in path.SegmentTypes)
			{
				if (vType == PathOperation.Move)
				{
					var point = path[pointIndex++];
					platformPath.MoveTo(offsetX + point.X * scaleX, offsetY + point.Y * scaleY);
				}
				else if (vType == PathOperation.Line)
				{
					var point = path[pointIndex++];
					platformPath.LineTo(offsetX + point.X * scaleX, offsetY + point.Y * scaleY);
				}

				else if (vType == PathOperation.Quad)
				{
					var controlPoint = path[pointIndex++];
					var point = path[pointIndex++];
					platformPath.QuadTo(offsetX + controlPoint.X * scaleX, offsetY + controlPoint.Y * scaleY, offsetX + point.X * scaleX, offsetY + point.Y * scaleY);
				}
				else if (vType == PathOperation.Cubic)
				{
					var controlPoint1 = path[pointIndex++];
					var controlPoint2 = path[pointIndex++];
					var point = path[pointIndex++];
					platformPath.CubicTo(offsetX + controlPoint1.X * scaleX, offsetY + controlPoint1.Y * scaleY, offsetX + controlPoint2.X * scaleX, offsetY + controlPoint2.Y * scaleY, offsetX + point.X * scaleX,
						offsetY + point.Y * scaleY);
				}
				else if (vType == PathOperation.Arc)
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

					var rect = new global::Android.Graphics.RectF(offsetX + topLeft.X * scaleX, offsetY + topLeft.Y * scaleY, offsetX + bottomRight.X * scaleX, offsetY + bottomRight.Y * scaleY);
					var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);

					startAngle *= -1;
					if (!clockwise)
					{
						sweep *= -1;
					}

					platformPath.ArcTo(rect, startAngle, sweep);
				}
				else if (vType == PathOperation.Close)
				{
					platformPath.Close();
				}
			}

			return platformPath;
		}

		public static Path AsAndroidPath(this PathF path, float ppu, float zoom)
		{
			return AsAndroidPath(path, ppu * zoom);
		}

		public static Path AsAndroidPathFromSegment(this PathF target, int segmentIndex, float ppu, float zoom)
		{
			ppu = zoom * ppu;

			var path = new Path();

			var type = target.GetSegmentType(segmentIndex);
			if (type == PathOperation.Line)
			{
				int pointIndex = target.GetSegmentPointIndex(segmentIndex);
				var startPoint = target[pointIndex - 1];
				path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

				var endPoint = target[pointIndex];
				path.LineTo(endPoint.X * ppu, endPoint.Y * ppu);
			}
			else if (type == PathOperation.Quad)
			{
				int pointIndex = target.GetSegmentPointIndex(segmentIndex);
				var startPoint = target[pointIndex - 1];
				path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

				var controlPoint = target[pointIndex++];
				var endPoint = target[pointIndex];
				path.QuadTo(controlPoint.X * ppu, controlPoint.Y * ppu, endPoint.X * ppu, endPoint.Y * ppu);
			}
			else if (type == PathOperation.Cubic)
			{
				int pointIndex = target.GetSegmentPointIndex(segmentIndex);
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

				var rect = new global::Android.Graphics.RectF(topLeft.X * ppu, topLeft.Y * ppu, bottomRight.X * ppu, bottomRight.Y * ppu);
				var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);

				startAngle *= -1;
				if (!clockwise)
				{
					sweep *= -1;
				}

				path.ArcTo(rect, startAngle, sweep);
			}

			return path;
		}

		public static Path AsRotatedAndroidPath(this PathF target, PointF center, float ppu, float zoom, float angle)
		{
			ppu = zoom * ppu;

			var path = new Path();

			int pointIndex = 0;
			int arcAngleIndex = 0;
			int arcClockwiseIndex = 0;

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

					var rect = new global::Android.Graphics.RectF(topLeft.X * ppu, topLeft.Y * ppu, bottomRight.X * ppu, bottomRight.Y * ppu);
					var sweep = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);

					startAngle *= -1;
					if (!clockwise)
					{
						sweep *= -1;
					}

					path.ArcTo(rect, startAngle, sweep);
				}
				else if (type == PathOperation.Close)
				{
					path.Close();
				}
			}

			return path;
		}

		public static PointF AsPointF(this global::Android.Graphics.PointF target)
		{
			return new PointF(target.X, target.Y);
		}

		public static Point AsPoint(this global::Android.Graphics.PointF target)
		{
			return new Point(target.X, target.Y);
		}

		public static SizeF AsSizeF(this global::Android.Util.SizeF target)
		{
			return new SizeF(target.Width, target.Height);
		}

		public static Size AsSize(this global::Android.Util.SizeF target)
		{
			return new Size(target.Width, target.Height);
		}

		public static Bitmap GetPatternBitmap(this PatternPaint patternPaint, float scale = 1)
		{
			var pattern = patternPaint?.Pattern;
			if (pattern == null)
				return null;

			using (var context = new PlatformBitmapExportContext((int)(pattern.Width * scale), (int)(pattern.Height * scale), scale, disposeBitmap: false))
			{
				var canvas = context.Canvas;
				canvas.Scale(scale, scale);
				pattern.Draw(canvas);
				return context.Bitmap;
			}
		}

		public static Bitmap GetPatternBitmap(
			this PatternPaint patternPaint,
			float scaleX,
			float scaleY)
		{
			var pattern = patternPaint?.Pattern;
			if (pattern == null)
				return null;

			using (var context = new PlatformBitmapExportContext((int)(pattern.Width * scaleX), (int)(pattern.Height * scaleY), disposeBitmap: false))
			{
				var scalingCanvas = new ScalingCanvas(context.Canvas);
				scalingCanvas.Scale(scaleX, scaleY);

				pattern.Draw(scalingCanvas);

				return context.Bitmap;
			}
		}

		public static SizeF GetTextSizeAsSizeF(this StaticLayout target, bool hasBoundedWidth)
		{
			// We need to know if the static layout was created with a bounded width, as this is what
			// StaticLayout.Width returns.
			if (hasBoundedWidth)
				return new SizeF(target.Width, target.Height);

			float maxWidth = 0;
			int lineCount = target.LineCount;

			for (int i = 0; i < lineCount; i++)
			{
				float lineWidth = target.GetLineWidth(i);
				if (lineWidth > maxWidth)
				{
					maxWidth = lineWidth;
				}
			}

			return new SizeF(maxWidth, target.Height);
		}

		public static SizeF GetOffsetsToDrawText(
			this StaticLayout target,
			float x,
			float y,
			float width,
			float height,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment)
		{
			if (verticalAlignment != VerticalAlignment.Top)
			{
				SizeF vTextFrameSize = target.GetTextSize();

				float vOffsetX = 0;
				float vOffsetY = 0;

				if (height > 0)
				{
					if (verticalAlignment == VerticalAlignment.Bottom)
						vOffsetY = height - vTextFrameSize.Height;
					else
						vOffsetY = (height - vTextFrameSize.Height) / 2;
				}

				return new SizeF(x + vOffsetX, y + vOffsetY);
			}

			return new SizeF(x, y);
		}

		public static Bitmap Downsize(this Bitmap target, int maxSize, bool dispose = true)
		{
			return Downsize(target, maxSize, maxSize, dispose);
		}

		public static Bitmap Downsize(this Bitmap target, int maxWidth, int maxHeight, bool dispose = true)
		{
			if (maxWidth <= 0 || maxHeight <= 0)
			{
				return target;
			}

			if (target.Width > maxWidth || target.Height > maxHeight)
			{
				float factorX = maxWidth / (float)target.Width;
				float factorY = maxHeight / (float)target.Height;

				float factor = Math.Min(factorX, factorY);

				maxWidth = (int)Math.Round(factor * target.Width);
				maxHeight = (int)Math.Round(factor * target.Height);

				var newImage = Bitmap.CreateScaledBitmap(target, maxWidth, maxHeight, true);
				if (dispose)
				{
					target.Recycle();
					target.Dispose();
				}

				return newImage;
			}

			return target;
		}
	}
}
