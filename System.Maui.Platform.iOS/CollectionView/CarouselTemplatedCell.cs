using System;
using CoreGraphics;
using Foundation;

namespace System.Maui.Platform.iOS
{
	public class CarouselTemplatedCell : TemplatedCell
	{
		public static NSString ReuseId = new NSString("System.Maui.Platform.iOS.CarouselTemplatedCell");

		[Export("initWithFrame:")]
		[Internals.Preserve(Conditional = true)]
		protected CarouselTemplatedCell(CGRect frame) : base(frame)
		{ }

		public override void ConstrainTo(nfloat constant)
		{
			
		}
		CGSize _constrain;
		public override void ConstrainTo(CGSize constraint)
		{
			_constrain = constraint;
			Layout(constraint);
		}

		public override CGSize Measure()
		{
			return new CGSize(_constrain.Width,_constrain.Height);
		}

		protected override (bool, Size) NeedsContentSizeUpdate(Size currentSize)
		{
			return (false, Size.Zero);
		}
	}
}
