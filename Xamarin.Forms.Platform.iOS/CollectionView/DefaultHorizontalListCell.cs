using CoreGraphics;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	// TODO hartez 2018/09/12 21:43:54 The "List" part of all these cell names needs to go	
	internal sealed class DefaultHorizontalListCell : DefaultCell
	{
		public static NSString ReuseId = new NSString("Xamarin.Forms.Platform.iOS.DefaultHorizontalListCell");

		[Export("initWithFrame:")]
		public DefaultHorizontalListCell(CGRect frame) : base(frame)
		{
			Constraint = Label.HeightAnchor.ConstraintEqualTo(Frame.Height);
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