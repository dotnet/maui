#nullable disable
using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class DefaultCell2 : ItemsViewCell2
	{
		//public const string ReuseId = "Microsoft.Maui.Controls.DefaultCell2";

		public UILabel Label { get; }

		protected NSLayoutConstraint Constraint { get; set; }

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public DefaultCell2(CGRect frame) : base(frame)
		{
			Label = new UILabel(frame)
			{
				TextColor = Maui.Platform.ColorExtensions.LabelColor,
				Lines = 1,
				Font = UIFont.PreferredBody,
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			ContentView.BackgroundColor = UIColor.Clear;

			InitializeContentConstraints(Label);
		}

		// public override void ConstrainTo(nfloat constant)
		// {
		// 	Constraint.Constant = constant;
		// }
	}
}