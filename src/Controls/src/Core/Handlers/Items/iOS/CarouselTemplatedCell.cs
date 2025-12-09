#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class CarouselTemplatedCell : TemplatedCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.CarouselTemplatedCell");
		CGSize _constraint;

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		protected CarouselTemplatedCell(CGRect frame) : base(frame)
		{
		}

		public override void ConstrainTo(nfloat constant)
		{
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ClearConstraints();

			_constraint = constraint;
		}

		public override CGSize Measure()
		{
			// Go through the measure pass even if the constraints are fixed
			// to ensure arrange pass has the appropriate desired size in place.
			PlatformHandler.VirtualView.Measure(_constraint.Width, _constraint.Height);
			return _constraint;
		}

		protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		{
			return (false, Size.Zero);
		}

		protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		{
			return _constraint.IsCloseTo(attributes.Frame.Size);
		}
	}
}
