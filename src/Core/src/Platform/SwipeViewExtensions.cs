using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static partial class SwipeViewExtensions
	{
		internal const int SwipeThreshold = 250;
		internal const int SwipeItemWidth = 100;

		public static Color? GetTextColor(this ISwipeItemMenuItem swipeItemMenuItem)
		{
			Color? backgroundColor = swipeItemMenuItem.Background?.ToColor();

			if (backgroundColor == null || (swipeItemMenuItem.Source is IFontImageSource fontImageSource && fontImageSource.Color != null))
				return null;

			var luminosity = 0.2126f * backgroundColor.Red + 0.7152f * backgroundColor.Green + 0.0722f * backgroundColor.Blue;

			return (luminosity < 0.75f ? Colors.White : Colors.Black);
		}

		internal static ISwipeItems? GetSwipeItemsByDirection(this ISwipeView swipeView, SwipeDirection? swipeDirection)
		{
			ISwipeItems? swipeItems = null;

			switch (swipeDirection)
			{
				case SwipeDirection.Left:
					swipeItems = swipeView.RightItems;
					break;
				case SwipeDirection.Right:
					swipeItems = swipeView.LeftItems;
					break;
				case SwipeDirection.Up:
					swipeItems = swipeView.BottomItems;
					break;
				case SwipeDirection.Down:
					swipeItems = swipeView.TopItems;
					break;
			}

			return swipeItems;
		}

		internal static bool IsHorizontalSwipe(this SwipeDirection? swipeDirection)
		{
			return swipeDirection == SwipeDirection.Left || swipeDirection == SwipeDirection.Right;
		}
	}
}
