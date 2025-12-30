using System;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
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


		// TODO ezhart This method is no longer used internally; we can't delete it right now because that'd be a breaking change
		[Obsolete]
		public static void UpdateContent(this UIScrollView scrollView, IView? content, IMauiContext context)
		{
			var nativeContent = content?.ToPlatform(context);

			if (scrollView.Subviews.Length > 0 && scrollView.Subviews[0] == nativeContent)
			{
				return;
			}

			if (scrollView.Subviews.Length > 0)
			{
				// TODO ezhart Are we sure this is always the correct index? The scroll indicators might be in here, too.
				scrollView.Subviews[0].RemoveFromSuperview();
			}

			if (nativeContent != null)
			{
				scrollView.AddSubview(nativeContent);
			}
		}

		internal static UIView? GetContentView(this UIScrollView scrollView)
		{
			for (int i = 0; i < scrollView.Subviews.Length; i++)
			{
				if (scrollView.Subviews[i] is { Tag: MauiScrollView.ContentTag } contentView)
				{
					return contentView;
				}
			}

			return null;
		}

		public static void UpdateContentSize(this UIScrollView scrollView, Size contentSize)
		{
			var nativeContentSize = contentSize.ToCGSize();

			if (nativeContentSize != scrollView.ContentSize)
			{
				scrollView.ContentSize = nativeContentSize;
			}
		}

		public static void UpdateIsEnabled(this UIScrollView nativeScrollView, IScrollView scrollView)
		{
			if (scrollView.Orientation == ScrollOrientation.Neither)
			{
				nativeScrollView.ScrollEnabled = false;
			}
			else
			{
				nativeScrollView.ScrollEnabled = scrollView.IsEnabled;
			}
		}
	}
}
