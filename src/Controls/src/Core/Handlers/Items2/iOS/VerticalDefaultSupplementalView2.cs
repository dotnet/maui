#nullable disable
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal sealed class VerticalDefaultSupplementalView2 : DefaultCell2
	{
		public static NSString ReuseId = new NSString("Microsoft.Maui.Controls.VerticalDefaultSupplementalView2");

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public VerticalDefaultSupplementalView2(CGRect frame) : base(frame)
		{
			Label.Font = UIFont.PreferredHeadline;

#pragma warning disable CS0618 // Type or member is obsolete
			Constraint = Label.WidthAnchor.ConstraintEqualTo(Frame.Width);
#pragma warning restore CS0618 // Type or member is obsolete
			Constraint.Priority = (float)UILayoutPriority.DefaultHigh;
			Constraint.Active = true;
		}
	}
}