namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class SwipeViewStub : StubBase, ISwipeView
	{
		public double Threshold { get; set; }

		public ISwipeItems LeftItems { get; set; }

		public ISwipeItems RightItems { get; set; }

		public ISwipeItems TopItems { get; set; }

		public ISwipeItems BottomItems { get; set; }

		public bool IsOpen { get; set; }

		public SwipeTransitionMode SwipeTransitionMode { get; set; }

		public object Content { get; set; }

		public IView PresentedContent { get; set; }

		public Thickness Padding { get; set; }

		public Size CrossPlatformArrange(Rect bounds)
		{
			return PresentedContent?.Arrange(bounds) ?? Size.Zero;
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return PresentedContent?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		public void RequestClose(SwipeViewCloseRequest swipeCloseRequest)
		{
			IsOpen = false;
		}

		public void RequestOpen(SwipeViewOpenRequest swipeOpenRequest)
		{
			IsOpen = true;
		}

		public void SwipeChanging(SwipeViewSwipeChanging swipeChanging)
		{

		}

		public void SwipeEnded(SwipeViewSwipeEnded swipeEnded)
		{

		}

		public void SwipeStarted(SwipeViewSwipeStarted swipeStarted)
		{

		}
	}
}