using Gtk;

namespace Microsoft.Maui
{

	public static class ThicknessExtensions
	{

		public static Thickness ToThickness(this Border it)
			=> new(it.Left, it.Top, it.Right, it.Bottom);

		public static Border ToPlatform(this Thickness it)
			=> new() { Left = (short)it.Left, Top = (short)it.Top, Right = (short)it.Right, Bottom = (short)it.Bottom };

		public static TWidget? WithPadding<TWidget>(this TWidget? it, Thickness padding) where TWidget : Widget
		{
			if (it == default)
				return it;

			// https://docs.gtk.org/gtk3/property.Widget.margin-start.html			
			it.MarginStart = (int)padding.Left;
			it.MarginTop = (int)padding.Top;
			it.MarginEnd = (int)padding.Right;
			it.MarginBottom = (int)padding.Bottom;

			return it;
		}

		public static Thickness GetPadding<TWidget>(this TWidget? it) where TWidget : Widget =>
			it == default ? default : new Thickness(it.MarginStart, it.MarginTop, it.MarginEnd, it.MarginBottom);
		
		public static Thickness GetPaddingThickness<TWidget>(this TWidget? it) where TWidget : Widget
		{

			return it?.StyleContext.GetPadding(StateFlags.Normal).ToThickness() ?? default;
		}

	}

}