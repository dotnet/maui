using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class ScrollViewStub : StubBase, IScrollView
	{
		public object Content { get; set; }
		public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }
		public ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
		public ScrollOrientation Orientation { get; set; }
		public Size ContentSize { get; set; }
		public double HorizontalOffset { get; set; }
		public double VerticalOffset { get; set; }
		public Thickness Padding { get; set; }
		public IView PresentedContent => Content as IView;

		public void RequestScrollTo(double horizontalOffset, double verticalOffset, bool instant)
		{
			throw new System.NotImplementedException();
		}

		public void ScrollFinished()
		{
			throw new System.NotImplementedException();
		}

		public Size CrossPlatformArrange(Rect bounds)
		{
			return bounds.Size;
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}
	}
}