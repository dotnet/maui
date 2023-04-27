/*
 * This class is adapted from IQKeyboardManager which is an open-source
 * library implemented for iOS to handle Keyboard interactions with
 * UITextFields/UITextViews. Link to their MIT License can be found here:
 * https://github.com/hackiftekhar/IQKeyboardManager/blob/7399efb730eea084571b45a1a9b36a3a3c54c44f/LICENSE.md
 */

using System;
using UIKit;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManager
{
	internal static void GoToNextResponderOrResign(UIView view, UIView? customSuperView = null)
	{
		if (!view.CheckIfEligible())
		{
			view.ResignFirstResponder();
			return;
		}

		var superview = customSuperView ?? view.GetContainerView();
		if (superview is null)
		{
			view.ResignFirstResponder();
			return;
		}

		var nextField = view.FindNextView(superview, view =>
		{
			var isValidTextView = view is UITextView textView && textView.Editable;
			var isValidTextField = view is UITextField textField && textField.Enabled;

			return (isValidTextView || isValidTextField) && !view.Hidden && view.Alpha != 0f;
		});

		view.ChangeFocusedView(nextField);
	}

	static bool CheckIfEligible(this UIView view)
	{
		if (view is UITextField field && field.ReturnKeyType == UIReturnKeyType.Next)
			return true;
		else if (view is UITextView)
			return true;

		return false;
	}
}
