#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal sealed class VerticalSupplementaryView2 : WidthConstrainedTemplatedCell2
	{
		public new static NSString ReuseId = new NSString("Microsoft.Maui.Controls.VerticalSupplementaryView2");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public VerticalSupplementaryView2(CGRect frame) : base(frame)
		{
		}

		// public override CGSize Measure()
		// {
		// 	if (PlatformHandler?.VirtualView == null)
		// 	{
		// 		return CGSize.Empty;
		// 	}
		//
		// 	var measure = PlatformHandler.VirtualView.Measure(ConstrainedDimension, double.PositiveInfinity);
		//
		// 	var height = PlatformHandler.VirtualView.Height > 0
		// 		? PlatformHandler.VirtualView.Height : measure.Height;
		//
		// 	return new CGSize(ConstrainedDimension, height);
		// }
		//
		// protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		// {
		// 	return attributes.Frame.Width == ConstrainedDimension;
		// }
	}
}