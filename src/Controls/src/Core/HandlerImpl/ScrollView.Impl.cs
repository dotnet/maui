using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class ScrollView : IScrollView
	{
		IView IScrollView.Content => Content;

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
			// We call OnSizeRequest so that the content gets measured appropriately
			// and then use the standard GetDesiredSize from the handler so the ScrollView's
			// backing control gets measured. 

			// TODO ezhart 2021-07-14 Verify that we've got the naming correct on this after we resolve the OnSizeRequest obsolete stuff
#pragma warning disable CS0618 // Type or member is obsolete
			var request = OnSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore CS0618 // Type or member is obsolete

			DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			var frame = this.ComputeFrame(bounds);

			// Force a native arrange call; otherwise, the native bookkeeping won't be done and things won't lay out correctly
			Handler?.NativeArrange(frame);

			Layout(frame);

			return Frame.Size;
		}
	}
}
