#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using System;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal sealed class HorizontalCell : HeightConstrainedTemplatedCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.HorizontalCell");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HorizontalCell(CGRect frame) : base(frame)
		{
		}

		public override CGSize Measure()
		{
			var measure = PlatformHandler.VirtualView.Measure(double.PositiveInfinity, ConstrainedDimension);

			return new CGSize(measure.Width, Math.Min(measure.Height, ConstrainedDimension));
		}

		protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		{
			return attributes.Frame.Width == ConstrainedDimension;
		}
	}
}