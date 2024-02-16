using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

internal class MauiCollectionView : UICollectionView
{
	public MauiCollectionView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout)
	{
	}

	public override void ScrollRectToVisible(CGRect rect, bool animated)
	{
		if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
			base.ScrollRectToVisible(rect, animated);
	}
}