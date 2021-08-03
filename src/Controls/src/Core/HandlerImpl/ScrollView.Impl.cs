using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView : IScrollView, IContentView
	{
		IView IContentView.Content => Content;

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

			if (Content is IView view)
			{
				_ = view.Measure(widthConstraint, heightConstraint);
			}

			return DesiredSize;
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			// We can't call base.ArrangeOverride here because ScrollView is based on Layout<T>, and that will call UpdateChildrenLayout.
			// Which we don't want; that's only for legacy layouts and causes all kinds of trouble if we have any padding defined.

			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);
			return Frame.Size;
		}
	}
}
