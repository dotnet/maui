#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class CarouselTemplatedCell2 : TemplatedCell2
	{
		public new static NSString ReuseId = new NSString("Microsoft.Maui.Controls.CarouselTemplatedCell2");
		//CGSize _constraint;

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		protected CarouselTemplatedCell2(CGRect frame) : base(frame)
		{
		}

		// public override void ConstrainTo(nfloat constant)
		// {
		// }
		//
		// public override void ConstrainTo(CGSize constraint)
		// {
		// 	ClearConstraints();
		//
		// 	_constraint = constraint;
		// }

		// public override CGSize Measure()
		// {
		// 	return new CGSize(_constraint.Width, _constraint.Height);
		// }
		//
		// protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		// {
		// 	return (false, Size.Zero);
		// }
		//
		// protected override bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes)
		// {
		// 	return false;
		// }
	}
}
