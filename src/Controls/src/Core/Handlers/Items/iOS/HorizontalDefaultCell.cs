#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal sealed class HorizontalDefaultCell : DefaultCell
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.HorizontalDefaultCell");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public HorizontalDefaultCell(CGRect frame) : base(frame)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			Constraint = Label.HeightAnchor.ConstraintEqualTo(Frame.Height);
#pragma warning restore CS0618 // Type or member is obsolete
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