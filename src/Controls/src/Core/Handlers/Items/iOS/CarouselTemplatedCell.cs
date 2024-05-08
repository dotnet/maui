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

		//On CarouselView we want the cells to keep the binding context 
		//since the previous and next are generated on the fly we don't want to lose the binding context
		internal override bool ShouldUnbindCell => false;
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
			return new CGSize(_constraint.Width, _constraint.Height);
		}

		protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		{
			return (false, Size.Zero);
		}

		protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		{
			return false;
		}
	}
}
