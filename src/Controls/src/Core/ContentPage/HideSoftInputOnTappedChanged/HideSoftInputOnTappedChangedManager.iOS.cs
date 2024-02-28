using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		internal IDisposable? SetupHideSoftInputOnTapped(UIView uiView)
		{
			if (!FeatureEnabled || uiView.Window is null)
			{
				return null;
			}

			var firstResponder = uiView.FindFirstResponder();

			if (firstResponder is not (UITextField or UITextView))
			{
				return null;
			}

			return ResignFirstResponderTouchGestureRecognizer
					.Update(firstResponder);
		}
	}
}