#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal sealed class HorizontalDefaultCell2 : DefaultCell2
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.HorizontalDefaultCell2");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HorizontalDefaultCell2(CGRect frame) : base(frame)
		{
			Constraint = Label.HeightAnchor.ConstraintEqualTo(Frame.Height);
			Constraint.Priority = (float)UILayoutPriority.DefaultHigh;
			Constraint.Active = true;
		}

		// public override void ConstrainTo(CGSize constraint)
		// {
		// 	Constraint.Constant = constraint.Height;
		// }
		//
		// public override CGSize Measure()
		// {
		// 	return new CGSize(Label.IntrinsicContentSize.Width, Constraint.Constant);
		// }
	}
}