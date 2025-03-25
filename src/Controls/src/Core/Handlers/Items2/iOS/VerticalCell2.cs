#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal sealed class VerticalCell2 : WidthConstrainedTemplatedCell2
	{
		public new static NSString ReuseId = new NSString("Microsoft.Maui.Controls.VerticalCell2");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public VerticalCell2(CGRect frame) : base(frame)
		{
		}

		// public override CGSize Measure()
		// {
		// 	var measure = PlatformHandler.VirtualView.Measure(ConstrainedDimension, double.PositiveInfinity);
		//
		// 	return new CGSize(ConstrainedDimension, measure.Height);
		// }
		//
		// protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		// {
		// 	return attributes.Frame.Width == ConstrainedDimension;
		// }
	}
}