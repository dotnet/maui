using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal sealed class HorizontalDefaultSupplementalView : DefaultCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.Compatibility.Platform.iOS.HorizontalDefaultSupplementalView");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HorizontalDefaultSupplementalView(CGRect frame) : base(frame)
		{
			Label.Font = UIFont.PreferredHeadline;

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