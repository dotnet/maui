/*
 * This class is adapted from IQKeyboardManager which is an open-source
 * library implemented for iOS to handle Keyboard interactions with
 * UITextFields/UITextViews. Link to their MIT License can be found here:
 * https://github.com/hackiftekhar/IQKeyboardManager/blob/7399efb730eea084571b45a1a9b36a3a3c54c44f/LICENSE.md
 */

using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManager
{
	// you can provide a topView argument or it will try to find the ContainerViewController
	internal static void GoToNextResponderOrResign(UIView view, UIView? topView = null, bool isEligibleOverride = false)
	{
		if (!view.CheckIfEligible(isEligibleOverride))
		{
			view.ResignFirstResponder();
			return;
		}

		var targetSuperview = topView ?? view.GetViewController<ContainerViewController>()?.View;
		if (targetSuperview is null)
		{
			view.ResignFirstResponder();
			return;
		}

		var nextField = view.FindNextField(targetSuperview);
		view.MoveToNextField(nextField);
	}

	static bool CheckIfEligible(this UIView view, bool isEligibleOverride)
	{
		// have is isEligibleOverride flag since EntryCells have a default ReturnKeyType by default
		if (view is UITextField field && (field.ReturnKeyType == UIReturnKeyType.Next || isEligibleOverride))
			return true;
		else if (view is UITextView)
			return true;

		return false;
	}

	internal static UIView? FindNextField(this UIView view, UIView superView)
	{
		var originalRect = view.ConvertRectToView(view.Bounds, null);
		var nextField = superView.SearchBestField(originalRect, null);

		return nextField;
	}

	static UIView? SearchBestField(this UIView view, CGRect originalRect, UIView? currentBest)
	{
		foreach (var child in view.Subviews)
		{
			if ((child is UITextField || child is UITextView) && child.CanBecomeFirstResponder())
			{
				if (TryFindNewBestField(originalRect, currentBest, child, out var newBest))
					currentBest = newBest;
			}

			else if (child.Subviews.Length > 0 && !child.Hidden && child.Alpha > 0f)
			{
				var newBestChild = child.SearchBestField(originalRect, currentBest);
				if (newBestChild is not null && TryFindNewBestField(originalRect, currentBest, newBestChild, out var newBest))
					currentBest = newBest;
			}
		}

		return currentBest;
	}

	static bool TryFindNewBestField(CGRect originalRect, UIView? currentBest, UIView newView, out UIView newBest)
	{
		var currentBestRect = currentBest?.ConvertRectToView(currentBest.Bounds, null);
		var newViewRect = newView.ConvertRectToView(newView.Bounds, null);

		var cbrValue = currentBestRect.GetValueOrDefault();
		newBest = newView;

		if (originalRect.Top < newViewRect.Top &&
			(currentBestRect is null || newViewRect.Top < cbrValue.Top))
		{
			return true;
		}

		else if (originalRect.Top == newViewRect.Top &&
				 originalRect.Left < newViewRect.Left &&
				 (currentBestRect is null || newViewRect.Left < cbrValue.Left))
		{
			return true;
		}

		return false;
	}

	static void MoveToNextField(this UIView view, UIView? newView)
	{
		if (newView is null)
			view.ResignFirstResponder();

		else
			newView.BecomeFirstResponder();
	}

	static bool CanBecomeFirstResponder(this UIView view)
	{
		var isFirstResponder = false;

		if (view is UITextView tview)
			isFirstResponder = tview.Editable;
		else if (view is UITextField field)
			isFirstResponder = field.Enabled;

		return !isFirstResponder ? false :
			!view.Hidden
			&& view.Alpha != 0f;
			// && !view.IsAlertViewTextField();
			// the above is in the original code but is not useful here
	}
}
