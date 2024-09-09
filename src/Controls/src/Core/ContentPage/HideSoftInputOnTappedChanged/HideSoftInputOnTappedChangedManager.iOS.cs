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

			UIView? uiText = uiView;

			if (uiText is not (UITextField or UITextView))
			{
				uiText = uiView.FindDescendantView<UIView>((view) => view is (UITextField or UITextView));
			}

			if (uiText is null)
			{
				return null;
			}

			return ResignFirstResponderTouchGestureRecognizer
					.Update(uiText);
		}
	}
}