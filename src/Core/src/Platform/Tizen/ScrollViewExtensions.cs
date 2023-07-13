using Tizen.UIExtensions.NUI;
using TScrollBarVisibility = Tizen.UIExtensions.Common.ScrollBarVisibility;
using TScrollOrientation = Tizen.UIExtensions.Common.ScrollOrientation;

namespace Microsoft.Maui.Platform
{
	public static class ScrollViewExtensions
	{
		public static void UpdateVerticalScrollBarVisibility(this ScrollView scrollView, ScrollBarVisibility scrollBarVisibility)
		{
			scrollView.VerticalScrollBarVisibility = scrollBarVisibility.ToPlatform();
		}

		public static void UpdateHorizontalScrollBarVisibility(this ScrollView scrollView, ScrollBarVisibility scrollBarVisibility)
		{
			scrollView.HorizontalScrollBarVisibility = scrollBarVisibility.ToPlatform();
		}

		public static void UpdateOrientation(this ScrollView scrollView, ScrollOrientation scrollOrientation)
		{
			scrollView.ScrollOrientation = scrollOrientation.ToNative();
		}

		public static TScrollOrientation ToNative(this ScrollOrientation scrollOrientation)
		{
			switch (scrollOrientation)
			{
				case ScrollOrientation.Horizontal:
					return TScrollOrientation.Horizontal;
				case ScrollOrientation.Vertical:
					return TScrollOrientation.Vertical;
				default:
					return TScrollOrientation.Both;
			}
		}

		public static TScrollBarVisibility ToPlatform(this ScrollBarVisibility visibility)
		{
			switch (visibility)
			{
				case ScrollBarVisibility.Default:
					return TScrollBarVisibility.Default;
				case ScrollBarVisibility.Always:
					return TScrollBarVisibility.Always;
				case ScrollBarVisibility.Never:
					return TScrollBarVisibility.Never;
				default:
					return TScrollBarVisibility.Default;
			}
		}
	}
}