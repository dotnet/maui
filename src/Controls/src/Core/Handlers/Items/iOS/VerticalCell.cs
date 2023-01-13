#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal sealed class VerticalCell : WidthConstrainedTemplatedCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.VerticalCell");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public VerticalCell(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = PlatformHandler.VirtualView.Measure(ConstrainedDimension, double.PositiveInfinity);

			return new CGSize(ConstrainedDimension, measure.Height);
		}

		protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		{
			return attributes.Frame.Width == ConstrainedDimension;
		}
	}
}