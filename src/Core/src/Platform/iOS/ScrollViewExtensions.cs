using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class ScrollViewExtensions
	{
		public static void UpdateVerticalScrollBarVisibility(this UIScrollView scrollView, ScrollBarVisibility scrollBarVisibility)
		{
			scrollView.ShowsVerticalScrollIndicator = scrollBarVisibility == ScrollBarVisibility.Always || scrollBarVisibility == ScrollBarVisibility.Default;
		}

		public static void UpdateHorizontalScrollBarVisibility(this UIScrollView scrollView, ScrollBarVisibility scrollBarVisibility)
		{
			scrollView.ShowsHorizontalScrollIndicator = scrollBarVisibility == ScrollBarVisibility.Always || scrollBarVisibility == ScrollBarVisibility.Default;
		}

		public static void UpdateContent(this UIScrollView scrollView, UIView content)
		{
			if (scrollView.Subviews.Length > 0 && scrollView.Subviews[0] == content)
			{
				return;
			}

			if (scrollView.Subviews.Length > 0)
			{
				// TODO ezhart Are we sure this is always the correct index? The scroll indicators might be in here, too.
				scrollView.Subviews[0].RemoveFromSuperview();
			}

			scrollView.AddSubview(content);
		}

		public static void UpdateContentSize(this UIScrollView scrollView, Size contentSize)
		{
			var nativeContentSize = contentSize.ToCGSize();

			if (nativeContentSize != scrollView.ContentSize)
			{
				scrollView.ContentSize = nativeContentSize;
			}
		}
	}
}
