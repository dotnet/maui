using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal sealed class HorizontalDefaultCell : DefaultCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.HorizontalDefaultCell");

		[Export("initWithFrame:")]
		[Internals.Preserve(Conditional = true)]
		public HorizontalDefaultCell(CGRect frame) : base(frame)
		{
			Constraint = Label.HeightAnchor.ConstraintEqualTo(Frame.Height);
			Constraint.Priority = (float)UILayoutPriority.DefaultHigh;
			Constraint.Active = true;
		}

		public override void ConstrainTo(CGSize constraint)
		{
			Constraint.Constant = constraint.Height;
		}

		public override CGSize Measure()
		{
			return new CGSize(Label.IntrinsicContentSize.Width, Constraint.Constant);
		}
	}
}