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
	internal static void GoToNextResponderOrResign(UIView view, bool isUnchangeableReturnKey = false)
	{
		if (!view.CheckIfEligible(isUnchangeableReturnKey))
		{
			view.ResignFirstResponder();
			return;
		}

		var superview = view.GetViewController<ContainerViewController>()?.View;
		if (superview is null)
		{
			view.ResignFirstResponder();
			return;
		}

		var nextField = view.FindNextView(superview, new Type[] { typeof(UITextView), typeof(UITextField) });
		view.ChangeFocusedView(nextField);
	}

	static bool CheckIfEligible(this UIView view, bool isUnchangeableReturnKey)
	{
		// have isUnchangeableReturnKey flag since EntryCells do not have a public property to change ReturnKeyType
		if (view is UITextField field && (field.ReturnKeyType == UIReturnKeyType.Next || isUnchangeableReturnKey))
			return true;
		else if (view is UITextView)
			return true;

		return false;
	}
}
