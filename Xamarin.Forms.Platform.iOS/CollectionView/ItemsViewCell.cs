using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public abstract class ItemsViewCell : UICollectionViewCell
	{
		[Export("initWithFrame:")]
		protected ItemsViewCell(CGRect frame) : base(frame)
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemsViewCell));
			ContentView.BackgroundColor = UIColor.Clear;
		}

		protected void InitializeContentConstraints(UIView nativeView)
		{
			ContentView.AddSubview(nativeView);
			ContentView.TranslatesAutoresizingMaskIntoConstraints = false;
			ContentView.TopAnchor.ConstraintEqualTo(nativeView.TopAnchor).Active = true;
			ContentView.BottomAnchor.ConstraintEqualTo(nativeView.BottomAnchor).Active = true;
			ContentView.LeadingAnchor.ConstraintEqualTo(nativeView.LeadingAnchor).Active = true;
			ContentView.TrailingAnchor.ConstraintEqualTo(nativeView.TrailingAnchor).Active = true;
		}

		public abstract void ConstrainTo(nfloat constant);
		public abstract void ConstrainTo(CGSize constraint);
		public abstract CGSize Measure();
	}
}