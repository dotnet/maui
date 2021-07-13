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
			var oldChildren = scrollView.Subviews;
			foreach (var child in oldChildren)
			{
				child.RemoveFromSuperview();
			}

			scrollView.AddSubview(content);
		}
	}
}
