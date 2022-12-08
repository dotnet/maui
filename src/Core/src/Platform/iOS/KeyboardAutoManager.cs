/*
 * This class is adapted from IQKeyboardManager which is an open-source
 * library implemented for iOS to handle Keyboard interactions with
 * UITextFields/UITextViews. Link to their license can be found here:
 * https://github.com/hackiftekhar/IQKeyboardManager/tree/09d22c087732b02d4fb594c7bb61502bf7bb2378#license
 */

using System.Collections.Generic;
using UIKit;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManager
{
	// you can provide a topView argument or it will try to find the ContainerViewController
	internal static void GoToNextResponderOrResign(UIView view, UIView? topView = null)
	{
		if (!view.CheckIfEligible())
		{
			view.ResignFirstResponder();
			return;
		}

		var targetSuperview = topView ?? view.GetViewController()?.GetContainerViewController();
		if (targetSuperview is null)
		{
			view.ResignFirstResponder();
			return;
		}

		var textFields = GetDeepResponderViews(targetSuperview);

		view.MoveToNextField(textFields);
	}

	static bool CheckIfEligible(this UIView view)
	{
		if (view is UITextField field && field.ReturnKeyType == UIReturnKeyType.Next)
			return true;
		else if (view is UITextView)
			return true;

		return false;
	}

	static UIView? GetContainerViewController(this UIViewController controller)
	{
		var curView = controller.View;

		while (curView?.Superview is not null)
		{
			var curController = curView.GetViewController();
			if (curController is ContainerViewController container)
				return container.View;

			curView = curView.Superview;
		}

		return null;
	}

	static void MoveToNextField (this UIView view, List<UIView> textFields)
	{
		// get the index of the current textField and go to the next one
		var currentIndex = textFields.FindIndex(v => v == view);
		var nextIndex = currentIndex < textFields.Count - 1 ? currentIndex + 1 : -1;

		if (nextIndex != -1)
			textFields[nextIndex].BecomeFirstResponder();
		else
			view.ResignFirstResponder();
	}

	// Find all of the eligible UITextFields and UITextViews inside this view
	internal static List<UIView> GetDeepResponderViews(UIView view)
	{
		var textItems = view.GetResponderViews();

		textItems.Sort(new ResponderSorter());

		return textItems;
	}

	static List<UIView> GetResponderViews(this UIView view)
	{
		var textItems = new List<UIView>();

		foreach (var child in view.Subviews)
		{
			if (child is UITextField textField && child.CanBecomeFirstResponder())
				textItems.Add(textField);

			else if (child is UITextView textView && child.CanBecomeFirstResponder())
				textItems.Add(textView);

			else if (child.Subviews.Length > 0 && !child.Hidden && child.Alpha > 0f)
				textItems.AddRange(child.GetResponderViews());
		}

		return textItems;
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
			&& view.Alpha != 0f
			&& !view.IsAlertViewTextField();
	}

	static bool IsAlertViewTextField(this UIView view)
	{
		var alertViewController = view.GetViewController();

		while (alertViewController is not null)
		{
			if (alertViewController is UIAlertController)
				return true;

			alertViewController = alertViewController.NextResponder as UIViewController;
		}

		return false;
	}

	static UIViewController? GetViewController(this UIView view)
	{
		var nextResponder = view as UIResponder;
		while (nextResponder is not null)
		{
			nextResponder = nextResponder.NextResponder;

			if (nextResponder is UIViewController viewController)
				return viewController;
		}
		return null;
	}

	class ResponderSorter : Comparer<UIView>
	{
		public override int Compare(UIView? view1, UIView? view2)
		{
			if (view1 is null || view2 is null)
				return 1;

			var bound1 = view1.ConvertRectToView(view1.Bounds, null);
			var bound2 = view2.ConvertRectToView(view2.Bounds, null);

			if (bound1.Top != bound2.Top)
				return bound1.Top < bound2.Top ? -1 : 1;
			else
				return bound1.Left < bound2.Left ? -1 : 1;
		}
	}
}
