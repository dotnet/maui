using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using static Microsoft.Maui.Layouts.LayoutManager;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView : IScrollView, IContentView
	{
		object IContentView.Content => Content;
		IView IContentView.PresentedContent => Content;

		double IScrollView.HorizontalOffset
		{
			get => ScrollX;
			set
			{
				if (ScrollX != value)
				{
					SetScrolledPosition(value, ScrollY);
				}
			}
		}

		double IScrollView.VerticalOffset
		{
			get => ScrollY;
			set
			{
				if (ScrollY != value)
				{
					SetScrolledPosition(ScrollX, value);
				}
			}
		}

		void IScrollView.RequestScrollTo(double horizontalOffset, double verticalOffset, bool instant)
		{
			var request = new ScrollToRequest(horizontalOffset, verticalOffset, instant);
			Handler?.Invoke(nameof(IScrollView.RequestScrollTo), request);
		}

		void IScrollView.ScrollFinished() => SendScrollFinished();

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if ((this as IContentView)?.PresentedContent is not IView content)
			{
				ContentSize = Size.Zero;
				return ContentSize;
			}

			switch (Orientation)
			{
				case ScrollOrientation.Horizontal:
					widthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Neither:
				case ScrollOrientation.Both:
					heightConstraint = double.PositiveInfinity;
					widthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Vertical:
				default:
					heightConstraint = double.PositiveInfinity;
					break;
			}

			content.Measure(widthConstraint, heightConstraint);
			return content.DesiredSize;
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);

			return Frame.Size;
		}

		Size IContentView.CrossPlatformArrange(Rect bounds)
		{
			if ((this as IContentView).PresentedContent is IView presentedContent)
			{
				var padding = Padding;

				// Normally we'd just want the content to be arranged within the ContentView's Frame,
				// but ScrollView content might be larger than the ScrollView itself (for obvious reasons)
				// So in each dimension, we assume the larger of the two values.
				bounds.Width = Math.Max(Frame.Width, presentedContent.DesiredSize.Width + padding.HorizontalThickness);
				bounds.Height = Math.Max(Frame.Height, presentedContent.DesiredSize.Height + padding.VerticalThickness);

				this.ArrangeContent(bounds);
			}

			return bounds.Size;
		}
	}
}
