#nullable disable
using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public abstract class ItemsViewCell2 : UICollectionViewCell
	{
		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		protected ItemsViewCell2(CGRect frame) : base(frame)
		{
			ContentView.BackgroundColor = UIColor.Clear;

			var selectedBackgroundView = new UIView
			{
				BackgroundColor = Maui.Platform.ColorExtensions.Gray
			};

			SelectedBackgroundView = selectedBackgroundView;
		}

		protected void InitializeContentConstraints(UIView platformView)
		{
			SetupPlatformView(platformView, true);
		}

		private protected void SetupPlatformView(UIView platformView, bool autoLayout = false)
		{
			ContentView.TranslatesAutoresizingMaskIntoConstraints = false;

			ContentView.AddSubview(platformView);

			// We want the cell to be the same size as the ContentView
			ContentView.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
			ContentView.BottomAnchor.ConstraintEqualTo(BottomAnchor).Active = true;
			ContentView.LeadingAnchor.ConstraintEqualTo(LeadingAnchor).Active = true;
			ContentView.TrailingAnchor.ConstraintEqualTo(TrailingAnchor).Active = true;

			if (autoLayout)
			{
				// And we want the ContentView to be the same size as the root renderer for the Forms element
				platformView.TranslatesAutoresizingMaskIntoConstraints = false;
				ContentView.TopAnchor.ConstraintEqualTo(platformView.TopAnchor).Active = true;
				ContentView.BottomAnchor.ConstraintEqualTo(platformView.BottomAnchor).Active = true;
				ContentView.LeadingAnchor.ConstraintEqualTo(platformView.LeadingAnchor).Active = true;
				ContentView.TrailingAnchor.ConstraintEqualTo(platformView.TrailingAnchor).Active = true;
			}
		}

		// public abstract void ConstrainTo(nfloat constant);
		// public abstract void ConstrainTo(CGSize constraint);
		// public abstract CGSize Measure();
	}
}