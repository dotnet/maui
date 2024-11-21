#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal sealed class HorizontalCell2 : HeightConstrainedTemplatedCell2
	{
		public new static NSString ReuseId = new NSString("Microsoft.Maui.Controls.HorizontalCell2");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HorizontalCell2(CGRect frame) : base(frame)
		{
		}

		// public override CGSize Measure()
		// {
		// 	var measure = PlatformHandler.VirtualView.Measure(double.PositiveInfinity, ConstrainedDimension);
		//
		// 	return new CGSize(measure.Width, ConstrainedDimension);
		// }
		//
		// protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		// {
		// 	return attributes.Frame.Width == ConstrainedDimension;
		// }
	}
}