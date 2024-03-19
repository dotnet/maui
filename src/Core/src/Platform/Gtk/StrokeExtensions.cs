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
		}

		[MissingMapper]
		public static void UpdateStrokeShape(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			if (border.Shape is { } shape)
			{
				var (mainNode, subnode) = borderView.GetBorderCssNode();

				void SetRadius(int radius)
				{
					platformView.SetStyleValueNode($"{radius}px", mainNode, "border-radius", subnode);

					if (border.StrokeThickness == 0)
					{
						platformView.SetStyleValueNode($"{radius * 2}px", mainNode, "border-width", subnode);
					}
				}

				if (border.Shape is IRoundRectangle roundRectangle)
				{
					// border-top-left-radius, border-top-right-radius, border-bottom-right-radius, border-bottom-left-radius
					// maybe radius can be calculated from path??:
					var path = shape.PathForBounds(platformView.Allocation.ToRect());
					path.Dispose();

					SetRadius(5);
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