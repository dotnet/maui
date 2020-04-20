using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public abstract class DefaultCell : ItemsViewCell
	{
		public UILabel Label { get; }

		protected NSLayoutConstraint Constraint { get; set; }

		[Export("initWithFrame:")]
		[Internals.Preserve(Conditional = true)]
		protected DefaultCell(CGRect frame) : base(frame)
		{
			Label = new UILabel(frame)
			{
				TextColor = ColorExtensions.LabelColor,
				Lines = 1,
				Font = UIFont.PreferredBody,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			ContentView.BackgroundColor = UIColor.Clear;

			InitializeContentConstraints(Label);
		}

		public override void ConstrainTo(nfloat constant)
		{
			Constraint.Constant = constant;
		}
	}
}