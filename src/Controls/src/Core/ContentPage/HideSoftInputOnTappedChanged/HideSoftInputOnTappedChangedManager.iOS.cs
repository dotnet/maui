using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		internal IDisposable? SetupHideSoftInputOnTapped(UIView uIView)
		{
			if (!FeatureEnabled || uIView.Window is null)
				return null;

			if (uIView is UISearchBar searchBar &&
				searchBar.GetSearchTextField() is UIView textField)
			{
				uIView = textField;
			}

			if (uIView is null)
				return null;

			return ResignFirstResponderTouchGestureRecognizer
					.Update(uIView);
		}
	}
}