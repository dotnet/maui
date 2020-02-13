using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal sealed class VerticalDefaultSupplementalView : DefaultCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.VerticalDefaultSupplementalView");

		[Export("initWithFrame:")]
		[Internals.Preserve(Conditional = true)]
		public VerticalDefaultSupplementalView(CGRect frame) : base(frame)
		{
			Label.Font = UIFont.PreferredHeadline;

			Constraint = Label.WidthAnchor.ConstraintEqualTo(Frame.Width);
			Constraint.Active = true;
		}

		public override void ConstrainTo(CGSize constraint)
		{
			Constraint.Constant = constraint.Width;
		}

		public override CGSize Measure()
		{
			return new CGSize(Constraint.Constant, Label.IntrinsicContentSize.Height);
		}
	}
}