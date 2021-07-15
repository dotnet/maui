using System;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public partial class NativeCanvas {

		private Cairo.Surface CreateSurface(Cairo.Context context, bool imageSurface = false) {
			var surface = context.GetTarget();

			var extents = context.PathExtents();
			var pathSize = new Size(extents.X + extents.Width, extents.Height + extents.Y);

			var s = surface.GetSize();

			var shadowSurface = s.HasValue && !imageSurface ?
				surface.CreateSimilar(surface.Content, (int) pathSize.Width, (int) pathSize.Height) :
				new Cairo.ImageSurface(Cairo.Format.ARGB32, (int) pathSize.Width, (int) pathSize.Height);

			return shadowSurface;

		}

		private void AddLine(Cairo.Context context, float x1, float y1, float x2, float y2) {
			context.MoveTo(x1, y1);
			context.LineTo(x2, y2);
		}

		private void AddArc(Cairo.Context context, float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed) {

			AddArc(context, x, y, width, height, startAngle, endAngle, clockwise);

			if (closed)
				context.ClosePath();
		}

		private void AddRectangle(Cairo.Context context, float x, float y, float width, float height) {
			context.Rectangle(x, y, width, height);
		}

		/// <summary>
		/// degree-value * mRadians = radians
		/// </summary>
		private const double mRadians = System.Math.PI / 180d;

		private void AddRoundedRectangle(Cairo.Context context, float left, float top, float width, float height, float radius) {

			context.NewPath();
			// top left
			context.Arc(left + radius, top + radius, radius, 180 * mRadians, 270 * mRadians);
			// // top right
			context.Arc(left + width - radius, top + radius, radius, 270 * mRadians, 0);
			// // bottom right
			context.Arc(left + width - radius, top + height - radius, radius, 0, 90 * mRadians);
			// // bottom left
			context.Arc(left + radius, top + height - radius, radius, 90 * mRadians, 180 * mRadians);
			context.ClosePath();
		}

		public void AddEllipse(Cairo.Context context, float x, float y, float width, float height) {
			context.Save();
			context.NewPath();

			context.Translate(x + width / 2, y + height / 2);
			context.Scale(width / 2f, height / 2f);
			context.Arc(0, 0, 1, 0, 2 * Math.PI);
			context.Restore();
		}

		private void AddArc(Cairo.Context context, float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise) {

			// https://developer.gnome.org/cairo/stable/cairo-Paths.html#cairo-arc
			// Angles are measured in radians

			var startAngleInRadians = startAngle * -mRadians;
			var endAngleInRadians = endAngle * -mRadians;

			var cx = x + width / 2f;
			var cy = y + height / 2f;

			var r = 1;

			context.Save();

			context.Translate(cx, cy);
			context.Scale(width / 2f, height / 2f);

			if (clockwise)
				context.Arc(0, 0, r, startAngleInRadians, endAngleInRadians);
			else {
				context.ArcNegative(0, 0, r, startAngleInRadians, endAngleInRadians);
			}

			context.Restore();

		}

		private void AddPath(Cairo.Context context, PathF target) {
			var pointIndex = 0;
			var arcAngleIndex = 0;
			var arcClockwiseIndex = 0;

			foreach (var type in target.SegmentTypes) {
				if (type == PathOperation.Move) {
					var point = target[pointIndex++];
					context.MoveTo(point.X, point.Y);
				} else if (type == PathOperation.Line) {
					var endPoint = target[pointIndex++];
					context.LineTo(endPoint.X, endPoint.Y);

				} else if (type == PathOperation.Quad) {
					var p1 = pointIndex > 0 ? target[pointIndex - 1] : context.CurrentPoint.ToPointF();
					var c = target[pointIndex++];
					var p2 = target[pointIndex++];

					// quad bezier to cubic bezier:
					// C1 = 2/3•C + 1/3•P1
					// C2 = 2/3•C + 1/3•P2

					var c1 = new PointF(c.X * 2 / 3 + p1.X / 3, c.Y * 2 / 3 + p1.Y / 3);
					var c2 = new PointF(c.X * 2 / 3 + p2.X / 3, c.Y * 2 / 3 + p2.Y / 3);

					// Adds a cubic Bézier spline to the path
					context.CurveTo(
						c1.X, c1.Y,
						c2.X, c2.Y,
						p2.X, p2.Y);

				} else if (type == PathOperation.Cubic) {
					var controlPoint1 = target[pointIndex++];
					var controlPoint2 = target[pointIndex++];
					var endPoint = target[pointIndex++];

					// https://developer.gnome.org/cairo/stable/cairo-Paths.html#cairo-curve-to
					// Adds a cubic Bézier spline to the path from the current point to position (x3, y3) in user-space coordinates,
					// using (x1, y1) and (x2, y2) as the control points. After this call the current point will be (x3, y3).
					// If there is no current point before the call to cairo_curve_to() this function will behave as if preceded by a call to cairo_move_to(cr, x1, y1).
					context.CurveTo(
						controlPoint1.X, controlPoint1.Y,
						controlPoint2.X, controlPoint2.Y,
						endPoint.X, endPoint.Y);

				} else if (type == PathOperation.Arc) {
					var topLeft = target[pointIndex++];
					var bottomRight = target[pointIndex++];
					var startAngle = target.GetArcAngle(arcAngleIndex++);
					var endAngle = target.GetArcAngle(arcAngleIndex++);
					var clockwise = target.GetArcClockwise(arcClockwiseIndex++);

					AddArc(context, topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y, startAngle, endAngle, clockwise);

				} else if (type == PathOperation.Close) {
					context.ClosePath();
				}
			}
		}

		public void DrawPixbuf(Cairo.Context context, Gdk.Pixbuf pixbuf, double x, double y, double width, double height) {
			context.Save();
			context.Translate(x, y);

			context.Scale(width / pixbuf.Width, height / pixbuf.Height);
			Gdk.CairoHelper.SetSourcePixbuf(context, pixbuf, 0, 0);

			using (var p = context.GetSource()) {
				if (p is Cairo.SurfacePattern pattern) {
					if (width > pixbuf.Width || height > pixbuf.Height) {
						// Fixes blur issue when rendering on an image surface
						pattern.Filter = Cairo.Filter.Fast;
					} else
						pattern.Filter = Cairo.Filter.Good;
				}
			}

			context.Paint();

			context.Restore();
		}

	}

}
