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
			scrollView.FillViewport = true;

			return scrollView;
		}

		protected override void ConnectHandler(MauiScrollView platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ScrollChange += ScrollChange;
			platformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		protected override void DisconnectHandler(MauiScrollView platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.ScrollChange -= ScrollChange;
			platformView.CrossPlatformArrange = null;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var Context = MauiContext?.Context;
			var platformView = PlatformView;
			var virtualView = VirtualView;

			if (platformView == null || virtualView == null || Context == null)
			{
				return Size.Zero;
			}

			// Create a spec to handle the native measure
			var widthSpec = Context.CreateMeasureSpec(widthConstraint, virtualView.Width, virtualView.MaximumWidth);
			var heightSpec = Context.CreateMeasureSpec(heightConstraint, virtualView.Height, virtualView.MaximumHeight);

			if (platformView.FillViewport)
			{
				/*	With FillViewport active, the Android ScrollView will measure the content at least once; if it is 
					smaller than the ScrollView's viewport, it measure a second time at the size of the viewport
					so that the content can properly fill the whole viewport. But it will only do this if the measurespec
					is set to Exactly. So if we want our ScrollView to Fill the space in the scroll direction, we need to
					adjust the MeasureSpec accordingly. If the ScrollView is not set to Fill, we can just leave the spec
					alone and the ScrollView will size to its content as usual. */

				var orientation = virtualView.Orientation;

				if (orientation == ScrollOrientation.Both || orientation == ScrollOrientation.Vertical)
				{
					heightSpec = AdjustSpecForAlignment(heightSpec, virtualView.VerticalLayoutAlignment);
				}

				if (orientation == ScrollOrientation.Both || orientation == ScrollOrientation.Horizontal)
				{
					widthSpec = AdjustSpecForAlignment(widthSpec, virtualView.HorizontalLayoutAlignment);
				}
			}

			platformView.Measure(widthSpec, heightSpec);

			// Convert back to xplat sizes for the return value
			return Context.FromPixels(platformView.MeasuredWidth, platformView.MeasuredHeight);
		}

		static int AdjustSpecForAlignment(int measureSpec, Primitives.LayoutAlignment alignment)
		{
			if (alignment == Primitives.LayoutAlignment.Fill && measureSpec.GetMode() == MeasureSpecMode.AtMost)
			{
				return MeasureSpecMode.Exactly.MakeMeasureSpec(measureSpec.GetSize());
			}

			return measureSpec;
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

			UpdateInsetView(scrollView, handler);
		}

		public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetHorizontalScrollBarVisibility(scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView.SetVerticalScrollBarVisibility(scrollView.VerticalScrollBarVisibility);
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

			var horizontalOffsetDevice = (int)context.ToPixels(request.HorizontalOffset);
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

			To handle both issues, we insert a container ContentViewGroup which always lays out at the origin but provides 
			both the Padding and the Margin for the content. The extra layer also provides cross-platform measurement and layout.
			The extra layer uses the native ContentViewGroup control (the same one we already use as the backing for ContentView, Page, etc.). 

			The methods below exist to support inserting/updating the extra padding/margin layer.
		*/

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
				CrossPlatformMeasure = IncludeScrollViewInsets(scrollView.PresentedContent.Measure, scrollView),
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
