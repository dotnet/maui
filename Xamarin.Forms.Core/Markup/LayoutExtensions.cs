using static Xamarin.Forms.Core.Markup.Markup;

namespace Xamarin.Forms.Markup
{
	public static class LayoutExtensions
	{
		public static TLayout Padding<TLayout>(this TLayout layout, Thickness padding) where TLayout : Layout
		{ VerifyExperimental(); layout.Padding = padding; return layout; }

		public static TLayout Padding<TLayout>(this TLayout layout, double horizontalSize, double verticalSize) where TLayout : Layout
		{ VerifyExperimental(); layout.Padding = new Thickness(horizontalSize, verticalSize); return layout; }

		public static TLayout Paddings<TLayout>(this TLayout layout, double left = 0, double top = 0, double right = 0, double bottom = 0) where TLayout : Layout
		{ VerifyExperimental(); layout.Padding = new Thickness(left, top, right, bottom); return layout; }
	}
}