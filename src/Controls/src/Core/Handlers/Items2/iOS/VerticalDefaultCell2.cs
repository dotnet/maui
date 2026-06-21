#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal sealed class VerticalDefaultCell2 : DefaultCell2
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.VerticalDefaultCell2");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public VerticalDefaultCell2(CGRect frame) : base(frame)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			Constraint = Label.WidthAnchor.ConstraintEqualTo(Frame.Width);
#pragma warning restore CS0618 // Type or member is obsolete
			Constraint.Priority = (float)UILayoutPriority.DefaultHigh;
			Constraint.Active = true;
		}

		// public override void ConstrainTo(CGSize constraint)
		// {
		// 	Constraint.Constant = constraint.Width;
		// }
		//
		// public override CGSize Measure()
		// {
		// 	return new CGSize(Constraint.Constant, Label.IntrinsicContentSize.Height);
		// }
	}
}