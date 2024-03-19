using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Platform
{
	public static class StrokeExtensions
	{
		static (string mainNode, string subNode) GetBorderCssNode(this BorderView platformView)
		{
			var mainNode = platformView.CssMainNode();

			return (mainNode, $"border,\n.{mainNode}");
		}

		[MissingMapper]
		public static void UpdateStroke(this BorderView platformView, IBorderStroke border)
		{
			var (mainNode, subnode) = platformView.GetBorderCssNode();
			platformView.SetStyleColor(border.Stroke.ToColor(), mainNode, "border-color", subnode);
			platformView.SetStyleValueNode($"{(int)border.StrokeThickness}px", mainNode, "border-width", subnode);
			if (border.StrokeDashPattern is { Length: > 1 })
			{
				if (border.StrokeDashPattern[0] > 1)
					platformView.SetStyleValueNode($"{BorderStyle.Dashed}", mainNode, "border-style", subnode);
				else
					platformView.SetStyleValueNode($"{BorderStyle.Dotted}", mainNode, "border-style", subnode);
			}
			else
			{
				platformView.SetStyleValueNode($"{BorderStyle.Solid}", mainNode, "border-style", subnode);
			}

			// no effect:
			// platformView.SetStyleValueNode("content-box", mainNode, "background-clip");
		}

		// radius can be calculated from path
		static (PointF from, PointF to)[] Corners(IRoundRectangle shape, Rect bounds)
		{
			using var path = shape.PathForBounds(bounds);

			var corners = new (PointF from, PointF to)[4];
			var iCorner = 0;
			PointF fromP = default;
			PointF toP = default;
			for (var i = 0; i < path.OperationCount; i++)
			{
				var type = path.GetSegmentType(i);
				var points = path.GetPointsForSegment(i);

				if (type == PathOperation.Cubic)
				{
					toP = points[2];
				}

				if (type == PathOperation.Move)
				{
					fromP = points[0];
				}

				if (type == PathOperation.Line)
				{
					corners[iCorner] = (fromP, toP);
					iCorner++;
					if (iCorner > corners.Length)
						break;
					fromP = points[0];
				}
			}

			corners[iCorner] = (fromP, toP);

			return corners;
		}

		static float[] Radii((PointF from, PointF to)[] corners)
		{
			float cornerOffset(int i) => Math.Abs(corners[i].to.X - corners[i].from.X);

			return [cornerOffset(0), cornerOffset(1), cornerOffset(2), cornerOffset(3)];
		}

		[MissingMapper]
		public static void UpdateStrokeShape(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			if (border.Shape is { } shape)
			{
				var (mainNode, subnode) = borderView.GetBorderCssNode();

				// border-top-left-radius, border-top-right-radius, border-bottom-right-radius, border-bottom-left-radius
				void SetRadius(float radius, float? tr = default, float? br = default, float? bl = default)
				{
					if (tr is not { } || br is not { } || bl is not { } ||
					    (radius == tr && radius == br && radius == bl))
					{
						platformView.SetStyleValueNode($"{radius}px", mainNode, "border-radius", subnode);
						return;
					}

					platformView.SetStyleValueNode($"{(int)radius}px", mainNode, "border-top-left-radius", subnode);
					platformView.SetStyleValueNode($"{(int)tr}px", mainNode, "border-top-right-radius", subnode);
					platformView.SetStyleValueNode($"{(int)br}px", mainNode, "border-bottom-right-radius", subnode);
					platformView.SetStyleValueNode($"{(int)bl}px", mainNode, "border-bottom-left-radius", subnode);
				}

				if (border.Shape is IRoundRectangle roundRectangle)
				{
					var radii = Radii(Corners(roundRectangle, platformView.Allocation.ToRect()));

					SetRadius(radii[0], radii[1], radii[2], radii[3]);
				}
				else
				{
					SetRadius(0);
				}
			}
		}

		[MissingMapper]
		public static void UpdateStroke(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeThickness(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeDashPattern(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeDashOffset(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeMiterLimit(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeLineCap(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}

		[MissingMapper]
		public static void UpdateStrokeLineJoin(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			UpdateStroke(borderView, border);
		}
	}
}