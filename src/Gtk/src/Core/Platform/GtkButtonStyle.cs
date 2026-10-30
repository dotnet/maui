using System.Globalization;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// The border (<see cref="IButtonStroke"/>) styling shared by the button-like handlers (Button, ImageButton, RadioButton), as CSS on the widget.
	/// </summary>
	internal static class GtkButtonStyle
	{
		/// <summary>Sets the CSS border from the stroke thickness and colour; a missing colour or thickness clears it.</summary>
		public static void UpdateStroke(global::Gtk.Widget widget, IButtonStroke stroke)
		{
			var style = GtkStyle.For(widget);
			double thickness = stroke.StrokeThickness;

			if (thickness > 0 && stroke.StrokeColor is { } color)
			{
				style.Set("border-style", "solid");
				style.Set("border-width", thickness.ToString("0.##", CultureInfo.InvariantCulture) + "px");
				style.Set("border-color", color.ToCss());
			}
			else
			{
				style.ClearPrefix("border-style");
				style.ClearPrefix("border-width");
				style.ClearPrefix("border-color");
			}
		}

		/// <summary>Sets the CSS <c>border-radius</c>; a negative radius (the platform default) clears it.</summary>
		public static void UpdateCornerRadius(global::Gtk.Widget widget, IButtonStroke stroke) =>
			GtkStyle.For(widget).Set("border-radius", stroke.CornerRadius >= 0 ? stroke.CornerRadius.ToString(CultureInfo.InvariantCulture) + "px" : null);
	}
}
