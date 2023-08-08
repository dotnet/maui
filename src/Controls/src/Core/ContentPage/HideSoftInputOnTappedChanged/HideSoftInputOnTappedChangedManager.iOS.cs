using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		internal IDisposable? SetupHideSoftInputOnTapped(UIView? uIView)
		{
			if (FeatureEnabled || uIView?.Window is null)
				return null;

			if (uIView is UISearchBar searchBar)
				uIView = searchBar.GetSearchTextField();

			IDisposable? watchingForTaps = null;

			if (uIView?.Window is UIWindow window)
			{
				if (uIView is UITextView textView)
				{
					ResignFirstResponderTouchGestureRecognizer.Update(
						textView,
						window,
						out watchingForTaps);
				}
				else if (uIView is UIControl uiControl)
				{
					ResignFirstResponderTouchGestureRecognizer.Update(
						uiControl,
						window,
						out watchingForTaps);
				}
			}

			if (watchingForTaps is null)
				return null;

			return watchingForTaps;
		}
	}
}