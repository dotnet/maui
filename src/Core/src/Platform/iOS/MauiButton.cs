using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class MauiButton : UIButton
	{
		public event EventHandler? FocusChanged;

		public override void DidUpdateFocus(UIFocusUpdateContext context, UIFocusAnimationCoordinator coordinator)
		{
			FocusChanged?.Invoke(this, new EventArgs());
			base.DidUpdateFocus(context, coordinator);
		}
	}
}

