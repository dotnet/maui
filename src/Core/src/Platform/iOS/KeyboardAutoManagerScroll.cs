using System;
using CoreGraphics;
using Foundation;
using UIKit;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManagerScroll
{
	static UIScrollView? LastScrollView;
	static CGPoint StartingContentOffset;
	static UIEdgeInsets StartingScrollIndicatorInsets;
	static UIEdgeInsets StartingContentInsets;
	static CGRect KeyboardFrame = CGRect.Empty;
	static CGPoint TopViewBeginOrigin = new(nfloat.MaxValue, nfloat.MaxValue);
	static readonly CGPoint InvalidPoint = new(nfloat.MaxValue, nfloat.MaxValue);
	static double AnimationDuration = 0.25;
	static UIView? View = null;
	static UIView? RootController = null;
	static CGRect? CursorRect = null;
	static bool IsKeyboardShowing = false;
	static int TextViewTopDistance = 20;
	static int DebounceCount = 0;
	static NSObject? WillShowToken = null;
	static NSObject? DidHideToken = null;
	static NSObject? TextFieldToken = null;
	static NSObject? TextViewToken = null;

	// Set up the observers for the keyboard and the UITextField/UITextView
	internal static void Init()
	{
		TextFieldToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextFieldTextDidBeginEditingNotification"), async (notification) =>
		{
			if (notification.Object is not null)
			{
				View = (UIView)notification.Object;
				RootController = View.FindResponder<ContainerViewController>()?.View;

				await SetUpTextEdit();
			}
		});

		TextViewToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextViewTextDidBeginEditingNotification"), async (notification) =>
		{
			if (notification.Object is not null)
			{
				View = (UIView)notification.Object;
				RootController = View.FindResponder<ContainerViewController>()?.View;

				await SetUpTextEdit();
			}
		});

		WillShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillShowNotification"), async (notification) =>
		{
			NSObject? frameSize = null;
			NSObject? curveSize = null;

			var foundFrameSize = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardFrameEndUserInfoKey"), out frameSize);
			if (foundFrameSize == true && frameSize is not null)
			{
				var frameSizeRect = DescriptionToCGRect(frameSize.Description);
				if (frameSizeRect is not null)
					KeyboardFrame = (CGRect)frameSizeRect;
			}

			var foundAnimationDuration = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out curveSize);
			if (foundAnimationDuration == true && curveSize is not null)
			{
				var num = (NSNumber)NSObject.FromObject(curveSize);
				AnimationDuration = (double)num;
			}

			if (TopViewBeginOrigin == InvalidPoint && RootController is not null)
				TopViewBeginOrigin = new CGPoint(RootController.Frame.X, RootController.Frame.Y);

			if (!IsKeyboardShowing)
			{
				await AdjustPositionDebounce();
				IsKeyboardShowing = true;
			}
		});

		DidHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardWillHideNotification"), (notification) =>
		{
			NSObject? curveSize = null;

			var foundAnimationDuration = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out curveSize);
			if (foundAnimationDuration == true && curveSize is not null)
			{
				var num = (NSNumber)NSObject.FromObject(curveSize);
				AnimationDuration = (double)num;
			}

			if (LastScrollView is not null)
			{
				AnimateScroll(() =>
				{
					if (LastScrollView.ContentInset != StartingContentInsets)
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
						superScrollView = superScrollView.Superview as UIScrollView;
					}
				});
			}

			if (IsKeyboardShowing)
				RestorePosition();

			IsKeyboardShowing = false;
			View = null;
			LastScrollView = null;
			KeyboardFrame = CGRect.Empty;
			StartingContentInsets = new UIEdgeInsets();
			StartingScrollIndicatorInsets = new UIEdgeInsets();
			StartingContentInsets = new UIEdgeInsets();
		});
	}

	internal static void Destroy()
	{
		if (WillShowToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(WillShowToken);
		if (DidHideToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(DidHideToken);
		if (TextFieldToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(TextFieldToken);
		if (TextViewToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(TextViewToken);
	}

	// Used to get the numeric values from the UserInfo dictionary's NSObject value to CGRect.
	// Doing manually since CGRectFromString is not yet bound
	static CGRect? DescriptionToCGRect(string description)
	{
		// example of passed in description: "NSRect: {{0, 586}, {430, 346}}"

		if (description is null)
			return null;

		// remove letters in all languages, spaces, and curly brackets
		var temp = Regex.Replace(description, @"[\p{L}\s:{}]", "");
		var dimensions = temp.Split(',');

		if (int.TryParse(dimensions[0], out var x) && int.TryParse(dimensions[1], out var y)
			&& int.TryParse(dimensions[2], out var width) && int.TryParse(dimensions[3], out var height))
		{
			return new CGRect(x, y, width, height);
		}

		return null;
	}

	static async Task SetUpTextEdit()
	{
		if (View is null)
			return;

		CursorRect = null;

		RootController = View.FindResponder<ContainerViewController>()?.View;

		// the cursor needs a small amount of time to update the position
		await Task.Delay(5);

		UITextRange? selectedTextRange;
		CGRect? localCursor = null;

		if (View is UITextView tv)
		{
			selectedTextRange = tv.SelectedTextRange;
			if (selectedTextRange is UITextRange selectedRange)
			{
				localCursor = tv.GetCaretRectForPosition(selectedRange.Start);
				if (localCursor is CGRect local)
					CursorRect = tv.ConvertRectToView(local, null);
			}
		}
		else if (View is UITextField tf)
		{
			selectedTextRange = tf.SelectedTextRange;
			if (selectedTextRange is UITextRange selectedRange)
			{
				localCursor = tf.GetCaretRectForPosition(selectedRange.Start);
				if (localCursor is CGRect local)
					CursorRect = tf.ConvertRectToView(local, null);
			}
		}

		TextViewTopDistance = localCursor is CGRect cGRect ? 20 + (int)cGRect.Height : 20;

		await AdjustPositionDebounce();
	}

	// Used to debounce calls from different oberservers so we can be sure
	// all the fields are updated before calling AdjustPostition()
	internal static async Task AdjustPositionDebounce()
	{
		DebounceCount++;
		var entranceCount = DebounceCount;

		await Task.Delay(10);

		if (entranceCount == DebounceCount)
		{
			AdjustPosition();
			DebounceCount = 0;
		}
	}

	// main method to calculate and animate the scrolling
	internal static void AdjustPosition()
	{
		if (View is not UITextField field && View is not UITextView)
			return;

		if (RootController is null)
			return;

		var rootViewOrigin = new CGPoint(RootController.Frame.GetMinX(), RootController.Frame.GetMinY());
		var window = RootController.Window;

		var kbSize = KeyboardFrame.Size;
		var intersectRect = CGRect.Intersect(KeyboardFrame, window.Frame);
		if (intersectRect == CGRect.Empty)
			kbSize = new CGSize(KeyboardFrame.Width, 0);
		else
			kbSize = intersectRect.Size;

		nfloat statusBarHeight;
		nfloat navigationBarAreaHeight;

		if (RootController.GetNavigationController() is UINavigationController navigationController)
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

		var topLayoutGuide = Math.Max(navigationBarAreaHeight, RootController.LayoutMargins.Bottom) + 5;

		var keyboardYPosition = window.Frame.Height - kbSize.Height - TextViewTopDistance;

		CGRect cursorRect;

		if (CursorRect is CGRect cRect)
			cursorRect = cRect;
		else
			return;

		if (cursorRect.Y >= topLayoutGuide && cursorRect.Y < keyboardYPosition)
			return;

		nfloat move = 0;

		// readjust contentInset when the textView height is too large for the screen
		var rootSuperViewFrameInWindow = window.Frame;
		if (RootController.Superview is UIView v)
			rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

		if (cursorRect.Y > keyboardYPosition)
			move = cursorRect.Y - keyboardYPosition;

		else if (cursorRect.Y <= topLayoutGuide)
			move = cursorRect.Y - (nfloat)topLayoutGuide;

		// Find the next parent ScrollView that is scrollable
		UIScrollView? superScrollView = null;
		var superView = View.FindResponder<UIScrollView>();
		while (superView is not null)
		{
			if (superView.ScrollEnabled)
			{
				superScrollView = superView;
				break;
			}

			superView = superView.FindResponder<UIScrollView>();
		}

		// This is the case when the keyboard is already showing and we click another editor/entry
		if (LastScrollView is not null)
		{
			// if there is not a current superScrollView, restore LastScrollView
			if (superScrollView is null)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					AnimateScroll(() =>
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					});
				}

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

			// if we have different LastScrollView and superScrollViews, set the LastScrollView to the original frame
			// and set the LastScrollView as the superScrolView
			else if (superScrollView != LastScrollView)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					AnimateScroll(() =>
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					});
				}

				if (!LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (View.FindResponder<UIStackView>() is not null)
						LastScrollView.SetContentOffset(StartingContentOffset, UIView.AnimationsEnabled);
					else
						LastScrollView.ContentOffset = StartingContentOffset;
				}

				LastScrollView = superScrollView;
				if (superScrollView is not null)
				{
					StartingContentInsets = superScrollView.ContentInset;
					StartingContentOffset = superScrollView.ContentOffset;

					if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
						StartingScrollIndicatorInsets = superScrollView.VerticalScrollIndicatorInsets;
					else
						StartingScrollIndicatorInsets = superScrollView.ScrollIndicatorInsets;
				}
			}
		}

		// If there was no LastScrollView, but there is a superScrollView,
		// set the LastScrollView to be the superScrollView
		else if (superScrollView is not null)
		{
			LastScrollView = superScrollView;
			StartingContentInsets = superScrollView.ContentInset;
			StartingContentOffset = superScrollView.ContentOffset;

			if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
				StartingScrollIndicatorInsets = superScrollView.VerticalScrollIndicatorInsets;
			else
				StartingScrollIndicatorInsets = superScrollView.ScrollIndicatorInsets;
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
							var previousCellRectInRootSuperview = tableView.ConvertRectToView(previousCellRect, RootController.Superview);
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
							var previousCellRectInRootSuperview = collectionView.ConvertRectToView(previousCellRect, RootController.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}
				}

				else
				{
					if (cursorRect.Y - innerScrollValue >= topLayoutGuide && cursorRect.Y - innerScrollValue <= keyboardYPosition)
						shouldContinue = false;
					else
						shouldContinue = true;

					if (cursorRect.Y - innerScrollValue < topLayoutGuide)
						move = cursorRect.Y - innerScrollValue - (nfloat)topLayoutGuide;
					else if (cursorRect.Y - innerScrollValue > keyboardYPosition)
						move = cursorRect.Y - innerScrollValue - keyboardYPosition;
				}

				// Go up the hierarchy and look for other scrollViews until we reach the UIWindow
				if (shouldContinue)
				{
					var tempScrollView = superScrollView.FindResponder<UIScrollView>();
					UIScrollView? nextScrollView = null;

					// set tempScrollView to next scrollable superview of superScrollView
					while (tempScrollView is not null)
					{
						if (tempScrollView.ScrollEnabled)
						{
							nextScrollView = tempScrollView;
							break;
						}
						tempScrollView = tempScrollView.FindResponder<UIScrollView>();
					}

					var shouldOffsetY = superScrollView.ContentOffset.Y - Math.Min(superScrollView.ContentOffset.Y, -move);

					// the contentOffset.Y will change to shouldOffSetY so we can subtract the difference from the move
					move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);

					var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, shouldOffsetY);

					if (!superScrollView.ContentOffset.Equals(newContentOffset))
					{
						if (nextScrollView is null)
						{
							AnimateScroll(() =>
							{
								newContentOffset.Y += innerScrollValue;
								innerScrollValue = 0;

								if (View.FindResponder<UIStackView>() is not null)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							});
						}

						else
						{
							// add the amount we would have moved to the next scroll value
							innerScrollValue += newContentOffset.Y;
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
		}

		if (move >= 0)
		{
			rootViewOrigin.Y = (nfloat)Math.Max(rootViewOrigin.Y - move, Math.Min(0, -kbSize.Height - TextViewTopDistance));

			if (RootController.Frame.X != rootViewOrigin.X || RootController.Frame.Y != rootViewOrigin.Y)
			{
				AnimateScroll(() =>
				{
					var rect = RootController.Frame;
					rect.X = rootViewOrigin.X;
					rect.Y = rootViewOrigin.Y;

					RootController.Frame = rect;
				});
			}
		}

		else
		{
			var disturbDistance = rootViewOrigin.Y - TopViewBeginOrigin.Y;

			if (disturbDistance <= 0)
			{
				rootViewOrigin.Y -= (nfloat)Math.Max(move, disturbDistance);

				if (RootController.Frame.X != rootViewOrigin.X || RootController.Frame.Y != rootViewOrigin.Y)
				{
					AnimateScroll(() =>
					{
						var rect = RootController.Frame;
						rect.X = rootViewOrigin.X;
						rect.Y = rootViewOrigin.Y;

						RootController.Frame = rect;
					});
				}
			}
		}
	}

	static void AnimateScroll(Action? action)
	{
		UIView.Animate(AnimationDuration, 0, UIViewAnimationOptions.CurveEaseOut, () =>
		{
			action?.Invoke();
		}, () => { });
	}

	static void RestorePosition()
	{
		if (RootController is not null && (RootController.Frame.X != TopViewBeginOrigin.X || RootController.Frame.Y != TopViewBeginOrigin.Y))
		{
			AnimateScroll(() =>
			{
				var rect = RootController.Frame;
				rect.X = TopViewBeginOrigin.X;
				rect.Y = TopViewBeginOrigin.Y;

				RootController.Frame = rect;
			});
		}
		View = null;
		RootController = null;
		TopViewBeginOrigin = InvalidPoint;
		CursorRect = null;
	}

	static NSIndexPath? GetPreviousIndexPath(this UITableView tableView, NSIndexPath indexPath)
	{
		var previousRow = indexPath.Row - 1;
		var previousSection = indexPath.Section;

		if (previousRow < 0)
		{
			previousSection -= 1;
			if (previousSection >= 0)
				previousRow = (int)(tableView.NumberOfRowsInSection(previousSection) - 1);
		}

		if (previousRow >= 0 && previousSection >= 0)
			return NSIndexPath.FromRowSection(previousRow, previousSection);
		else
			return null;
	}

	static NSIndexPath? GetPreviousIndexPath(this UICollectionView collectionView, NSIndexPath indexPath)
	{
		var previousRow = indexPath.Row - 1;
		var previousSection = indexPath.Section;

		if (previousRow < 0)
		{
			previousSection -= 1;
			if (previousSection >= 0)
				previousRow = (int)(collectionView.NumberOfItemsInSection(previousSection) - 1);
		}

		if (previousRow >= 0 && previousSection >= 0)
			return NSIndexPath.FromRowSection(previousRow, previousSection);
		else
			return null;
	}
}
