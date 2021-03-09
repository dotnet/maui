using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static class ScrollToPositionExtensions
	{
		public static UICollectionViewScrollPosition ToCollectionViewScrollPosition(this ScrollToPosition scrollToPosition, 
			UICollectionViewScrollDirection scrollDirection = UICollectionViewScrollDirection.Vertical, bool isLtr = false)
		{
			if (scrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				return scrollToPosition.ToHorizontalCollectionViewScrollPosition(isLtr);
			}

			return scrollToPosition.ToVerticalCollectionViewScrollPosition();
		}

		public static UICollectionViewScrollPosition ToHorizontalCollectionViewScrollPosition(this ScrollToPosition scrollToPosition, bool isLtr)
		{
			switch (scrollToPosition)
			{
				case ScrollToPosition.MakeVisible:
					return UICollectionViewScrollPosition.None;
				case ScrollToPosition.Start:
					return isLtr ? UICollectionViewScrollPosition.Right : UICollectionViewScrollPosition.Left;
				case ScrollToPosition.End:
					return isLtr ? UICollectionViewScrollPosition.Left : UICollectionViewScrollPosition.Right;
				case ScrollToPosition.Center:
				default:
					return UICollectionViewScrollPosition.CenteredHorizontally;
			}
		}

		public static UICollectionViewScrollPosition ToVerticalCollectionViewScrollPosition(this ScrollToPosition scrollToPosition)
		{
			switch (scrollToPosition)
			{
				case ScrollToPosition.MakeVisible:
					return UICollectionViewScrollPosition.None;
				case ScrollToPosition.Start:
					return UICollectionViewScrollPosition.Top;
				case ScrollToPosition.End:
					return UICollectionViewScrollPosition.Bottom;
				case ScrollToPosition.Center:
				default:
					return UICollectionViewScrollPosition.CenteredVertically;
			}
		}
	}
}