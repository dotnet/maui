#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal sealed class VerticalDefaultSupplementalView : DefaultCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.VerticalDefaultSupplementalView");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public VerticalDefaultSupplementalView(CGRect frame) : base(frame)
		{
			Label.Font = UIFont.PreferredHeadline;

#pragma warning disable CS0618 // Type or member is obsolete
			Constraint = Label.WidthAnchor.ConstraintEqualTo(Frame.Width);
#pragma warning restore CS0618 // Type or member is obsolete
			Constraint.Priority = (float)UILayoutPriority.DefaultHigh;
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