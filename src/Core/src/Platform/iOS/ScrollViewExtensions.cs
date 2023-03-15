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
		public static void UpdateContent(this UIScrollView scrollView, IView? content, IMauiContext context)
		{
			var nativeContent = content == null ? null : content.ToPlatform(context);

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

		public static void UpdateIsEnabled(this UIScrollView nativeScrollView, IScrollView scrollView)
		{
			nativeScrollView.ScrollEnabled = scrollView.IsEnabled;
		}

		internal static void UpdateOrientation(this UIScrollView platformScrollView, IScrollView scrollView)
		{
			var bounds = platformScrollView.GetPlatformViewBounds();
			var widthConstraint = bounds.Width;
			var heightConstraint = bounds.Height;
			var result = scrollView.CrossPlatformMeasure(widthConstraint, heightConstraint);
			var contentSize = ScrollViewHandler.AccountForOrientation(result, widthConstraint, heightConstraint, scrollView.Orientation);
			platformScrollView.ContentSize = contentSize;
		}
	}
}
