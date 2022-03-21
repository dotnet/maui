using System;
using Android.Views;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Layouts.LayoutExtensions;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, MauiScrollView>
	{
		const string InsetPanelTag = "MAUIContentInsetPanel";

		protected override MauiScrollView CreatePlatformView()
		{
			var scrollView = new MauiScrollView(
				new Android.Views.ContextThemeWrapper(MauiContext!.Context, Resource.Style.scrollViewTheme), null!,
					Resource.Attribute.scrollViewStyle);

			scrollView.ClipToOutline = true;

			return scrollView;
		}

		protected override void ConnectHandler(MauiScrollView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ScrollChange += ScrollChange;
		}

		protected override void DisconnectHandler(MauiScrollView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.ScrollChange -= ScrollChange;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var result = base.GetDesiredSize(widthConstraint, heightConstraint);

			if (FindInsetPanel(this) == null)
			{
				VirtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);
			}

			return result;
		}

		void ScrollChange(object? sender, AndroidX.Core.Widget.NestedScrollView.ScrollChangeEventArgs e)
		{
			var context = (sender as View)?.Context;

			if (context == null)
			{
				return;
			}

			VirtualView.VerticalOffset = Context.FromPixels(e.ScrollY);
			VirtualView.HorizontalOffset = Context.FromPixels(e.ScrollX);
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.PlatformView == null || handler.MauiContext == null)
				return;

			if (NeedsInsetView(scrollView))
			{
				UpdateInsetView(scrollView, handler);
			}
			else
			{
				handler.PlatformView.UpdateContent(scrollView.PresentedContent, handler.MauiContext);
			}
		}

		public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetHorizontalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetVerticalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapOrientation(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetOrientation(scrollView.Orientation);
		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is not ScrollToRequest request)
			{
				return;
			}

			var context = handler.PlatformView.Context;

			if (context == null)
			{
				return;
			}

			var horizontalOffsetDevice = (int)context.ToPixels(request.HoriztonalOffset);
			var verticalOffsetDevice = (int)context.ToPixels(request.VerticalOffset);

			handler.PlatformView.ScrollTo(horizontalOffsetDevice, verticalOffsetDevice,
				request.Instant, () => handler.VirtualView.ScrollFinished());
		}

		/*
			Problem 1: Android treats Padding differently than what we want for MAUI; Padding creates space
			_around_ the scrollable area, rather than padding the content inside of it. 
			
			Problem 2: The Android ScrollView control will ignore the cross-platform Margin of its content when 
			making native Measure calls. The internal content size values recorded by the native ScrollView will 
			not account for the margin, and the control won't scroll all the way to the bottom of the content. 

			To handle both issues, we detect whether the content has a Margin or the cross-platform ScrollView has a Padding;
			if so, we insert a container ContentViewGroup which always lays out at the origin but provides both the Padding
			and the Margin for the content. The extra layer is only inserted if necessary, and is removed if the Padding
			and Margin are set to zero. The extra layer uses the native ContentViewGroup control (the same one we already 
			use as the backing for ContentView, Page, etc.). 

			The methods below exist to support inserting/updating the extra padding/margin layer.
		*/

		static bool NeedsInsetView(IScrollView scrollView)
		{
			if (scrollView.PresentedContent == null)
			{
				return false;
			}

			if (scrollView.Padding != Thickness.Zero)
			{
				return true;
			}

			if (scrollView.PresentedContent.Margin != Thickness.Zero)
			{
				return true;
			}

			return false;
		}

		static ContentViewGroup? FindInsetPanel(IScrollViewHandler handler)
		{
			return handler.PlatformView.FindViewWithTag(InsetPanelTag) as ContentViewGroup;
		}

		static void UpdateInsetView(IScrollView scrollView, IScrollViewHandler handler)
		{
			if (scrollView.PresentedContent == null || handler.MauiContext == null)
			{
				return;
			}

			var nativeContent = scrollView.PresentedContent.ToPlatform(handler.MauiContext);

			if (FindInsetPanel(handler) is ContentViewGroup currentPaddingLayer)
			{
				if (currentPaddingLayer.ChildCount == 0 || currentPaddingLayer.GetChildAt(0) != nativeContent)
				{
					currentPaddingLayer.RemoveAllViews();
					currentPaddingLayer.AddView(nativeContent);
				}
			}
			else
			{
				InsertInsetView(handler, scrollView, nativeContent);
			}
		}

		static void InsertInsetView(IScrollViewHandler handler, IScrollView scrollView, View nativeContent)
		{
			if (scrollView.PresentedContent == null || handler.MauiContext?.Context == null)
			{
				return;
			}

			var paddingShim = new ContentViewGroup(handler.MauiContext.Context)
			{
				CrossPlatformMeasure = IncludeScrollViewInsets(scrollView.CrossPlatformMeasure, scrollView),
				CrossPlatformArrange = scrollView.CrossPlatformArrange,
				Tag = InsetPanelTag
			};

			handler.PlatformView.RemoveAllViews();
			paddingShim.AddView(nativeContent);
			handler.PlatformView.SetContent(paddingShim);
		}

		static Func<double, double, Size> IncludeScrollViewInsets(Func<double, double, Size> internalMeasure, IScrollView scrollView)
		{
			return (widthConstraint, heightConstraint) =>
			{
				return InsetScrollView(widthConstraint, heightConstraint, internalMeasure, scrollView);
			};
		}

		static Size InsetScrollView(double widthConstraint, double heightConstraint, Func<double, double, Size> internalMeasure, IScrollView scrollView)
		{
			var padding = scrollView.Padding;

			if (scrollView.PresentedContent == null)
			{
				return new Size(padding.HorizontalThickness, padding.VerticalThickness);
			}

			// Exclude the padding while measuring the internal content ...
			var measurementWidth = widthConstraint - padding.HorizontalThickness;
			var measurementHeight = heightConstraint - padding.VerticalThickness;

			var result = internalMeasure.Invoke(measurementWidth, measurementHeight);

			// ... and add the padding back in to the final result
			var fullSize = new Size(result.Width + padding.HorizontalThickness, result.Height + padding.VerticalThickness);

			if (double.IsInfinity(widthConstraint))
			{
				widthConstraint = result.Width;
			}

			if (double.IsInfinity(heightConstraint))
			{
				heightConstraint = result.Height;
			}

			return fullSize.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), scrollView.PresentedContent);
		}
	}
}
