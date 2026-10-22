using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class LayoutView : MauiView
	{
		bool _userInteractionEnabled;

		public override void SubviewAdded(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.SubviewAdded(uiview);
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.WillRemoveSubview(uiview);
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent)
		{
			var result = base.HitTest(point, uievent);

			if (result is null)
			{
				Issue17389CoreTrace.ObserveHit(this, point, uievent, result, Issue17389CoreTrace.HitReturn.BaseNull);
				return null;
			}

			if (!_userInteractionEnabled && Equals(result))
			{
				// If user interaction is disabled (IOW, if the corresponding Layout is InputTransparent),
				// then we exclude the LayoutView itself from hit testing. But it's children are valid
				// hit testing targets.

				Issue17389CoreTrace.ObserveHit(this, point, uievent, result, Issue17389CoreTrace.HitReturn.ExcludedSelf);
				return null;
			}

			if (result is LayoutView layoutView && !layoutView.UserInteractionEnabledOverride)
			{
				// If the child is a layout then we need to check the UserInteractionEnabledOverride
				// since layouts always have user interaction enabled.

				Issue17389CoreTrace.ObserveHit(this, point, uievent, result, Issue17389CoreTrace.HitReturn.ExcludedLayout);
				return null;
			}

			Issue17389CoreTrace.ObserveHit(this, point, uievent, result, Issue17389CoreTrace.HitReturn.Returned);
			return result;
		}

		internal bool UserInteractionEnabledOverride => _userInteractionEnabled;

		public override bool UserInteractionEnabled
		{
			get => base.UserInteractionEnabled;
			set
			{
				// We leave the base UIE value true no matter what, so that hit testing will find children
				// of the LayoutView. But we track the intended value so we can use it during hit testing
				// to ignore the LayoutView itself, if necessary.

				base.UserInteractionEnabled = true;
				_userInteractionEnabled = value;
			}
		}
	}
}