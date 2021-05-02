using Gtk;

namespace Microsoft.Maui
{

	public static class ThicknessExtensions
	{

		public static Thickness ToThickness(this Border it)
			=> new(it.Left, it.Top, it.Right, it.Bottom);

		public static Border ToNative(this Thickness it)
			=> new() { Left = (short)it.Left, Top = (short)it.Top, Right = (short)it.Right, Bottom = (short)it.Bottom };

		public static TWidget? WithMargin<TWidget>(this TWidget? it, Thickness margin) where TWidget : Widget
		{
			if (it == default)
				return it;

			// https://developer.gnome.org/gtk3/stable/GtkWidget.html#GtkWidget--margin-start			
			it.MarginStart = (int)margin.Left;
			it.MarginTop = (int)margin.Top;
			it.MarginEnd = (int)margin.Right;
			it.MarginBottom = (int)margin.Bottom;

			return it;
		}

		public static Thickness GetMargin<TWidget>(this TWidget? it) where TWidget : Widget =>
			it == default ? default : new Thickness(it.MarginStart, it.MarginTop, it.MarginEnd, it.MarginBottom);
		
		public static Thickness GetPadding<TWidget>(this TWidget? it) where TWidget : Widget
		{

			return it?.StyleContext.GetPadding(StateFlags.Normal).ToThickness() ?? default;
		}

	}

}