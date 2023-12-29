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
				return null;

			var firstResponder = uiView.FindFirstResponder(v => v is UITextField or UITextView);

			if (firstResponder is null)
				return null;

			return ResignFirstResponderTouchGestureRecognizer
					.Update(firstResponder);
		}
	}
}