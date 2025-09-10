#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Core;
using static Microsoft.Maui.Layouts.LayoutExtensions;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScrollView, ScrollViewer>, ICrossPlatformLayout
	{
		const string ContentPanelTag = "MAUIScrollViewContentPanel";

		protected override ScrollViewer CreatePlatformView()
		{
			return new ScrollViewer();
		}

		internal static void MapInvalidateMeasure(IScrollViewHandler handler, IView view, object? args)
		{
			handler.PlatformView.InvalidateMeasure(view);

			if (handler.PlatformView.Content is FrameworkElement content)
			{
				content.InvalidateMeasure();
			}
		}

		protected override void ConnectHandler(ScrollViewer platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ViewChanged += ViewChanged;
		}

		protected override void DisconnectHandler(ScrollViewer platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.ViewChanged -= ViewChanged;
		}

		public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
		{
			if (handler.PlatformView == null || handler.MauiContext == null)
				return;

			if (handler is not ICrossPlatformLayout crossPlatformLayout)
			{
				return;
			}

			UpdateContentPanel(scrollView, handler, crossPlatformLayout);
		}

		public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView?.UpdateScrollBarVisibility(scrollView.Orientation, scrollView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
		{
			handler.PlatformView?.UpdateScrollBarVisibility(scrollView.Orientation, scrollView.VerticalScrollBarVisibility);
		}

		public static void MapOrientation(IScrollViewHandler handler, IScrollView scrollView)
		{
			var scrollBarVisibility = scrollView.Orientation == ScrollOrientation.Horizontal
					? scrollView.HorizontalScrollBarVisibility
					: scrollView.VerticalScrollBarVisibility;

			handler.PlatformView?.UpdateScrollBarVisibility(scrollView.Orientation, scrollBarVisibility);
		}

		public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
		{
			if (args is ScrollToRequest request)
			{
				if (handler.PlatformView.HorizontalOffset == request.HorizontalOffset &&
					handler.PlatformView.VerticalOffset == request.VerticalOffset)
				{
					handler.VirtualView.ScrollFinished();
				}
				else
				{
					handler.PlatformView.ChangeView(request.HorizontalOffset, request.VerticalOffset, null, request.Instant);
				}
			}
		}

		void ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
		{
			VirtualView.VerticalOffset = PlatformView.VerticalOffset;
			VirtualView.HorizontalOffset = PlatformView.HorizontalOffset;

			if (e.IsIntermediate == false)
			{
				VirtualView.ScrollFinished();
			}
		}

		/*
			Problem 1: Windows treats Padding differently than what we want for MAUI; Padding creates space
			_around_ the scrollable area, rather than padding the content inside of it. 

			Problem 2: The ScrollViewer will force any content to start at the origin (0,0), even if we ask 
			to arrange it at an offset. This defeats our content's Margin properties. 

			To handle this, we insert a container ContentPanel which always lays out at the origin but provides both 
			the Padding and the Margin for the content. The extra layer uses the native ContentPanel control we already 
			provide as the backing control for ContentView, Page, etc. 

			The extra layer also provides a place to call CrossPlatformArrange for the content, since we 
			can't subclass ScrollViewer.

			The methods below exist to support inserting/updating the padding/margin panel.
		 */

		static ContentPanel? GetContentPanel(ScrollViewer scrollViewer)
		{
			if (scrollViewer.Content is ContentPanel contentPanel)
			{
				if (contentPanel.Tag is string tag && tag == ContentPanelTag)
				{
					return contentPanel;
				}
			}

			return null;
		}

		static void UpdateContentPanel(IScrollView scrollView, IScrollViewHandler handler, ICrossPlatformLayout crossPlatformLayout)
		{
			if (scrollView.PresentedContent == null || handler.MauiContext == null)
			{
				return;
			}

			var scrollViewer = handler.PlatformView;
			var nativeContent = scrollView.PresentedContent.ToPlatform(handler.MauiContext);

			if (GetContentPanel(scrollViewer) is ContentPanel currentPaddingLayer)
			{
				if (currentPaddingLayer.CachedChildren.Count == 0 || currentPaddingLayer.CachedChildren[0] != nativeContent)
				{
					currentPaddingLayer.CachedChildren.Clear();
					currentPaddingLayer.CachedChildren.Add(nativeContent);

				}
			}
			else
			{
				InsertContentPanel(scrollViewer, scrollView, nativeContent, crossPlatformLayout);
			}
		}

		static void InsertContentPanel(ScrollViewer scrollViewer, IScrollView scrollView, FrameworkElement nativeContent, ICrossPlatformLayout crossPlatformLayout)
		{
			if (scrollView.PresentedContent == null)
			{
				return;
			}

			var paddingShim = new ContentPanel()
			{
				CrossPlatformLayout = crossPlatformLayout,
				Tag = ContentPanelTag
			};

			scrollViewer.Content = null;
			paddingShim.CachedChildren.Add(nativeContent);
			scrollViewer.Content = paddingShim;
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var scrollView = VirtualView;

			var padding = scrollView.Padding;
			var presentedContent = scrollView.PresentedContent;

			if (presentedContent == null)
			{
				return new Size(padding.HorizontalThickness, padding.VerticalThickness);
			}

			// Exclude the padding while measuring the internal content ...
			var measurementWidth = widthConstraint - padding.HorizontalThickness;
			var measurementHeight = heightConstraint - padding.VerticalThickness;

			var result = (scrollView as ICrossPlatformLayout).CrossPlatformMeasure(measurementWidth, measurementHeight);

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

			// If the presented content has LayoutAlignment Fill, we'll need to adjust the measurement to account for that
			return fullSize.AdjustForFill(new Rect(0, 0, widthConstraint, heightConstraint), presentedContent);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			return (VirtualView as ICrossPlatformLayout).CrossPlatformArrange(bounds);
		}
	}
}
