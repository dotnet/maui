/*
 * This class is adapted from IQKeyboardManager which is an open-source
 * library implemented for iOS to handle Keyboard interactions with
 * UITextFields/UITextViews. Link to their MIT License can be found here:
 * https://github.com/hackiftekhar/IQKeyboardManager/blob/7399efb730eea084571b45a1a9b36a3a3c54c44f/LICENSE.md
 */

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform;

public static class KeyboardAutoManagerScroll
{
	internal static bool IsKeyboardAutoScrollHandling;
	static UIScrollView? LastScrollView;
	static UIScrollView? ScrolledView;
	static CGPoint StartingContentOffset;
	static UIEdgeInsets StartingScrollIndicatorInsets;
	static UIEdgeInsets StartingContentInsets;
	static CGRect KeyboardFrame = CGRect.Empty;
	static CGPoint TopViewBeginOrigin = new(nfloat.MaxValue, nfloat.MaxValue);
	static readonly CGPoint InvalidPoint = new(nfloat.MaxValue, nfloat.MaxValue);
	static double AnimationDuration = 0.25;
	static UIView? View;
	static UIView? ContainerView;
	static CGRect? CursorRect;
	internal static bool IsKeyboardShowing;
	static int TextViewTopDistance = 20;
	static int DebounceCount;
	static NSObject? WillShowToken;
	static NSObject? WillHideToken;
	static NSObject? DidHideToken;
	static NSObject? TextFieldToken;
	static NSObject? TextViewToken;

	public static void Connect()
	{
		if (TextFieldToken is not null)
			return;

		TextFieldToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextFieldTextDidBeginEditingNotification"), DidUITextBeginEditing);

		TextViewToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextViewTextDidBeginEditingNotification"), DidUITextBeginEditing);

		WillShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), WillKeyboardShow);

		WillHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillHideNotification"), WillHideKeyboard);

		DidHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardDidHideNotification"), DidHideKeyboard);
	}

	public static void Disconnect()
	{
		if (WillShowToken is not null)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(WillShowToken);
			WillShowToken = null;
		}
		if (WillHideToken is not null)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(WillHideToken);
			WillHideToken = null;
		}
		if (DidHideToken is not null)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(DidHideToken);
			DidHideToken = null;
		}
		if (TextFieldToken is not null)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(TextFieldToken);
			TextFieldToken = null;
		}
		if (TextViewToken is not null)
		{
			NSNotificationCenter.DefaultCenter.RemoveObserver(TextViewToken);
			TextViewToken = null;
		}

		IsKeyboardAutoScrollHandling = false;
	}

	static async void DidUITextBeginEditing(NSNotification notification)
	{
		IsKeyboardAutoScrollHandling = true;

		if (notification.Object is not null)
		{
			View = notification.Object as UIView;

			if (View is null || View.FindResponder<UIAlertController>() is not null)
			{
				IsKeyboardAutoScrollHandling = false;
				return;
			}

			CursorRect = null;

			ContainerView = View.GetContainerView();

			// the cursor needs a small amount of time to update the position
			await Task.Delay(5);

			var localCursor = FindLocalCursorPosition();
			if (localCursor is CGRect local)
				CursorRect = View.ConvertRectToView(local, null);

			TextViewTopDistance = ((int?)localCursor?.Height ?? 0) + 20;

			await AdjustPositionDebounce();
		}
	}

	static CGRect? FindLocalCursorPosition()
	{
		var textInput = View as IUITextInput;
		var selectedTextRange = textInput?.SelectedTextRange;
		return selectedTextRange is not null ? textInput?.GetCaretRectForPosition(selectedTextRange.Start) : null;
	}

	internal static CGRect? FindCursorPosition()
	{
		var localCursor = FindLocalCursorPosition();
		if (localCursor is CGRect local)
			return View?.ConvertRectToView(local, null);

		return null;
	}

	static async void WillKeyboardShow(NSNotification notification)
	{
		var userInfo = notification.UserInfo;

		if (userInfo is not null)
		{
			var frameSize = userInfo.FindValue("UIKeyboardFrameEndUserInfoKey");
			var frameSizeRect = DescriptionToCGRect(frameSize?.Description);
			if (frameSizeRect is not null)
				KeyboardFrame = (CGRect)frameSizeRect;

			userInfo.SetAnimationDuration();
		}

		if (!IsKeyboardShowing)
		{
			await AdjustPositionDebounce();
			IsKeyboardShowing = true;
		}
	}

	static void WillHideKeyboard(NSNotification notification)
	{
		notification.UserInfo?.SetAnimationDuration();

		if (LastScrollView is not null)
			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, AnimateHidingKeyboard, () => { });

		if (IsKeyboardShowing)
			RestorePosition();

		IsKeyboardShowing = false;
		View = null;
		LastScrollView = null;
		KeyboardFrame = CGRect.Empty;
		StartingContentInsets = new UIEdgeInsets();
		StartingScrollIndicatorInsets = new UIEdgeInsets();
		StartingContentInsets = new UIEdgeInsets();
	}

	static void DidHideKeyboard(NSNotification notification)
	{
		IsKeyboardAutoScrollHandling = false;
	}

	static NSObject? FindValue(this NSDictionary dict, string key)
	{
		using var keyName = new NSString(key);
		var isFound = dict.TryGetValue(keyName, out var obj);
		return obj;
	}

	static void SetAnimationDuration(this NSDictionary dict)
	{
		var durationObj = dict.FindValue("UIKeyboardAnimationDurationUserInfoKey");
		var durationNum = (NSNumber)NSObject.FromObject(durationObj);
		var num = (double)durationNum;
		if (num != 0)
			AnimationDuration = num;
	}

	static void AnimateHidingKeyboard()
	{
		if (LastScrollView is not null && LastScrollView.ContentInset != StartingContentInsets)
		{
			LastScrollView.ContentInset = StartingContentInsets;
			LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
		}

		var superScrollView = LastScrollView;
		while (superScrollView is not null)
		{
			var contentSize = new CGSize(Math.Max(superScrollView.ContentSize.Width, superScrollView.Frame.Width),
				Math.Max(superScrollView.ContentSize.Height, superScrollView.Frame.Height));

			var minY = contentSize.Height - superScrollView.Frame.Height;
			if (minY < superScrollView.ContentOffset.Y)
			{
				var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, minY);
				if (!superScrollView.ContentOffset.Equals(newContentOffset))
				{
					if (View?.Superview is UIStackView)
						superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
					else
						superScrollView.ContentOffset = newContentOffset;
				}
			}
			superScrollView = superScrollView.FindResponder<UIScrollView>();
		}
	}

	// Used to get the numeric values from the UserInfo dictionary's NSObject value to CGRect.
	// Doing manually since CGRectFromString is not yet bound
	static CGRect? DescriptionToCGRect(string? description)
	{
		// example of passed in description: "NSRect: {{0, 586}, {430, 346}}"

		if (description is null)
			return null;

		// remove everything except for numbers and commas
		var temp = Regex.Replace(description, @"[^0-9,]", "");
		var dimensions = temp.Split(',');

		if (dimensions.Length == 4
			&& nfloat.TryParse(dimensions[0], out var x)
			&& nfloat.TryParse(dimensions[1], out var y)
			&& nfloat.TryParse(dimensions[2], out var width)
			&& nfloat.TryParse(dimensions[3], out var height))
		{
			return new CGRect(x, y, width, height);
		}

		return null;
	}

	// Used to debounce calls from different oberservers so we can be sure
	// all the fields are updated before calling AdjustPostition()
	internal static async Task AdjustPositionDebounce()
	{
		Interlocked.Increment(ref DebounceCount);

		var entranceCount = DebounceCount;

		await Task.Delay(10);

		if (entranceCount == DebounceCount)
			AdjustPosition();
	}

	// main method to calculate and animate the scrolling
	internal static void AdjustPosition()
	{
		if (ContainerView is null
			|| CursorRect is null
			|| (View is not UITextField && View is not UITextView))
		{
			IsKeyboardAutoScrollHandling = false;
			return;
		}

		if (TopViewBeginOrigin == InvalidPoint)
			TopViewBeginOrigin = new CGPoint(ContainerView.Frame.X, ContainerView.Frame.Y);

		var rootViewOrigin = new CGPoint(ContainerView.Frame.GetMinX(), ContainerView.Frame.GetMinY());
		var window = ContainerView.Window;

		var intersectRect = CGRect.Intersect(KeyboardFrame, window.Frame);
		var kbSize = intersectRect == CGRect.Empty ? new CGSize(KeyboardFrame.Width, 0) : intersectRect.Size;

		nfloat statusBarHeight;
		nfloat navigationBarAreaHeight;

		if (ContainerView.GetNavigationController() is UINavigationController navigationController)
		{
			navigationBarAreaHeight = navigationController.NavigationBar.Frame.GetMaxY();
		}
		else
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13, 0))
				statusBarHeight = window.WindowScene?.StatusBarManager?.StatusBarFrame.Height ?? 0;
			else
				statusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;

			navigationBarAreaHeight = statusBarHeight;
		}

		var topLayoutGuide = Math.Max(navigationBarAreaHeight, ContainerView.LayoutMargins.Top) + 5;

		var keyboardYPosition = window.Frame.Height - kbSize.Height - TextViewTopDistance;

		var viewRectInWindow = View.ConvertRectToView(View.Bounds, window);

		// readjust contentInset when the textView height is too large for the screen
		var rootSuperViewFrameInWindow = window.Frame;
		if (ContainerView.Superview is UIView v)
			rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

		var cursorRect = (CGRect)CursorRect;

		nfloat cursorNotInViewScroll = 0;
		nfloat move = 0;
		bool cursorTooHigh = false;
		bool cursorTooLow = false;

		if (cursorRect.Y >= viewRectInWindow.GetMaxY())
		{
			cursorNotInViewScroll = viewRectInWindow.GetMaxY() - cursorRect.GetMaxY();
			move = cursorRect.Y - keyboardYPosition + cursorNotInViewScroll;
			cursorTooLow = true;
		}

		else if (cursorRect.Y < viewRectInWindow.GetMinY())
		{
			cursorNotInViewScroll = viewRectInWindow.GetMinY() - cursorRect.Y;
			move = cursorRect.Y - keyboardYPosition + cursorNotInViewScroll;
			cursorTooHigh = true;

			// no need to move the screen down if we can already see the view
			if (move < 0)
				move = 0;
		}

		else if (cursorRect.Y >= topLayoutGuide && cursorRect.Y < keyboardYPosition)
			return;

		else if (cursorRect.Y > keyboardYPosition)
			move = cursorRect.Y - keyboardYPosition;

		else if (cursorRect.Y <= topLayoutGuide)
			move = cursorRect.Y - (nfloat)topLayoutGuide;

		// Find the next parent ScrollView that is scrollable
		var superView = View.FindResponder<UIScrollView>();
		var superScrollView = FindParentScroll(superView);

		// This is the case when the keyboard is already showing and we click another editor/entry
		if (LastScrollView is not null)
		{
			// if there is not a current superScrollView, restore LastScrollView
			if (superScrollView is null)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
					UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, AnimateStartingLastScrollView, () => { });

				if (!LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (View.FindResponder<UIStackView>() is UIStackView)
						LastScrollView.SetContentOffset(StartingContentOffset, UIView.AnimationsEnabled);
					else
						LastScrollView.ContentOffset = StartingContentOffset;
				}

				StartingContentInsets = new UIEdgeInsets();
				StartingScrollIndicatorInsets = new UIEdgeInsets();
				StartingContentOffset = new CGPoint(0, 0);
				LastScrollView = null;
			}
		}

		else if (superScrollView is not null)
		{
			LastScrollView = superScrollView;
			StartingContentInsets = superScrollView.ContentInset;
			StartingContentOffset = superScrollView.ContentOffset;

			StartingScrollIndicatorInsets = OperatingSystem.IsIOSVersionAtLeast(11, 1) ?
				superScrollView.VerticalScrollIndicatorInsets : superScrollView.ScrollIndicatorInsets;
		}

		// Calculate the move for the ScrollViews
		if (LastScrollView is not null)
		{
			var lastView = View;
			superScrollView = LastScrollView;
			nfloat innerScrollValue = 0;

			while (superScrollView is not null)
			{
				var shouldContinue = false;

				if (move > 0)
					shouldContinue = move > -superScrollView.ContentOffset.Y - superScrollView.ContentInset.Top;

				else if (superScrollView.FindResponder<UITableView>() is UITableView tableView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && View.FindResponder<UITableViewCell>() is UITableViewCell tableCell
						&& tableView.IndexPathForCell(tableCell) is NSIndexPath indexPath
						&& tableView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath)
					{
						var previousCellRect = tableView.RectForRowAtIndexPath(previousIndexPath);
						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = tableView.ConvertRectToView(previousCellRect, ContainerView.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}
				}

				else if (superScrollView.FindResponder<UICollectionView>() is UICollectionView collectionView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && View.FindResponder<UICollectionViewCell>() is UICollectionViewCell collectionCell
						&& collectionView.IndexPathForCell(collectionCell) is NSIndexPath indexPath
						&& collectionView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath
						&& collectionView.GetLayoutAttributesForItem(previousIndexPath) is UICollectionViewLayoutAttributes attributes)
					{
						var previousCellRect = attributes.Frame;

						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = collectionView.ConvertRectToView(previousCellRect, ContainerView.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}
				}

				else
				{
					shouldContinue = !(innerScrollValue == 0
						&& cursorRect.Y + cursorNotInViewScroll >= topLayoutGuide
						&& cursorRect.Y + cursorNotInViewScroll <= keyboardYPosition);

					if (cursorRect.Y - innerScrollValue < topLayoutGuide && !cursorTooHigh)
						move = cursorRect.Y - innerScrollValue - (nfloat)topLayoutGuide;
					else if (cursorRect.Y - innerScrollValue > keyboardYPosition && !cursorTooLow)
						move = cursorRect.Y - innerScrollValue - keyboardYPosition;
				}

				// Go up the hierarchy and look for other scrollViews until we reach the UIWindow
				if (shouldContinue)
				{
					var tempScrollView = superScrollView.FindResponder<UIScrollView>();
					var nextScrollView = FindParentScroll(tempScrollView);

					var shouldOffsetY = superScrollView.ContentOffset.Y - Math.Min(superScrollView.ContentOffset.Y, -move);

					// the contentOffset.Y will change to shouldOffSetY so we can subtract the difference from the move
					move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);

					var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, shouldOffsetY);

					if (!superScrollView.ContentOffset.Equals(newContentOffset) || innerScrollValue != 0)
					{
						// if we can scroll the superScrollView and still not be above keyboard, pass scrolling to the parent
						var superScrollViewRect = superScrollView.ConvertRectToView(superScrollView.Bounds, window);

						if (nextScrollView is null && superScrollViewRect.Y < keyboardYPosition)
						{
							UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
							{
								newContentOffset.Y += innerScrollValue;
								innerScrollValue = 0;
								ScrolledView = superScrollView;

								if (View.FindResponder<UIStackView>() is not null)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							}, () => { });
						}

						else
						{
							// add the amount we would have moved to the next scroll value
							innerScrollValue += newContentOffset.Y - superScrollView.ContentOffset.Y;
						}
					}

					lastView = superScrollView;
					superScrollView = nextScrollView;
				}

				else
				{
					// if we did not get to scroll all the way, add the value to move
					move += innerScrollValue;
					break;
				}
			}

			move += innerScrollValue;

			// ContentInset logic
			if (ScrolledView is not null)
			{
				var bottomInset = ScrolledView.Bounds.Height + ScrolledView.ContentOffset.Y - ScrolledView.ContentSize.Height;
				var bottomScrollIndicatorInset = bottomInset - TextViewTopDistance;

				bottomInset = nfloat.Max(StartingContentInsets.Bottom, bottomInset);
				bottomScrollIndicatorInset = nfloat.Max(StartingScrollIndicatorInsets.Bottom, bottomScrollIndicatorInset);

				if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
				{
					bottomInset -= ScrolledView.SafeAreaInsets.Bottom;
					bottomScrollIndicatorInset -= ScrolledView.SafeAreaInsets.Bottom;
				}

				var movedInsets = ScrolledView.ContentInset;
				movedInsets.Bottom = bottomInset;

				if (LastScrollView.ContentInset != movedInsets)
					UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateInset(ScrolledView, movedInsets, bottomScrollIndicatorInset), () => { });
			}
		}

		if (move >= 0)
		{
			rootViewOrigin.Y = (nfloat)Math.Max(rootViewOrigin.Y - move, Math.Min(0, -kbSize.Height - TextViewTopDistance));

			if (ContainerView.Frame.X != rootViewOrigin.X || ContainerView.Frame.Y != rootViewOrigin.Y)
			{
				var rect = ContainerView.Frame;
				rect.X = rootViewOrigin.X;
				rect.Y = rootViewOrigin.Y;

				UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateRootView(rect), () => { });
			}
		}

		else
		{
			rootViewOrigin.Y -= move;

			if (ContainerView.Frame.X != rootViewOrigin.X || ContainerView.Frame.Y != rootViewOrigin.Y)
			{
				var rect = ContainerView.Frame;
				rect.X = rootViewOrigin.X;
				rect.Y = rootViewOrigin.Y;

				UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateRootView(rect), () => { });
			}
		}
	}

	static void AnimateInset(UIScrollView? scrollView, UIEdgeInsets movedInsets, nfloat bottomScrollIndicatorInset)
	{
		if (scrollView is null)
			return;

		scrollView.ContentInset = movedInsets;
		UIEdgeInsets newscrollIndicatorInset;

		if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
			newscrollIndicatorInset = scrollView.VerticalScrollIndicatorInsets;
		else
			newscrollIndicatorInset = scrollView.ScrollIndicatorInsets;

		newscrollIndicatorInset.Bottom = bottomScrollIndicatorInset;
		scrollView.ScrollIndicatorInsets = newscrollIndicatorInset;
	}

	static void AnimateStartingLastScrollView()
	{
		if (LastScrollView is not null)
		{
			LastScrollView.ContentInset = StartingContentInsets;
			LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
		}
	}

	static void AnimateRootView(CGRect rect)
	{
		if (ContainerView is not null)
			ContainerView.Frame = rect;
	}

	static UIScrollView? FindParentScroll(UIScrollView? view)
	{
		while (view is not null)
		{
			if (view.ScrollEnabled)
				return view;

			view = view.FindResponder<UIScrollView>();
		}

		return null;
	}

	internal static nfloat FindKeyboardHeight()
	{
		if (ContainerView is null)
			return 0;

		var window = ContainerView.Window;
		var intersectRect = CGRect.Intersect(KeyboardFrame, window.Frame);
		var kbSize = intersectRect == CGRect.Empty ? new CGSize(KeyboardFrame.Width, 0) : intersectRect.Size;

		return window.Frame.Height - kbSize.Height;
	}

	static void RestorePosition()
	{
		if (ContainerView is not null
			&& (ContainerView.Frame.X != TopViewBeginOrigin.X || ContainerView.Frame.Y != TopViewBeginOrigin.Y)
			&& TopViewBeginOrigin != InvalidPoint)
		{
			var rect = ContainerView.Frame;
			rect.X = TopViewBeginOrigin.X;
			rect.Y = TopViewBeginOrigin.Y;

			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateRootView(rect), () => { });
		}

		if (ScrolledView is not null && ScrolledView.ContentInset != UIEdgeInsets.Zero)
			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateInset(ScrolledView, UIEdgeInsets.Zero, 0), () => { });

		ScrolledView = null;
		View = null;
		ContainerView = null;
		TopViewBeginOrigin = InvalidPoint;
		CursorRect = null;
	}

	static NSIndexPath? GetPreviousIndexPath(this UIScrollView scrollView, NSIndexPath indexPath)
	{
		var previousRow = indexPath.Row - 1;
		var previousSection = indexPath.Section;

		if (previousRow < 0)
		{
			previousSection -= 1;
			if (previousSection >= 0 && scrollView is UICollectionView collectionView)
				previousRow = (int)(collectionView.NumberOfItemsInSection(previousSection) - 1);
			else if (previousSection >= 0 && scrollView is UITableView tableView)
				previousRow = (int)(tableView.NumberOfRowsInSection(previousSection) - 1);
			else
				return null;
		}

		if (previousRow >= 0 && previousSection >= 0)
			return NSIndexPath.FromRowSection(previousRow, previousSection);
		else
			return null;
	}
}
