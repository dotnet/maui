using ElmSharp;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class ScrollViewExtensions
	{
		public static void UpdateVerticalScrollBarVisibility(this ScrollView scrollView, ScrollBarVisibility scrollBarVisibility)
		{
			scrollView.VerticalScrollBarVisiblePolicy = scrollBarVisibility.ToNative();
		}

		public static void UpdateHorizontalScrollBarVisibility(this ScrollView scrollView, ScrollBarVisibility scrollBarVisibility)
		{
			scrollView.HorizontalScrollBarVisiblePolicy = scrollBarVisibility.ToNative();
		}

		public static void UpdateOrientation(this ScrollView scrollView, ScrollOrientation scrollOrientation)
		{
			switch (scrollOrientation)
			{
				case ScrollOrientation.Horizontal:
					scrollView.ScrollBlock = ScrollBlock.Vertical;
					scrollView.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					scrollView.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;
					break;
				case ScrollOrientation.Vertical:
					scrollView.ScrollBlock = ScrollBlock.Horizontal;
					scrollView.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible;
					scrollView.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					break;
				default:
					scrollView.ScrollBlock = ScrollBlock.None;
					scrollView.HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					scrollView.VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Auto;
					break;
			}
		}

		public static ScrollBarVisiblePolicy ToNative(this ScrollBarVisibility visibility)
		{
			switch (visibility)
			{
				case ScrollBarVisibility.Default:
					return ScrollBarVisiblePolicy.Auto;
				case ScrollBarVisibility.Always:
					return ScrollBarVisiblePolicy.Visible;
				case ScrollBarVisibility.Never:
					return ScrollBarVisiblePolicy.Invisible;
				default:
					return ScrollBarVisiblePolicy.Auto;
			}
		}
	}
}