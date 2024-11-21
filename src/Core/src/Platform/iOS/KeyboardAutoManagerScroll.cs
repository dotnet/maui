/*
 * This class is adapted from IQKeyboardManager which is an open-source
 * library implemented for iOS to handle Keyboard interactions with
 * UITextFields/UITextViews. Link to their MIT License can be found here:
 * https://github.com/hackiftekhar/IQKeyboardManager/blob/7399efb730eea084571b45a1a9b36a3a3c54c44f/LICENSE.md
 */

using System;
using System.Text;
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
	static int TextViewDistanceFromBottom = 20;
	static NSObject? WillShowToken;
	static NSObject? WillHideToken;
	static NSObject? DidHideToken;
	static NSObject? TextFieldToken;
	static NSObject? TextViewToken;
	internal static bool ShouldDisconnectLifecycle;
	internal static bool ShouldIgnoreSafeAreaAdjustment;
	internal static bool ShouldScrollAgain;

	/// <summary>
	/// Enables automatic scrolling with keyboard interactions on iOS devices.
	/// </summary>
	/// <remarks>
	/// This method is being called by default on iOS and will scroll the page when the keyboard
	/// comes up. Call the method 'KeyboardAutoManagerScroll.Disconnect()'
	/// to remove this scrolling behavior.
	/// </remarks>
	public static void Connect()
	{
		if (TextFieldToken is not null)
		{
			return;
		}

		TextFieldToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextFieldTextDidBeginEditingNotification"), DidUITextBeginEditing);

		TextViewToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextViewTextDidBeginEditingNotification"), DidUITextBeginEditing);

		WillShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), WillKeyboardShow);

		WillHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillHideNotification"), WillHideKeyboard);

		DidHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardDidHideNotification"), DidHideKeyboard);
	}

	/// <summary>
	/// Disables automatic scrolling with keyboard interactions on iOS devices.
	/// </summary>
	/// <remarks>
	/// When this method is called, scrolling will not automatically happen when the keyboard comes up.
	/// </remarks>
	public static void Disconnect()
	{
		// if Disconnect is called prior to Connect, signal to not
		// Connect during the Created Lifecycle event
		ShouldDisconnectLifecycle = true;

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

	static void DidUITextBeginEditing(NSNotification notification)
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

			AdjustPositionDebounce().FireAndForget();
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
		if (localCursor is CGRect local && ContainerView is not null)
		{
			var cursorInContainer = ContainerView.ConvertRectFromView(local, View);
			var cursorInWindow = ContainerView.ConvertRectToView(cursorInContainer, null);
			return cursorInWindow;
		}

		return null;
	}

	static void WillKeyboardShow(NSNotification notification)
	{
		var userInfo = notification.UserInfo;
		var oldKeyboardFrame = KeyboardFrame;

		if (userInfo is not null)
		{
			var frameSize = userInfo.FindValue("UIKeyboardFrameEndUserInfoKey");
			var frameSizeRect = DescriptionToCGRect(frameSize?.Description);
			if (frameSizeRect is not null)
			{
				KeyboardFrame = (CGRect)frameSizeRect;
			}

			userInfo.SetAnimationDuration();
		}

		if (!IsKeyboardShowing)
		{
			IsKeyboardShowing = true;
			AdjustPositionDebounce().FireAndForget();
		}
		else if (oldKeyboardFrame != KeyboardFrame && IsKeyboardShowing)
		{
			// this could be the case if the keyboard is already showing but type of keyboard changes
			AdjustPositionDebounce().FireAndForget();
		}
	}

	static void WillHideKeyboard(NSNotification notification)
	{
		notification.UserInfo?.SetAnimationDuration();

		if (LastScrollView is not null)
		{
			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, AnimateHidingKeyboard, () => { });
		}

		if (IsKeyboardShowing)
		{
			RestorePosition();
		}

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
		ShouldIgnoreSafeAreaAdjustment = false;
		ShouldScrollAgain = false;
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
		{
			AnimationDuration = num;
		}
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
					{
						superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
					}
					else
					{
						superScrollView.ContentOffset = newContentOffset;
					}
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
		{
			return null;
		}

		var temp = RemoveEverythingExceptForNumbersAndCommas(description);
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

		static string RemoveEverythingExceptForNumbersAndCommas(string input)
		{
			var sb = new StringBuilder(input.Length);
			foreach (var character in input)
			{
				if (char.IsDigit(character) || character == ',')
				{
					sb.Append(character);
				}
			}
			return sb.ToString();
		}
	}

	// Used to debounce calls from different oberservers so we can be sure
	// all the fields are updated before calling AdjustPostition()
	internal static async Task AdjustPositionDebounce()
	{
		if (IsKeyboardShowing)
		{
			// If we are going to a new view that has an InputAccessoryView
			// while we have the keyboard up, we need a delay to recalculate
			// the height of the InputAccessoryView
			if (View?.InputAccessoryView is not null)
			{
				await Task.Delay(30);
			}
			AdjustPosition();

			// See if the layout requests to scroll again after our initial scroll
			await Task.Delay(5);
			if (ShouldScrollAgain)
			{
				AdjustPosition();
			}
		}
	}

	// main method to calculate and animate the scrolling
	internal static void AdjustPosition()
	{
		if (ContainerView is null
			|| (View is not UITextField && View is not UITextView)
			|| !View.IsDescendantOfView(ContainerView))
		{
			IsKeyboardAutoScrollHandling = false;
			return;
		}

		if (TopViewBeginOrigin == InvalidPoint)
		{
			TopViewBeginOrigin = new CGPoint(ContainerView.Frame.X, ContainerView.Frame.Y);
		}

		var rootViewOrigin = new CGPoint(ContainerView.Frame.GetMinX(), ContainerView.Frame.GetMinY());
		var window = ContainerView.Window;

		if (window is null)
		{
			IsKeyboardAutoScrollHandling = false;
			return;
		}

		var intersectRect = CGRect.Intersect(KeyboardFrame, window.Frame);
		var kbSize = intersectRect == CGRect.Empty ? new CGSize(KeyboardFrame.Width, 0) : intersectRect.Size;

		nfloat statusBarHeight;
		nfloat navigationBarAreaHeight;

		if (View.FindResponder<UINavigationController>() is UINavigationController navigationController)
		{
			if (View.IsDescendantOfView(navigationController.NavigationBar))
			{
				IsKeyboardAutoScrollHandling = false;
				return;
			}

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

		var topLayoutGuide = Math.Max(navigationBarAreaHeight, ContainerView.LayoutMargins.Top);

		// calculate the cursor rect
		CursorRect = FindCursorPosition();

		if (CursorRect is null)
		{
			IsKeyboardAutoScrollHandling = false;
			return;
		}

		var cursorRect = (CGRect)CursorRect;

		var viewRectInContainer = ContainerView.ConvertRectFromView(View.Frame, View.Superview);
		var viewRectInWindow = ContainerView.ConvertRectToView(viewRectInContainer, null);

		// since the cursorRect does not have a height for Pickers, we can assign the height of the picker as the cursor height
		if (cursorRect.Height == 0)
		{
			cursorRect.Height = View.Bounds.Height;
		}

		var keyboardYPosition = window.Frame.Height - kbSize.Height - TextViewDistanceFromBottom;

		// readjust contentInset when the textView height is too large for the screen
		var rootSuperViewFrameInWindow = window.Frame;
		if (ContainerView.Superview is UIView v)
		{
			rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);
		}

		nfloat cursorNotInViewScroll = 0;
		nfloat move = 0;
		bool cursorTooHigh = false;
		bool cursorTooLow = false;

		// Find the next parent ScrollView that is scrollable or use the current View if it is a ScrollView
		var superView = View.FindResponder<UIScrollView>() ?? View as UIScrollView;
		var superScrollView = FindParentScroll(superView);

		CGRect? superScrollViewRect = null;
		var topBoundary = topLayoutGuide;
		var bottomBoundary = (double)keyboardYPosition;

		if (superScrollView is not null)
		{
			var superScrollInContainer = ContainerView.ConvertRectFromView(superScrollView.Frame, superScrollView.Superview);
			superScrollViewRect = ContainerView.ConvertRectToView(superScrollInContainer, null);
			topBoundary = Math.Max(topBoundary, superScrollViewRect.Value.Top);
			var superScrollViewBottom = superScrollViewRect.Value.Bottom - TextViewDistanceFromBottom;

			// if the superScrollView is a small editor, it may not make sense to scroll the entire screen if cursor is visible
			if (superScrollView is UITextView && superScrollViewRect.Value.Bottom - TextViewDistanceFromBottom < cursorRect.Bottom)
			{
				superScrollViewBottom = superScrollViewRect.Value.Bottom;
			}

			bottomBoundary = Math.Min(bottomBoundary, superScrollViewBottom);
		}

		bool forceSetContentInsets = true;

		// scenario where we go into an editor with the "Next" keyboard button,
		// but the cursor location on the editor is scrolled below the visible section
		if (View is UITextView && IsKeyboardShowing && cursorRect.Bottom >= viewRectInWindow.GetMaxY())
		{
			move = viewRectInWindow.Bottom - (nfloat)bottomBoundary;
		}

		// scenario where we go into an editor with the "Next" keyboard button,
		// but the cursor location on the editor is scrolled above the visible section
		else if (View is UITextView && IsKeyboardShowing && cursorRect.Y < viewRectInWindow.GetMinY())
		{
			move = viewRectInWindow.Top - (nfloat)bottomBoundary;

			// no need to move the screen down if we can already see the view
			if (move < 0)
			{
				move = 0;
			}
		}

		else if (cursorRect.Bottom > bottomBoundary && cursorRect.Y > topBoundary)
		{
			move = cursorRect.Bottom - (nfloat)bottomBoundary;
		}

		else if (cursorRect.Y <= topBoundary && cursorRect.Bottom <= bottomBoundary)
		{
			move = cursorRect.Y - (nfloat)topBoundary;
		}

		else if (cursorRect.Y <= topBoundary && cursorRect.Bottom >= bottomBoundary)
		{
			cursorNotInViewScroll = viewRectInWindow.GetMinY() - cursorRect.Y;
			move = cursorRect.Bottom - (nfloat)bottomBoundary - cursorNotInViewScroll;
			cursorTooHigh = true;
		}

		// This is the case when the keyboard is already showing and we click another editor/entry
		if (LastScrollView is not null)
		{
			// if there is not a current superScrollView, restore LastScrollView
			if (superScrollView is null)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, AnimateStartingLastScrollView, () => { });
				}

				if (!LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (View.FindResponder<UIStackView>() is UIStackView)
					{
						LastScrollView.SetContentOffset(StartingContentOffset, UIView.AnimationsEnabled);
					}
					else
					{
						LastScrollView.ContentOffset = StartingContentOffset;
					}
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
			nfloat tempMove = 0;

			while (superScrollView is not null)
			{
				var shouldContinue = false;

				// if we have an innerScrollValue, let's move with this value first and then do the move
				if (cursorNotInViewScroll != 0)
				{
					tempMove = move;
					move = cursorNotInViewScroll;
					shouldContinue = true;
				}

				else if (move > 0 || tempMove > 0)
				{
					if (move == 0)
					{
						move = tempMove;
					}
					shouldContinue = move > -superScrollView.ContentOffset.Y - superScrollView.ContentInset.Top;
				}

				else if (superScrollView.FindResponder<UITableView>() is UITableView tableView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && View?.FindResponder<UITableViewCell>() is UITableViewCell tableCell
						&& tableView.IndexPathForCell(tableCell) is NSIndexPath indexPath
						&& tableView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath)
					{
						var previousCellRect = tableView.RectForRowAtIndexPath(previousIndexPath);
						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = tableView.ConvertRectToView(previousCellRect, ContainerView.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topBoundary);
						}
					}
				}

				else if (superScrollView.FindResponder<UICollectionView>() is UICollectionView collectionView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && View?.FindResponder<UICollectionViewCell>() is UICollectionViewCell collectionCell
						&& collectionView.IndexPathForCell(collectionCell) is NSIndexPath indexPath
						&& collectionView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath
						&& collectionView.GetLayoutAttributesForItem(previousIndexPath) is UICollectionViewLayoutAttributes attributes)
					{
						var previousCellRect = attributes.Frame;

						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = collectionView.ConvertRectToView(previousCellRect, ContainerView.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topBoundary);
						}
					}
				}

				else
				{
					shouldContinue = !(innerScrollValue == 0
						&& cursorRect.Y + cursorNotInViewScroll >= topBoundary
						&& cursorRect.Bottom + cursorNotInViewScroll <= bottomBoundary);

					if (cursorRect.Y - innerScrollValue < topBoundary && !cursorTooHigh)
					{
						move = cursorRect.Y - innerScrollValue - (nfloat)topBoundary;
					}
					else if (cursorRect.Y - innerScrollValue > bottomBoundary && !cursorTooLow)
					{
						move = cursorRect.Y - innerScrollValue - (nfloat)bottomBoundary;
					}
				}

				// Go up the hierarchy and look for other scrollViews until we reach the UIWindow
				if (shouldContinue)
				{
					forceSetContentInsets = false;

					var tempScrollView = superScrollView.FindResponder<UIScrollView>();
					var nextScrollView = FindParentScroll(tempScrollView);

					// if PrefersLargeTitles is true, we may need additional logic to handle the collapsable navbar
					var navController = View?.FindResponder<UINavigationController>();
					var prefersLargeTitles = navController?.NavigationBar.PrefersLargeTitles ?? false;

					if (prefersLargeTitles)
					{
						move = AdjustForLargeTitles(move, superScrollView, navController!);
					}

					var origContentOffsetY = superScrollView.ContentOffset.Y;
					var shouldOffsetY = superScrollView.ContentOffset.Y - Math.Min(superScrollView.ContentOffset.Y, -move);
					var requestedMove = move;

					// the contentOffset.Y will change to shouldOffSetY so we can subtract the difference from the move
					move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);

					var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, shouldOffsetY);

					if ((!superScrollView.ContentOffset.Equals(newContentOffset) || innerScrollValue != 0) && superScrollViewRect is not null)
					{
						if ((nextScrollView is null && superScrollViewRect.Value.Y + cursorRect.Height + TextViewDistanceFromBottom < bottomBoundary) ||
							cursorNotInViewScroll != 0)
						{
							UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
							{
								newContentOffset.Y += innerScrollValue;
								innerScrollValue = 0;
								ScrolledView = superScrollView;

								if (View?.FindResponder<UIStackView>() is not null)
								{
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								}
								else
								{
									superScrollView.ContentOffset = newContentOffset;
								}
							}, () => { });

							// after this scroll finishes, there is an edge case where if we have Large Titles,
							// the entire requeseted scroll amount may not be allowed. If so, we need to scroll again.
							var actualScrolledAmount = superScrollView.ContentOffset.Y - origContentOffsetY;
							var amountNotScrolled = requestedMove - actualScrolledAmount;

							if (prefersLargeTitles && amountNotScrolled > 1)
							{
								ShouldScrollAgain = true;
							}
						}

						else
						{
							// add the amount we would have moved to the next scroll value
							innerScrollValue += newContentOffset.Y - superScrollView.ContentOffset.Y;
						}
					}

					// if we needed to scroll for cursorNotInViewScroll first, use the same superScrollView and handle the move now
					if (cursorNotInViewScroll != 0)
					{
						cursorNotInViewScroll = 0;
					}
					else
					{
						lastView = superScrollView;
						superScrollView = nextScrollView;
					}
				}

				else
				{
					// if we did not get to scroll all the way, add the value to move
					move += innerScrollValue;
					break;
				}
			}

			move += innerScrollValue;

			// Adjust the parent's ContentInset.Bottom so we can still scroll to the top with the keyboard showing
			if (forceSetContentInsets && superScrollView is not null)
			{
				ApplyContentInset(superScrollView, LastScrollView, false, false);
				// if our View is an editor, we can adjust the ContentInset.Bottom so that the text cursor will stay above the keyboard
				if (superScrollView != View && View is UITextView textView)
				{
					ApplyContentInset(textView, textView, false, true);
				}
			}
			else
			{
				ApplyContentInset(ScrolledView, LastScrollView, true, false);
				// if our View is an editor, we can adjust the ContentInset.Bottom so that the text cursor will stay above the keyboard
				if (ScrolledView != View && View is UITextView textView)
				{
					ApplyContentInset(textView, textView, true, true);
				}
			}
		}

		if (move >= 0)
		{
			rootViewOrigin.Y = (nfloat)Math.Max(rootViewOrigin.Y - move, Math.Min(0, -kbSize.Height - TextViewDistanceFromBottom));

			if (ContainerView.Frame.X != rootViewOrigin.X || ContainerView.Frame.Y != rootViewOrigin.Y)
			{
				ShouldIgnoreSafeAreaAdjustment = true;
				var rect = ContainerView.Frame;
				rect.X = rootViewOrigin.X;
				rect.Y = rootViewOrigin.Y;

				UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateRootView(rect), () => { });

				// this is the scenario where there is a scrollview, but the whole scrollview is below
				// where the keyboard will be. We need to scroll the ContainerView and add ContentInsets to the scrollview.
				if (LastScrollView is not null)
				{
					ApplyContentInset(LastScrollView, LastScrollView, false, false);
				}
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
		{
			return;
		}

		scrollView.ContentInset = movedInsets;
		UIEdgeInsets newscrollIndicatorInset;

		if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
		{
			newscrollIndicatorInset = scrollView.VerticalScrollIndicatorInsets;
		}
		else
		{
			newscrollIndicatorInset = scrollView.ScrollIndicatorInsets;
		}

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
		{
			ContainerView.Frame = rect;
		}
	}

	// Adjusts the ContentInset of our view that Scrolled so that we can still scroll to the top and bottom with the keyboard showing.
	static void ApplyContentInset(UIScrollView? scrolledView, UIScrollView? lastScrollView, bool didMove, bool isInnerEditor)
	{
		if (scrolledView is null || lastScrollView is null || ContainerView is null)
		{
			return;
		}

		var frameInContainer = ContainerView.ConvertRectFromView(scrolledView.Frame, scrolledView.Superview);
		var frameInWindow = ContainerView.ConvertRectToView(frameInContainer, null);

		var keyboardIntersect = CGRect.Intersect(KeyboardFrame, frameInWindow);

		var bottomInset = keyboardIntersect.Height;

		// For new lines in an editor, we want the cursor to stay right above the keyboard.
		// When adding contentInsets for a scrollview, it is nice to have a little extra padding.
		if (scrolledView is not UITextView && keyboardIntersect.Height > 0)
		{
			bottomInset += TextViewDistanceFromBottom;
		}

		var bottomScrollIndicatorInset = bottomInset;

		bottomInset = nfloat.Max(StartingContentInsets.Bottom, bottomInset);
		bottomScrollIndicatorInset = nfloat.Max(StartingScrollIndicatorInsets.Bottom, bottomScrollIndicatorInset);

		if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
		{
			bottomInset -= scrolledView.SafeAreaInsets.Bottom;
			bottomScrollIndicatorInset -= scrolledView.SafeAreaInsets.Bottom;
		}

		var movedInsets = scrolledView.ContentInset;
		movedInsets.Bottom = bottomInset;

		// if we are in an editor that is inside a scrollView and are below where the keyboard will appear,
		// the outer scrollview will put the cursor above the keyboard and we will
		// need to add a bottom inset to the inner editor so that the cursor will
		// stay above the keyboard when we add new lines.
		if (didMove && isInnerEditor && scrolledView is UITextView textView)
		{
			var cursorRect = FindCursorPosition();
			if (cursorRect is CGRect cursor)
			{
				var editorBottomInset = frameInWindow.Bottom - cursor.Bottom - TextViewDistanceFromBottom;
				movedInsets.Bottom = nfloat.Max(0, editorBottomInset);
				bottomScrollIndicatorInset = nfloat.Max(0, editorBottomInset);
			}
		}

		if (lastScrollView.ContentInset != movedInsets)
		{
			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateInset(scrolledView, movedInsets, bottomScrollIndicatorInset), () => { });
		}
	}

	static UIScrollView? FindParentScroll(UIScrollView? view)
	{
		while (view is not null)
		{
			if (view.ScrollEnabled && !IsHorizontalCollectionView(view))
			{
				return view;
			}

			view = view.FindResponder<UIScrollView>();
		}

		return null;
	}

	static bool IsHorizontalCollectionView(UIView collectionView)
	=> collectionView is UICollectionView { CollectionViewLayout: UICollectionViewFlowLayout { ScrollDirection: UICollectionViewScrollDirection.Horizontal } };

	internal static nfloat FindKeyboardHeight()
	{
		if (ContainerView is null)
		{
			return 0;
		}

		var window = ContainerView.Window;
		var intersectRect = CGRect.Intersect(KeyboardFrame, window.Frame);
		var kbSize = intersectRect == CGRect.Empty ? new CGSize(KeyboardFrame.Width, 0) : intersectRect.Size;

		return window.Frame.Height - kbSize.Height;
	}

	// In the case we have PrefersLargeTitles set to true, the UINavigationBar
	// has additional height that collapses when scrolling. Try to remove
	// this collapsable height difference from the calculated move distance.
	static nfloat AdjustForLargeTitles(nfloat move, UIScrollView superScrollView, UINavigationController navController)
	{
		// The Large Titles will be presented in Portrait modes on iPhones and all orientations of iPads,
		// so skip if we are not in those scenarios.
		if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone
			&& (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeLeft || UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeRight))
		{
			return move;
		}

		// These values are not publicly available but can be tested. It is possible that these can change in the future.
		var navBarCollapsedHeight = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 44 : 50;
		var navBarExpandedHeight = navController.NavigationBar.SizeThatFits(new CGSize(0, 0)).Height;

		var minPageMoveToCollapseNavBar = navBarExpandedHeight - navBarCollapsedHeight;
		var amountScrolled = superScrollView.ContentOffset.Y;
		var amountLeftToCollapseNavBar = minPageMoveToCollapseNavBar - amountScrolled;
		var navBarCollapseDifference = navController.NavigationBar.Frame.Height - navBarCollapsedHeight;

		// if the navbar will collapse from our scroll
		if (move >= amountLeftToCollapseNavBar)
		{
			// if subtracting navBarCollapseDifference from our scroll
			// will cause the collapse not to happen, we need to scroll
			// to the minimum amount that will cause the collapse or else
			// we will not see our view
			if (move - navBarCollapseDifference < amountLeftToCollapseNavBar)
			{
				return amountLeftToCollapseNavBar;
			}

			// else the navBar will collapse and we want to subtract the navBarCollapseDifference to account for it
			else
			{
				return move - navBarCollapseDifference;
			}
		}
		return move;
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
		{
			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateInset(ScrolledView, UIEdgeInsets.Zero, 0), () => { });
		}

		if (View is not null && View is UIScrollView editorScrollView && editorScrollView.ContentInset != UIEdgeInsets.Zero && View is UITextView textView)
		{
			UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () => AnimateInset(editorScrollView, UIEdgeInsets.Zero, 0), () => { });
		}

		ScrolledView = null;
		View = null;
		ContainerView = null;
		TopViewBeginOrigin = InvalidPoint;
		CursorRect = null;
		ShouldIgnoreSafeAreaAdjustment = false;
		ShouldScrollAgain = false;
	}

	static NSIndexPath? GetPreviousIndexPath(this UIScrollView scrollView, NSIndexPath indexPath)
	{
		var previousRow = indexPath.Row - 1;
		var previousSection = indexPath.Section;

		if (previousRow < 0)
		{
			previousSection -= 1;
			if (previousSection >= 0 && scrollView is UICollectionView collectionView)
			{
				previousRow = (int)(collectionView.NumberOfItemsInSection(previousSection) - 1);
			}
			else if (previousSection >= 0 && scrollView is UITableView tableView)
			{
				previousRow = (int)(tableView.NumberOfRowsInSection(previousSection) - 1);
			}
			else
			{
				return null;
			}
		}

		if (previousRow >= 0 && previousSection >= 0)
		{
			return NSIndexPath.FromRowSection(previousRow, previousSection);
		}
		else
		{
			return null;
		}
	}
}
