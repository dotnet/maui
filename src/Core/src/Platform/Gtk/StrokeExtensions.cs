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

		}

		[MissingMapper]
		public static void UpdateStrokeShape(this Gtk.Widget platformView, IBorderStroke border)
		{
			if (platformView is not BorderView borderView)
				return;

			if (border.Shape is { } shape)
			{
				var (mainNode, subnode) = borderView.GetBorderCssNode();

				if (border.Shape is IRoundRectangle roundRectangle)
				{

					// border-top-left-radius, border-top-right-radius, border-bottom-right-radius, border-bottom-left-radius
					var path = shape.PathForBounds(platformView.Allocation.ToRect());
					platformView.SetStyleValueNode($"5px", mainNode, "border-radius", subnode);
					path.Dispose();

				}
				else
				{
					platformView.SetStyleValueNode($"0px", mainNode, "border-radius", subnode);
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