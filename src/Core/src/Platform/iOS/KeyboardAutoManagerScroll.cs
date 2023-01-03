using System;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using UIKit;
using ObjCRuntime;
using System.Text;

namespace Microsoft.Maui.Platform;

internal static class KeyboardAutoManagerScroll
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0052
#pragma warning disable IDE0051
#pragma warning disable CS0649
#pragma warning disable CS0169
#pragma warning disable CS0414

	static nfloat MovedDistance;
	static UIScrollView? LastScrollView;
	static CGPoint StartingContentOffset;
	static UIEdgeInsets StartingScrollIndicatorInsets;
	static UIEdgeInsets StartingContentInsets;
	static UIEdgeInsets StartingTextViewContentInsets;
	static UIEdgeInsets StartingTextViewScrollIndicatorInsets;
	static bool IsTextViewContentInsetChanged;
	static bool HasPendingAdjustRequest;
	static bool ShouldIgnoreScrollingAdjustment;
	static bool ShouldRestoreScrollViewContentOffset;
	static bool ShouldIgnoreContentInsetAdjustment;

	static nfloat KeyboardDistanceFromTextField = 10.0f;
	static nfloat SearchBarKeyboardDistanceFromTextField = 15.0f; // not sure what value IQKeyboard uses
	static CGRect KeyboardFrame = CGRect.Empty;
	static double AnimationDuration = 0.25;
	static UIViewAnimationOptions AnimationCurve = UIViewAnimationOptions.CurveEaseOut;
	static UIEdgeInsets uIEdgeInsets = new UIEdgeInsets ();
	static bool LayoutifNeededOnUpdate = false;
	static CGPoint TopViewBeginOrigin = new CGPoint (nfloat.MaxValue, nfloat.MaxValue);

	static NSObject? WillShowToken = null;
	//static NSObject? DidShowToken = null;
	static NSObject? DidHideToken = null;
	static NSObject? TextFieldToken = null;
	static NSObject? TextViewToken = null;
	static bool IsKeyboardShowing = false;

	static UIView? view = null;
	static UIView? rootController = null;




	internal static void Init()
	{
		//DidShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardDidShowNotification"), (notification) =>
		//{
		//});

		//  UITextFieldTextDidBeginEditingNotification, UITextViewTextDidBeginEditingNotification
		TextFieldToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextFieldTextDidBeginEditingNotification"), (notification) =>
		{
			if (notification.Object is not null)
			{
				view = (UIView)notification.Object;
				rootController = view.GetViewController()?.GetContainerViewController();
			}
		});
		TextViewToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UITextViewTextDidBeginEditingNotification"), (notification) =>
		{
			if (notification.Object is not null)
			{
				view = (UIView)notification.Object;
				rootController = view.GetViewController()?.GetContainerViewController();
			}
		});


		WillShowToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString ("UIKeyboardWillShowNotification"), (notification) =>
		{
			NSObject? frameSize = null;
			NSObject? curveSize = null;

			var foundFrameSize = notification.UserInfo?.TryGetValue(new NSString ("UIKeyboardFrameEndUserInfoKey"), out frameSize);
			if (foundFrameSize == true && frameSize is not null)
			{
				var frameSizeRect = DescriptionToCGRect(frameSize.Description);
				if (frameSizeRect is not null)
					KeyboardFrame = (CGRect)frameSizeRect;
			}

			//var foundCurve = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationCurveUserInfoKey"), out curveSize);
			var foundAnimationDuration = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out curveSize);
			if (foundAnimationDuration == true && curveSize is not null)
			{
				var num = (NSNumber)NSNumber.FromObject(curveSize);
				AnimationDuration = (double)num;
			}

			if (!IsKeyboardShowing)
			{
				AdjustPostition();
				IsKeyboardShowing = true;
			}

		});

		DidHideToken = NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIKeyboardDidHideNotification"), (notification) =>
		{
			NSObject? curveSize = null;

			var foundAnimationDuration = notification.UserInfo?.TryGetValue(new NSString("UIKeyboardAnimationDurationUserInfoKey"), out curveSize);
			if (foundAnimationDuration == true && curveSize is not null)
			{
				var num = (NSNumber)NSNumber.FromObject(curveSize);
				AnimationDuration = (double)num;
			}



			if (LastScrollView is not null)
			{
				UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
				{

					if (LastScrollView.ContentInset != StartingContentInsets)
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					}

					// TODO Not implemented section
					//	if lastScrollView.shouldRestoreScrollViewContentOffset, !lastScrollView.contentOffset.equalTo(self.startingContentOffset) {
					//		self.showLog("Restoring contentOffset to: \(self.startingContentOffset)")


					//	let animatedContentOffset = self.textFieldView?.superviewOfClassType(UIStackView.self, belowView: lastScrollView) != nil  //  (Bug ID: #1365, #1508, #1541)

					//	if animatedContentOffset {
					//			lastScrollView.setContentOffset(self.startingContentOffset, animated: UIView.areAnimationsEnabled)

					//	}
					//		else
					//		{
					//			lastScrollView.contentOffset = self.startingContentOffset

					//	}






					//	var superScrollView: UIScrollView ? = lastScrollView


					//while let scrollView = superScrollView {

					//		let contentSize = CGSize(width: max(scrollView.contentSize.width, scrollView.frame.width), height: max(scrollView.contentSize.height, scrollView.frame.height))


					//	let minimumY = contentSize.height - scrollView.frame.height


					//	if minimumY < scrollView.contentOffset.y {

					//			let newContentOffset = CGPoint(x: scrollView.contentOffset.x, y: minimumY)

					//		if scrollView.contentOffset.equalTo(newContentOffset) == false {

					//				let animatedContentOffset = self.textFieldView?.superviewOfClassType(UIStackView.self, belowView: scrollView) != nil  //  (Bug ID: #1365, #1508, #1541)

					//			if animatedContentOffset {
					//					scrollView.setContentOffset(newContentOffset, animated: UIView.areAnimationsEnabled)

					//			}
					//				else
					//				{
					//					scrollView.contentOffset = newContentOffset

					//			}

					//				self.showLog("Restoring contentOffset to: \(self.startingContentOffset)")

					//		}
					//		}

					//		superScrollView = scrollView.superviewOfClassType(UIScrollView.self) as? UIScrollView

					//}

					var superScrollView = LastScrollView as UIScrollView;
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
								if (view?.Superview is UIStackView)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							}
						}

						superScrollView = superScrollView.Superview as UIScrollView;
					}



				}, () => { });
			}

			// TODO Restore is not working properly :(
			if (IsKeyboardShowing)
				RestorePosition();

			LastScrollView = null;
			KeyboardFrame = CGRect.Empty;
			StartingContentInsets = new UIEdgeInsets ();
			StartingScrollIndicatorInsets = new UIEdgeInsets ();
			StartingContentInsets = new UIEdgeInsets();

			IsKeyboardShowing = false;
		});
	}

	// used to get the numeric values from the UserInfo dictionary's NSObject value to CGRect
	static CGRect? DescriptionToCGRect(string description)
	{
		if (description is null)
			return null;

		string one, two, three, four;
		one = two = three = four = string.Empty;

		var sb = new StringBuilder();
		var isInNumber = false;
		foreach (var c in description)
		{
			if (char.IsDigit(c))
			{
				sb.Append(c);
				isInNumber = true;
			}

			else if (isInNumber && !char.IsDigit(c))
			{
				if (string.IsNullOrEmpty(one))
					one = sb.ToString();
				else if (string.IsNullOrEmpty(two))
					two = sb.ToString();
				else if (string.IsNullOrEmpty(three))
					three = sb.ToString();
				else if (string.IsNullOrEmpty(four))
					four = sb.ToString();
				else
					break;

				isInNumber = false;
				sb.Clear();
			}
		}

		if (int.TryParse(one, out var oneNum) && int.TryParse(two, out var twoNum)
			&& int.TryParse(three, out var threeNum) && int.TryParse(four, out var fourNum))
		{
			return new CGRect(oneNum, twoNum, threeNum, fourNum);
		}

		return null;
	}

	internal static void Destroy()
	{
		if (WillShowToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(WillShowToken);
		if (DidHideToken is not null)
			NSNotificationCenter.DefaultCenter.RemoveObserver(DidHideToken);
	}

	internal static void AdjustPostition()
	{
		if (view is not UITextField field && view is not UITextView)
			return;

		if (rootController is null)
			return;
		var rootFrame = rootController.Frame;
		var rootViewOrigin = new CGPoint(rootFrame.GetMinX(), rootFrame.GetMinY());
		var window = rootController.Window;


		////Maintain KeyboardDistanceFromTextField
		//var specialKeyboardDistanceFromTextField = textFieldView.KeyboardDistanceFromTextField


		//if let searchBar = textFieldView.textFieldSearchBar() {
		//	specialKeyboardDistanceFromTextField = searchBar.KeyboardDistanceFromTextField

		//}

		// 
		//let newKeyboardDistanceFromTextField = (specialKeyboardDistanceFromTextField == kIQUseDefaultKeyboardDistance) ? KeyboardDistanceFromTextField : specialKeyboardDistanceFromTextField


		// TODO Set the expected distance between the keyboard and the textfield depending if we are in a search bar or not
		var specialKeyboardDistanceFromTextField = view.GetTextFieldSearchBar() is null ?
			KeyboardDistanceFromTextField : SearchBarKeyboardDistanceFromTextField;


		//		var kbSize = KeyboardFrame.size


		//		do
		//		{
		//			var kbFrame = KeyboardFrame


		//			kbFrame.origin.y -= newKeyboardDistanceFromTextField

		//			kbFrame.size.height += newKeyboardDistanceFromTextField

		//			//Calculating actual keyboard covered size respect to window, keyboard frame may be different when hardware keyboard is attached (Bug ID: #469) (Bug ID: #381) (Bug ID: #1506)
		//			let intersectRect = kbFrame.intersection(window.frame)


		//			if intersectRect.isNull {
		//				kbSize = CGSize(width: kbFrame.size.width, height: 0)

		//			}
		//			else
		//			{
		//				kbSize = intersectRect.size

		//			}
		//		}

		// TODO Set up the keyboard size
		// https://learn.microsoft.com/en-us/dotnet/api/coregraphics.cgrect?view=xamarin-ios-sdk-12#properties
		var kbSize = KeyboardFrame.Size;
		var kbFrame = KeyboardFrame;
		kbFrame.Y -= specialKeyboardDistanceFromTextField;
		kbFrame.Height += specialKeyboardDistanceFromTextField;
		var intersectRect = CGRect.Intersect(kbFrame, window.Frame);
		if (intersectRect == CGRect.Empty)
			kbSize = new CGSize(kbFrame.Width, 0);
		else
			kbSize = intersectRect.Size;





		//		let statusBarHeight:
		//		CGFloat

		//		let navigationBarAreaHeight:
		//		CGFloat

		//		if let navigationController = rootController.navigationController {
		//			navigationBarAreaHeight = navigationController.navigationBar.frame.maxY

		//		} else
		//		{
		//#if swift(>=5.1)
		//            if #available(iOS 13, *) {
		//                statusBarHeight = window.windowScene?.statusBarManager?.statusBarFrame.height ?? 0
		//            } else {
		//                statusBarHeight = UIApplication.shared.statusBarFrame.height
		//            }
		//#else
		//			statusBarHeight = UIApplication.shared.statusBarFrame.height
		//#endif
		//			navigationBarAreaHeight = statusBarHeight

		//		}

		// TODO Set the StatusBarHeight and NavigationBarAreaHeight

		nfloat statusBarHeight;
		nfloat navigationBarAreaHeight;

		if (rootController.GetNavigationController() is UINavigationController navigationController)
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





		//		let layoutAreaHeight: CGFloat = rootController.view.layoutMargins.bottom


		//		let isTextView: Bool
		//		let isNonScrollableTextView:
		//		Bool


		//		if let textView = textFieldView as? UIScrollView, textFieldView.responds(to: #selector(getter: UITextView.isEditable)) {

		//            isTextView = true

		//			isNonScrollableTextView = !textView.isScrollEnabled

		//		} else {
		//            isTextView = false
		//            isNonScrollableTextView = false
		//        }


		//TODO Set up isTextView and isNonScrollableTextView

		var layoutAreaHeight = rootController.LayoutMargins.Bottom;
		var isTextView = false;
		var isNonScrollableTextView = false;

		if (view is UIScrollView scrollView)
		{
			isTextView = true;
			isNonScrollableTextView = !scrollView.ScrollEnabled;
		}


		//let topLayoutGuide: CGFloat = max(navigationBarAreaHeight, layoutAreaHeight) + 5

		//        let bottomLayoutGuide: CGFloat = (isTextView && !isNonScrollableTextView) ? 0 : rootController.view.layoutMargins.bottom  //Validation of textView for case where there is a tab bar at the bottom or running on iPhone X and textView is at the bottom.
		//		let visibleHeight: CGFloat = window.frame.height-kbSize.height

		//		//  Move positive = textField is hidden.
		//		//  Move negative = textField is showing.
		//		//  Calculating move position.
		//		var move: CGFloat

		//        //Special case: when the textView is not scrollable, then we'll be scrolling to the bottom part and let hide the top part above
		//        if isNonScrollableTextView {
		//            move = textFieldViewRectInWindow.maxY - visibleHeight + bottomLayoutGuide
		//        } else
		//{
		//	move = min(textFieldViewRectInRootSuperview.minY - (topLayoutGuide), textFieldViewRectInWindow.maxY - visibleHeight + bottomLayoutGuide)

		//		}

		//showLog("Need to move: \(move)")

		// TODO Figure out how much things need to move

		var topLayoutGuide = Math.Max(navigationBarAreaHeight, layoutAreaHeight) + 5;
		var bottomLayoutGuide = (isTextView && !isNonScrollableTextView) ? 0 : rootController.LayoutMargins.Bottom;
		var visibleHeight = window.Frame.Height - kbSize.Height;

		var viewRectInWindowMaxY = view.ConvertRectToView(view.Bounds, window).GetMaxY();
		var viewRectInRootSuperviewMinY = view.ConvertRectToView(view.Bounds, rootController.Superview).GetMinY();
		nfloat move;

		if (isNonScrollableTextView)
			move = viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide;
		else
			move = (nfloat)Math.Min(viewRectInRootSuperviewMinY - topLayoutGuide, viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide);


		//		var superScrollView: UIScrollView?
		//		var superView = textFieldView.superviewOfClassType(UIScrollView.self) as? UIScrollView

		//		//Getting UIScrollView whose scrolling is enabled.    //  (Bug ID: #285)
		//		while let view = superView {

		//	if view.isScrollEnabled, !view.ShouldIgnoreScrollingAdjustment {
		//		superScrollView = view

		//				break

		//			} else
		//	{
		//		//  Getting it's superScrollView.   //  (Enhancement ID: #21, #24)
		//		superView = view.superviewOfClassType(UIScrollView.self) as? UIScrollView

		//			}
		//}

		// TODO Get information about the TextView ScrollView

		UIScrollView? superScrollView = null;
		var superView = view.Superview as UIScrollView;
		while (superView is not null)
		{
			if (superView.ScrollEnabled && ShouldIgnoreScrollingAdjustment)
			{
				superScrollView = superView;
				break;
			}

			superView = superView.Superview as UIScrollView;
		}



		////If there was a LastScrollView.    //  (Bug ID: #34)
		//if let LastScrollView = LastScrollView {
		//	//If we can't find current superScrollView, then setting LastScrollView to it's original form.
		//	if superScrollView == nil {

		//		if LastScrollView.contentInset != self.StartingContentInsets {
		//			showLog("Restoring contentInset to: \(StartingContentInsets)")

		//					UIView.animate(withDuration: AnimationDuration, delay: 0, options: AnimationCurve, animations: {
		//				()->Void in

		//                        LastScrollView.contentInset = self.StartingContentInsets

		//						LastScrollView.scrollIndicatorInsets = self.StartingScrollIndicatorInsets

		//					})
		//                }

		//	TODO if LastScrollView.ShouldRestoreScrollViewContentOffset, !LastScrollView.contentOffset.equalTo(StartingContentOffset) {
		//			showLog("Restoring contentOffset to: \(StartingContentOffset)")


		//					let animatedContentOffset = textFieldView.superviewOfClassType(UIStackView.self, belowView: LastScrollView) != nil  //  (Bug ID: #1365, #1508, #1541)

		//					if animatedContentOffset {
		//				LastScrollView.setContentOffset(StartingContentOffset, animated: UIView.areAnimationsEnabled)

		//					}
		//			else
		//			{
		//				LastScrollView.contentOffset = StartingContentOffset

		//					}
		//		}
		//		StartingContentInsets = UIEdgeInsets()

		//				StartingScrollIndicatorInsets = UIEdgeInsets()

		//				StartingContentOffset = CGPoint.zero

		//				self.LastScrollView = nil

		//			}


		// TODO

		if (LastScrollView is not null)
		{
			if (superScrollView is null)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					}, () => { });
				}

				if (ShouldRestoreScrollViewContentOffset && !LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (view.Superview is UIStackView)
						LastScrollView.SetContentOffset(StartingContentOffset, UIView.AnimationsEnabled);
					else
						LastScrollView.ContentOffset = StartingContentOffset;
				}

				StartingContentInsets = uIEdgeInsets;
				StartingScrollIndicatorInsets = uIEdgeInsets;
				StartingContentOffset = new CGPoint(0,0); // TODO is this the same as CGPoint.Zero?
				LastScrollView = null;

			}





			//	else if superScrollView != LastScrollView {     //If both scrollView's are different, then reset LastScrollView to it's original frame and setting current scrollView as last scrollView.
			//		if LastScrollView.contentInset != self.StartingContentInsets {
			//			showLog("Restoring contentInset to: \(StartingContentInsets)")

			//					UIView.animate(withDuration: AnimationDuration, delay: 0, options: AnimationCurve, animations: {
			//				()->Void in

			//                        LastScrollView.contentInset = self.StartingContentInsets

			//						LastScrollView.scrollIndicatorInsets = self.StartingScrollIndicatorInsets

			//					})
			//                }

			//		if LastScrollView.ShouldRestoreScrollViewContentOffset, !LastScrollView.contentOffset.equalTo(StartingContentOffset) {
			//			showLog("Restoring contentOffset to: \(StartingContentOffset)")


			//					let animatedContentOffset = textFieldView.superviewOfClassType(UIStackView.self, belowView: LastScrollView) != nil  //  (Bug ID: #1365, #1508, #1541)

			//					if animatedContentOffset {
			//				LastScrollView.setContentOffset(StartingContentOffset, animated: UIView.areAnimationsEnabled)

			//					}
			//			else
			//			{
			//				LastScrollView.contentOffset = StartingContentOffset

			//					}
			//		}

			//		self.LastScrollView = superScrollView

			//				if let scrollView = superScrollView {
			//			StartingContentInsets = scrollView.contentInset

			//					StartingContentOffset = scrollView.contentOffset

			//					#if swift(>=5.1)
			//                    if #available(iOS 11.1, *) {
			//                        StartingScrollIndicatorInsets = scrollView.verticalScrollIndicatorInsets
			//                    } else {
			//                        StartingScrollIndicatorInsets = scrollView.scrollIndicatorInsets
			//                    }
			//#else
			//					StartingScrollIndicatorInsets = scrollView.scrollIndicatorInsets
			//					#endif
			//				}

			//		showLog("Saving ScrollView New contentInset: \(StartingContentInsets) and contentOffset: \(StartingContentOffset)")

			//			}

			else if (superScrollView != LastScrollView)
			{
				if (LastScrollView.ContentInset != StartingContentInsets)
				{
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						LastScrollView.ContentInset = StartingContentInsets;
						LastScrollView.ScrollIndicatorInsets = StartingScrollIndicatorInsets;
					}, () => { });
				}

				if (ShouldRestoreScrollViewContentOffset && !LastScrollView.ContentOffset.Equals(StartingContentOffset))
				{
					if (view.Superview is UIStackView)
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


		} // (LastScrollView is not null)

		//	//Else the case where superScrollView == LastScrollView means we are on same scrollView after switching to different textField. So doing nothing, going ahead
		//} else if let unwrappedSuperScrollView = superScrollView {    //If there was no LastScrollView and we found a current scrollView. then setting it as LastScrollView.
		//	LastScrollView = unwrappedSuperScrollView

		//			StartingContentInsets = unwrappedSuperScrollView.contentInset

		//			StartingContentOffset = unwrappedSuperScrollView.contentOffset

		//			#if swift(>=5.1)
		//            if #available(iOS 11.1, *) {
		//                StartingScrollIndicatorInsets = unwrappedSuperScrollView.verticalScrollIndicatorInsets
		//            } else {
		//                StartingScrollIndicatorInsets = unwrappedSuperScrollView.scrollIndicatorInsets
		//            }
		//#else
		//			StartingScrollIndicatorInsets = unwrappedSuperScrollView.scrollIndicatorInsets
		//			#endif

		//			showLog("Saving ScrollView contentInset: \(StartingContentInsets) and contentOffset: \(StartingContentOffset)")

		//		}

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























		////  Special case for ScrollView.
		////  If we found LastScrollView then setting it's contentOffset to show textField.
		//if let LastScrollView = LastScrollView {
		//	//Saving
		//	var lastView = textFieldView

		//			var superScrollView = self.LastScrollView


		//			while let scrollView = superScrollView {

		//		var shouldContinue = false


		//				if move > 0 {
		//			shouldContinue = move > (-scrollView.contentOffset.y - scrollView.contentInset.top)


		//				}
		//		else if let tableView = scrollView.superviewOfClassType(UITableView.self) as? UITableView {

		//			shouldContinue = scrollView.contentOffset.y > 0


		//					if shouldContinue, let tableCell = textFieldView.superviewOfClassType(UITableViewCell.self) as? UITableViewCell, let indexPath = tableView.indexPath(for: tableCell), let previousIndexPath = tableView.previousIndexPath(of: indexPath) {

		//				let previousCellRect = tableView.rectForRow(at: previousIndexPath)

		//						if !previousCellRect.isEmpty {
		//					let previousCellRectInRootSuperview = tableView.convert(previousCellRect, to: rootController.view.superview)


		//							move = min(0, previousCellRectInRootSuperview.maxY - topLayoutGuide)

		//						}
		//			}


		if (LastScrollView is not null)
		{
			var lastView = view;
			superScrollView = LastScrollView;

			while (superScrollView is not null)
			{
				var shouldContinue = false;

				if (move > 0) {
					shouldContinue = MovedDistance > (-superScrollView.ContentOffset.Y - superScrollView.ContentInset.Top);
				}

				else if (superScrollView.Superview is UITableView tableView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && view.Superview is UITableViewCell tableCell
						&& tableView.IndexPathForCell(tableCell) is NSIndexPath indexPath
						&& tableView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath)
					{
						var previousCellRect = tableView.RectForRowAtIndexPath(previousIndexPath);
						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = tableView.ConvertRectToView(previousCellRect, rootController.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}
				}


				//		} else if let collectionView = scrollView.superviewOfClassType(UICollectionView.self) as? UICollectionView {

				//			shouldContinue = scrollView.contentOffset.y > 0


				//					if shouldContinue, let collectionCell = textFieldView.superviewOfClassType(UICollectionViewCell.self) as? UICollectionViewCell, let indexPath = collectionView.indexPath(for: collectionCell), let previousIndexPath = collectionView.previousIndexPath(of: indexPath), let attributes = collectionView.layoutAttributesForItem(at: previousIndexPath) {

				//				let previousCellRect = attributes.frame

				//						if !previousCellRect.isEmpty {
				//					let previousCellRectInRootSuperview = collectionView.convert(previousCellRect, to: rootController.view.superview)


				//							move = min(0, previousCellRectInRootSuperview.maxY - topLayoutGuide)

				//						}
				//			}


				else if (superScrollView.Superview is UICollectionView collectionView)
				{
					shouldContinue = superScrollView.ContentOffset.Y > 0;

					if (shouldContinue && view.Superview is UICollectionViewCell collectionCell
						&& collectionView.IndexPathForCell(collectionCell) is NSIndexPath indexPath
						&& collectionView.GetPreviousIndexPath(indexPath) is NSIndexPath previousIndexPath
						&&  collectionView.GetLayoutAttributesForItem(previousIndexPath) is UICollectionViewLayoutAttributes attributes)
					{
						var previousCellRect = attributes.Frame;

						if (!previousCellRect.IsEmpty)
						{
							var previousCellRectInRootSuperview = collectionView.ConvertRectToView(previousCellRect, rootController.Superview);
							move = (nfloat)Math.Min(0, previousCellRectInRootSuperview.GetMaxY() - topLayoutGuide);
						}
					}
				}

				//		} else
				//		{

				//			if isNonScrollableTextView {
				//				shouldContinue = textFieldViewRectInWindow.maxY < visibleHeight + bottomLayoutGuide


				//						if shouldContinue {
				//					move = min(0, textFieldViewRectInWindow.maxY - visibleHeight + bottomLayoutGuide)

				//						}
				//			}
				//			else
				//			{
				//				shouldContinue = textFieldViewRectInRootSuperview.minY < topLayoutGuide


				//						if shouldContinue {
				//					move = min(0, textFieldViewRectInRootSuperview.minY - topLayoutGuide)

				//						}
				//			}
				//		}

				else
				{
					if (isNonScrollableTextView)
					{
						shouldContinue = viewRectInWindowMaxY < visibleHeight + bottomLayoutGuide;

						if (shouldContinue)
							move = (nfloat)Math.Min(0, viewRectInWindowMaxY - visibleHeight + bottomLayoutGuide);
					}
					else
					{
						shouldContinue = viewRectInRootSuperviewMinY < topLayoutGuide;

						if (shouldContinue)
							move = (nfloat)Math.Min(0, viewRectInRootSuperviewMinY - topLayoutGuide);
					}
				}


				//		//Looping in upper hierarchy until we don't found any scrollView in it's upper hirarchy till UIWindow object.
				//		if shouldContinue {

				//			var tempScrollView = scrollView.superviewOfClassType(UIScrollView.self) as? UIScrollView

				//			var nextScrollView:
				//			UIScrollView ?

				//					while let view = tempScrollView {

				//				if view.isScrollEnabled, !view.ShouldIgnoreScrollingAdjustment {
				//					nextScrollView = view

				//							break

				//						} else
				//				{
				//					tempScrollView = view.superviewOfClassType(UIScrollView.self) as? UIScrollView

				//						}
				//			}

				if (shouldContinue)
				{
					var tempScrollView = superScrollView.Superview as UIScrollView;
					UIScrollView? nextScrollView = null;

					while (tempScrollView is not null)
					{
						if (tempScrollView.ScrollEnabled && ShouldIgnoreScrollingAdjustment)
						{
							nextScrollView = tempScrollView;
							break;
						}
						tempScrollView = tempScrollView.Superview as UIScrollView;
					}





					//			//Getting lastViewRect.
					//			if let lastViewRect = lastView.superview?.convert(lastView.frame, to: scrollView) {

					//				//Calculating the expected Y offset from move and scrollView's contentOffset.
					//				var shouldOffsetY = scrollView.contentOffset.y - min(scrollView.contentOffset.y, -move)

					//						//Rearranging the expected Y offset according to the view.
					//						if isNonScrollableTextView {
					//					shouldOffsetY = min(shouldOffsetY, lastViewRect.maxY - visibleHeight + bottomLayoutGuide)

					//						}
					//				else
					//				{
					//					shouldOffsetY = min(shouldOffsetY, lastViewRect.minY)

					//						}

					//				//[_textFieldView isKindOfClass:[UITextView class]] If is a UITextView type
					//				//nextScrollView == nil    If processing scrollView is last scrollView in upper hierarchy (there is no other scrollView upper hierrchy.)
					//				//[_textFieldView isKindOfClass:[UITextView class]] If is a UITextView type
					//				//shouldOffsetY >= 0     shouldOffsetY must be greater than in order to keep distance from navigationBar (Bug ID: #92)
					//				if isTextView, !isNonScrollableTextView,
					//                            nextScrollView == nil,
					//                            shouldOffsetY >= 0 {

					//					//  Converting Rectangle according to window bounds.
					//					if let currentTextFieldViewRect = textFieldView.superview?.convert(textFieldView.frame, to: window) {

					//						//Calculating expected fix distance which needs to be managed from navigation bar
					//						let expectedFixDistance: CGFloat = currentTextFieldViewRect.minY - topLayoutGuide

					//								//Now if expectedOffsetY (superScrollView.contentOffset.y + expectedFixDistance) is lower than current shouldOffsetY, which means we're in a position where navigationBar up and hide, then reducing shouldOffsetY with expectedOffsetY (superScrollView.contentOffset.y + expectedFixDistance)
					//								shouldOffsetY = min(shouldOffsetY, scrollView.contentOffset.y + expectedFixDistance)

					//								//Setting move to 0 because now we don't want to move any view anymore (All will be managed by our contentInset logic.
					//								move = 0

					//							} else
					//					{
					//						//Subtracting the Y offset from the move variable, because we are going to change scrollView's contentOffset.y to shouldOffsetY.
					//						move -= (shouldOffsetY - scrollView.contentOffset.y)

					//							}

					if (lastView.Superview.ConvertRectToView(lastView.Frame, superScrollView) is CGRect lastViewRect)
					{
						var shouldOffsetY = superScrollView.ContentOffset.Y - Math.Min(superScrollView.ContentOffset.Y, -move);

						if (isNonScrollableTextView)
							shouldOffsetY = Math.Min(shouldOffsetY, lastViewRect.GetMaxY() - visibleHeight + bottomLayoutGuide);
						else
							shouldOffsetY = Math.Min(shouldOffsetY, lastViewRect.GetMinY());

						if (isTextView && !isNonScrollableTextView && nextScrollView is null && shouldOffsetY >= 0)
						{
							if (view.Superview.ConvertRectToView(view.Frame, window) is CGRect currentTextFieldViewRect)
							{
								var expectedFixDistance = currentTextFieldViewRect.GetMinY() - topLayoutGuide;

								shouldOffsetY = Math.Min(shouldOffsetY, superScrollView.ContentOffset.Y + expectedFixDistance);
								move = 0;
							}
							else
							{
								move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);
							}
						}

						else
						{
							move -= (nfloat)(shouldOffsetY - superScrollView.ContentOffset.Y);
						}



						//				} else
						//				{
						//					//Subtracting the Y offset from the move variable, because we are going to change scrollView's contentOffset.y to shouldOffsetY.
						//					move -= (shouldOffsetY - scrollView.contentOffset.y)

						//						}

						//				let newContentOffset = CGPoint(x: scrollView.contentOffset.x, y: shouldOffsetY)


						//						if scrollView.contentOffset.equalTo(newContentOffset) == false {

						//					showLog("old contentOffset: \(scrollView.contentOffset) new contentOffset: \(newContentOffset)")

						//							self.showLog("Remaining Move: \(move)")

						//							//Getting problem while using `setContentOffset:animated:`, So I used animation API.
						//							UIView.animate(withDuration: AnimationDuration, delay: 0, options: AnimationCurve, animations: {
						//						()->Void in

						//                                let animatedContentOffset = textFieldView.superviewOfClassType(UIStackView.self, belowView: scrollView) != nil  //  (Bug ID: #1365, #1508, #1541)

						//								if animatedContentOffset {
						//							scrollView.setContentOffset(newContentOffset, animated: UIView.areAnimationsEnabled)

						//								}
						//						else
						//						{
						//							scrollView.contentOffset = newContentOffset

						//								}
						//					}, completion:
						//					{
						//						_ in

						//                                if scrollView is UITableView || scrollView is UICollectionView
						//									{
						//										//This will update the next/previous states
						//										self.addToolbarIfRequired()
						//									}

						//							})
						//                        }
						//			}

						//			//  Getting next lastView & superScrollView.
						//			lastView = scrollView

						//					superScrollView = nextScrollView

						//				}
						//		else
						//		{
						//			move = 0

						//					break

						//				}
						//	}

						var newContentOffset = new CGPoint(superScrollView.ContentOffset.X, shouldOffsetY);

						if (!superScrollView.ContentOffset.Equals(newContentOffset))
						{
							UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
							{
								if (view.Superview is UIStackView)
									superScrollView.SetContentOffset(newContentOffset, UIView.AnimationsEnabled);
								else
									superScrollView.ContentOffset = newContentOffset;
							}, () =>
							{
								if (superScrollView is UITableView || superScrollView is UICollectionView)
								{
									// TODO add the method: AddToolbarIfRequired();
								}
							});
						}
					}
					lastView = superScrollView;
					superScrollView = nextScrollView;
				} // (shouldContinue)

				else
				{
					move = 0;
					break;
				}
			} // (superScrollView is not null)





			//	//Updating contentInset
			//	if let lastScrollViewRect = LastScrollView.superview?.convert(LastScrollView.frame, to: window),
			//                LastScrollView.ShouldIgnoreContentInsetAdjustment == false {

			//		var bottomInset: CGFloat = (kbSize.height) - (window.frame.height - lastScrollViewRect.maxY)

			//				var bottomScrollIndicatorInset = bottomInset - newKeyboardDistanceFromTextField

			//				// Update the insets so that the scroll vew doesn't shift incorrectly when the offset is near the bottom of the scroll view.
			//				bottomInset = max(StartingContentInsets.bottom, bottomInset)

			//				bottomScrollIndicatorInset = max(StartingScrollIndicatorInsets.bottom, bottomScrollIndicatorInset)


			//				if #available(iOS 11, *) {
			//                    bottomInset -= LastScrollView.safeAreaInsets.bottom

			//					bottomScrollIndicatorInset -= LastScrollView.safeAreaInsets.bottom

			//				}

			//	var movedInsets = LastScrollView.contentInset

			//				movedInsets.bottom = bottomInset


			//				if LastScrollView.contentInset != movedInsets {
			//		showLog("old ContentInset: \(LastScrollView.contentInset) new ContentInset: \(movedInsets)")


			//					UIView.animate(withDuration: AnimationDuration, delay: 0, options: AnimationCurve, animations: {
			//			()->Void in
			//                        LastScrollView.contentInset = movedInsets


			//						var newScrollIndicatorInset: UIEdgeInsets

			//#if swift(>=5.1)
			//                        if #available(iOS 11.1, *) {
			//                            newScrollIndicatorInset = LastScrollView.verticalScrollIndicatorInsets
			//                        } else {
			//                            newScrollIndicatorInset = LastScrollView.scrollIndicatorInsets
			//                        }
			//#else
			//						newScrollIndicatorInset = LastScrollView.scrollIndicatorInsets
			//						#endif

			//						newScrollIndicatorInset.bottom = bottomScrollIndicatorInset

			//						LastScrollView.scrollIndicatorInsets = newScrollIndicatorInset

			//					})
			//                }
			//}
			//        }

			if (LastScrollView.Superview.ConvertRectToView(LastScrollView.Frame, window) is CGRect lastScrollViewRect
				&& !ShouldIgnoreContentInsetAdjustment)
			{
				var bottomInset = kbSize.Height - window.Frame.Height - lastScrollViewRect.GetMaxY();
				var bottomScrollIndicatorInset = bottomInset - specialKeyboardDistanceFromTextField;

				bottomInset = (nfloat)Math.Max(StartingContentInsets.Bottom, bottomInset);
				bottomScrollIndicatorInset = (nfloat)Math.Max(StartingScrollIndicatorInsets.Bottom, bottomScrollIndicatorInset);

				if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
				{
					bottomInset -= LastScrollView.SafeAreaInsets.Bottom;
					bottomScrollIndicatorInset -= LastScrollView.SafeAreaInsets.Bottom;
				}

				var movedInsets = LastScrollView.ContentInset;
				movedInsets.Bottom = bottomInset;

				if (LastScrollView.ContentInset != movedInsets)
				{
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						LastScrollView.ContentInset = movedInsets;
						UIEdgeInsets newScrollIndicatorInsets;

						if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
							newScrollIndicatorInsets = LastScrollView.VerticalScrollIndicatorInsets;
						else
							newScrollIndicatorInsets = LastScrollView.ScrollIndicatorInsets;

						newScrollIndicatorInsets.Bottom = bottomScrollIndicatorInset;
						LastScrollView.ScrollIndicatorInsets = newScrollIndicatorInsets;
					}, () => { });
				}
			} // (LastScrollView.Superview.ConvertRectToView(LastScrollView.Frame, window) is CGRect lastScrollViewRect)
		} // (LastScrollView is not null)






		//		//Special case for UITextView(Readjusting textView.contentInset when textView hight is too big to fit on screen)
		//		//_lastScrollView       If not having inside any scrollView, (now contentInset manages the full screen textView.
		//		//[_textFieldView isKindOfClass:[UITextView class]] If is a UITextView type
		//		if let textView = textFieldView as? UIScrollView, textView.isScrollEnabled, textFieldView.responds(to: #selector(getter: UITextView.isEditable)) {

		//            //                CGRect rootSuperViewFrameInWindow = [_rootViewController.view.superview convertRect:_rootViewController.view.superview.bounds toView:keyWindow];
		//            //
		//            //                CGFloat keyboardOverlapping = CGRectGetMaxY(rootSuperViewFrameInWindow) - keyboardYPosition;
		//            //
		//            //                CGFloat textViewHeight = MIN(CGRectGetHeight(_textFieldView.frame), (CGRectGetHeight(rootSuperViewFrameInWindow)-topLayoutGuide-keyboardOverlapping));
		//            let keyboardYPosition = window.frame.height - (kbSize.height - newKeyboardDistanceFromTextField)

		//			var rootSuperViewFrameInWindow = window.frame

		//			if let rootSuperview = rootController.view.superview {
		//			rootSuperViewFrameInWindow = rootSuperview.convert(rootSuperview.bounds, to: window)

		//			}

		//		let keyboardOverlapping = rootSuperViewFrameInWindow.maxY - keyboardYPosition


		//			let textViewHeight = min(textView.frame.height, rootSuperViewFrameInWindow.height - topLayoutGuide - keyboardOverlapping)


		//			if textView.frame.size.height - textView.contentInset.bottom > textViewHeight {
		//			//_isTextViewContentInsetChanged,  If frame is not change by library in past, then saving user textView properties  (Bug ID: #92)
		//			if !self.IsTextViewContentInsetChanged {
		//				self.StartingTextViewContentInsets = textView.contentInset

		//					#if swift(>=5.1)
		//                    if #available(iOS 11.1, *) {
		//                        self.StartingTextViewScrollIndicatorInsets = textView.verticalScrollIndicatorInsets
		//                    } else {
		//                        self.StartingTextViewScrollIndicatorInsets = textView.scrollIndicatorInsets
		//                    }
		//#else
		//					self.StartingTextViewScrollIndicatorInsets = textView.scrollIndicatorInsets
		//					#endif
		//				}

		//			self.IsTextViewContentInsetChanged = true


		//				var newContentInset = textView.contentInset

		//				newContentInset.bottom = textView.frame.size.height - textViewHeight


		//				if #available(iOS 11, *) {
		//                    newContentInset.bottom -= textView.safeAreaInsets.bottom

		//				}

		//		if textView.contentInset != newContentInset {
		//			self.showLog("\(textFieldView) Old UITextView.contentInset: \(textView.contentInset) New UITextView.contentInset: \(newContentInset)")


		//					UIView.animate(withDuration: AnimationDuration, delay: 0, options: AnimationCurve, animations: {
		//				()->Void in

		//                        textView.contentInset = newContentInset

		//						textView.scrollIndicatorInsets = newContentInset

		//					}, completion:
		//			{ (_)->Void in })
		//                }
		//	}
		//}

		if (view is UIScrollView textView && textView.ScrollEnabled && view is UITextView tview && tview.Editable)
		{
			var keyboardYPosition = window.Frame.Height - kbSize.Height - specialKeyboardDistanceFromTextField;
			var rootSuperViewFrameInWindow = window.Frame;
			if (rootController.Superview is UIView v)
				rootSuperViewFrameInWindow = v.ConvertRectToView(v.Bounds, window);

			var keyboardOverlapping = rootSuperViewFrameInWindow.GetMaxY() - keyboardYPosition;

			var textViewHeight = Math.Min(textView.Frame.Height, rootSuperViewFrameInWindow.Height - topLayoutGuide - keyboardOverlapping);

			if (textView.Frame.Size.Height - textView.ContentInset.Bottom > textViewHeight)
			{
				if (!IsTextViewContentInsetChanged)
				{
					StartingTextViewContentInsets = textView.ContentInset;
					if (OperatingSystem.IsIOSVersionAtLeast(11, 1))
						StartingTextViewScrollIndicatorInsets = textView.VerticalScrollIndicatorInsets;
					else
						StartingTextViewScrollIndicatorInsets = textView.ScrollIndicatorInsets;
				}

				IsTextViewContentInsetChanged = true;

				var newContentInset = textView.ContentInset;
				newContentInset.Bottom = (nfloat)(textView.Frame.Size.Height - textViewHeight);

				if (OperatingSystem.IsIOSVersionAtLeast(11, 0))
					newContentInset.Bottom -= textView.SafeAreaInsets.Bottom;

				if (textView.ContentInset != newContentInset)
				{
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						textView.ContentInset = newContentInset;
						textView.ScrollIndicatorInsets = newContentInset;
					}, () => { });
				}
			} // (textView.Frame.Size.Height - textView.ContentInset.Bottom > textViewHeight)
		}



		//        //  +Positive or zero.
		//        if move >= 0 {

		//	rootViewOrigin.y = max(rootViewOrigin.y - move, min(0, -(kbSize.height - newKeyboardDistanceFromTextField)))


		//			if rootController.view.frame.origin.equalTo(rootViewOrigin) == false {
		//		showLog("Moving Upward")


		//				UIView.animate(withDuration: AnimationDuration, delay: 0, options: AnimationCurve, animations: {
		//			()->Void in

		//                    var rect = rootController.view.frame

		//					rect.origin = rootViewOrigin

		//					rootController.view.frame = rect

		//					//Animating content if needed (Bug ID: #204)
		//					if self.layoutIfNeededOnUpdate {
		//				//Animating content (Bug ID: #160)
		//				rootController.view.setNeedsLayout()

		//						rootController.view.layoutIfNeeded()

		//					}

		//			self.showLog("Set \(rootController) origin to: \(rootViewOrigin)")

		//				})
		//            }

		//	MovedDistance = (TopViewBeginOrigin.y - rootViewOrigin.y)

		//		}

		if (move >= 0)
		{
			rootViewOrigin.Y = (nfloat)Math.Max(rootViewOrigin.Y - move, Math.Min(0, -(kbSize.Height - specialKeyboardDistanceFromTextField)));

			if (rootController.Frame.X != rootViewOrigin.X || rootController.Frame.Y != rootViewOrigin.Y)
			{
				UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
				{
					var rect = rootController.Frame;
					rect.X = rootViewOrigin.X;
					rect.Y = rootViewOrigin.Y;

					rootController.Frame = rect;

					if (LayoutifNeededOnUpdate)
					{
						rootController.SetNeedsLayout();
						rootController.LayoutIfNeeded();

					}
				}, () => { });
			}

			MovedDistance = (TopViewBeginOrigin.Y - rootViewOrigin.Y);
		} // (move >= 0)



		//else
		//{  //  -Negative
		//	let disturbDistance: CGFloat = rootViewOrigin.y - TopViewBeginOrigin.y

		//			//  disturbDistance Negative = frame disturbed.
		//			//  disturbDistance positive = frame not disturbed.
		//			if disturbDistance <= 0 {

		//		rootViewOrigin.y -= max(move, disturbDistance)


		//				if rootController.view.frame.origin.equalTo(rootViewOrigin) == false {
		//			showLog("Moving Downward")
		//					//  Setting adjusted rootViewRect
		//					//  Setting adjusted rootViewRect
		//					UIView.animate(withDuration: AnimationDuration, delay: 0, options: AnimationCurve, animations: {
		//				()->Void in

		//                        var rect = rootController.view.frame

		//						rect.origin = rootViewOrigin

		//						rootController.view.frame = rect

		//						//Animating content if needed (Bug ID: #204)
		//						if self.layoutIfNeededOnUpdate {
		//					//Animating content (Bug ID: #160)
		//					rootController.view.setNeedsLayout()

		//							rootController.view.layoutIfNeeded()

		//						}

		//				self.showLog("Set \(rootController) origin to: \(rootViewOrigin)")

		//					})
		//                }

		//		MovedDistance = (TopViewBeginOrigin.y - rootViewOrigin.y)

		//			}
		//}

		//let elapsedTime = CACurrentMediaTime() - startTime
		//        showLog("<<<<< \(#function) ended: \(elapsedTime) seconds <<<<<", indentation: -1)
		//    }

		else
		{
			var disturbDistance = rootViewOrigin.Y - TopViewBeginOrigin.Y;

			if (disturbDistance <= 0)
			{
				rootViewOrigin.Y -= (nfloat)Math.Max(move, disturbDistance);

				if (rootController.Frame.X != rootViewOrigin.X || rootController.Frame.Y != rootViewOrigin.Y)
				{
					UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
					{
						var rect = rootController.Frame;
						rect.X = rootViewOrigin.X;
						rect.Y = rootViewOrigin.Y;

						rootController.Frame = rect;

						if (LayoutifNeededOnUpdate)
						{
							rootController.SetNeedsLayout();
							rootController.LayoutIfNeeded();

						}
					}, () => { });
				}
				MovedDistance = TopViewBeginOrigin.Y - rootViewOrigin.Y;
			}
		}
	}



	//internal func restorePosition()
	//{

	//	hasPendingAdjustRequest = false

	//	//  Setting rootViewController frame to it's original position. //  (Bug ID: #18)
	//	guard topViewBeginOrigin.equalTo(IQKeyboardManager.kIQCGPointInvalid) == false, let rootViewController = rootViewController else
	//	{
	//		return

	//	}

	//	if rootViewController.view.frame.origin.equalTo(self.topViewBeginOrigin) == false {
	//		//Used UIViewAnimationOptionBeginFromCurrentState to minimize strange animations.
	//		UIView.animate(withDuration: animationDuration, delay: 0, options: animationCurve, animations: {
	//			()->Void in

	//               self.showLog("Restoring \(rootViewController) origin to: \(self.topViewBeginOrigin)")

	//			//  Setting it's new frame
	//			var rect = rootViewController.view.frame

	//			rect.origin = self.topViewBeginOrigin

	//			rootViewController.view.frame = rect

	//			//Animating content if needed (Bug ID: #204)
	//			if self.layoutIfNeededOnUpdate {
	//				//Animating content (Bug ID: #160)
	//				rootViewController.view.setNeedsLayout()

	//				rootViewController.view.layoutIfNeeded()

	//			}
	//		})
	//       }

	//	self.movedDistance = 0


	//	if rootViewController.navigationController?.interactivePopGestureRecognizer?.state == .began {
	//		self.rootViewControllerWhilePopGestureRecognizerActive = rootViewController

	//		self.topViewBeginOriginWhilePopGestureRecognizerActive = self.topViewBeginOrigin

	//	}

	//	self.rootViewController = nil

	//}

	static void RestorePosition()
	{
		HasPendingAdjustRequest = false;

		if (rootController is not null && (rootController.Frame.X != TopViewBeginOrigin.X || rootController.Frame.Y != TopViewBeginOrigin.Y))
		{
			UIView.Animate(AnimationDuration, 0, AnimationCurve, () =>
			{
				var rect = rootController.Frame;
				rect.X = TopViewBeginOrigin.X;
				rect.Y = TopViewBeginOrigin.Y;

				rootController.Frame = rect;

				if (LayoutifNeededOnUpdate)
				{
					rootController.SetNeedsLayout();
					rootController.LayoutIfNeeded();

				}
			}, () => { });
		}

		MovedDistance = 0;

		rootController = null;
	}


	static NSIndexPath? GetPreviousIndexPath (this UITableView tableView, NSIndexPath indexPath)
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

	// Add toolbar if it is required to add on textFields and it's siblings.
	//internal func addToolbarIfRequired()
	//{

	//	//Either there is no inputAccessoryView or if accessoryView is not appropriate for current situation(There is Previous/Next/Done toolbar).
	//	guard let siblings = responderViews(), !siblings.isEmpty,
 //             let textField = textFieldView, textField.responds(to: #selector(setter: UITextField.inputAccessoryView)),
 //             (textField.inputAccessoryView == nil ||
	//			textField.inputAccessoryView?.tag == IQKeyboardManager.kIQPreviousNextButtonToolbarTag ||
	//			textField.inputAccessoryView?.tag == IQKeyboardManager.kIQDoneButtonToolbarTag) else
	//	{
	//		return

	//	}

	//	let startTime = CACurrentMediaTime()

	//	showLog(">>>>> \(#function) started >>>>>", indentation: 1)


	//	showLog("Found \(siblings.count) responder sibling(s)")


	//	let rightConfiguration: IQBarButtonItemConfiguration


	//	if let doneBarButtonItemImage = toolbarDoneBarButtonItemImage {
	//		rightConfiguration = IQBarButtonItemConfiguration(image: doneBarButtonItemImage, action: #selector(self.doneAction(_:)))
 //       } else if let doneBarButtonItemText = toolbarDoneBarButtonItemText {
	//		rightConfiguration = IQBarButtonItemConfiguration(title: doneBarButtonItemText, action: #selector(self.doneAction(_:)))
 //       } else
	//	{
	//		rightConfiguration = IQBarButtonItemConfiguration(barButtonSystemItem: .done, action: #selector(self.doneAction(_:)))
 //       }
	//	rightConfiguration.accessibilityLabel = toolbarDoneBarButtonItemAccessibilityLabel ?? "Done"

	//	//    If only one object is found, then adding only Done button.
	//	if (siblings.count <= 1 && previousNextDisplayMode == .default) || previousNextDisplayMode == .alwaysHide {

	//		textField.addKeyboardToolbarWithTarget(target: self, titleText: (shouldShowToolbarPlaceholder ? textField.drawingToolbarPlaceholder : nil), rightBarButtonConfiguration: rightConfiguration, previousBarButtonConfiguration: nil, nextBarButtonConfiguration: nil)


	//		textField.inputAccessoryView?.tag = IQKeyboardManager.kIQDoneButtonToolbarTag //  (Bug ID: #78)


	//	} else if previousNextDisplayMode == .default || previousNextDisplayMode == .alwaysShow {

	//		let prevConfiguration: IQBarButtonItemConfiguration


	//		if let doneBarButtonItemImage = toolbarPreviousBarButtonItemImage {
	//			prevConfiguration = IQBarButtonItemConfiguration(image: doneBarButtonItemImage, action: #selector(self.previousAction(_:)))
 //           } else if let doneBarButtonItemText = toolbarPreviousBarButtonItemText {
	//			prevConfiguration = IQBarButtonItemConfiguration(title: doneBarButtonItemText, action: #selector(self.previousAction(_:)))
 //           } else
	//		{
	//			prevConfiguration = IQBarButtonItemConfiguration(image: (UIImage.keyboardPreviousImage() ?? UIImage()), action: #selector(self.previousAction(_:)))
 //           }
	//		prevConfiguration.accessibilityLabel = toolbarPreviousBarButtonItemAccessibilityLabel ?? "Previous"


	//		let nextConfiguration: IQBarButtonItemConfiguration


	//		if let doneBarButtonItemImage = toolbarNextBarButtonItemImage {
	//			nextConfiguration = IQBarButtonItemConfiguration(image: doneBarButtonItemImage, action: #selector(self.nextAction(_:)))
 //           } else if let doneBarButtonItemText = toolbarNextBarButtonItemText {
	//			nextConfiguration = IQBarButtonItemConfiguration(title: doneBarButtonItemText, action: #selector(self.nextAction(_:)))
 //           } else
	//		{
	//			nextConfiguration = IQBarButtonItemConfiguration(image: (UIImage.keyboardNextImage() ?? UIImage()), action: #selector(self.nextAction(_:)))
 //           }
	//		nextConfiguration.accessibilityLabel = toolbarNextBarButtonItemAccessibilityLabel ?? "Next"


	//		textField.addKeyboardToolbarWithTarget(target: self, titleText: (shouldShowToolbarPlaceholder ? textField.drawingToolbarPlaceholder : nil), rightBarButtonConfiguration: rightConfiguration, previousBarButtonConfiguration: prevConfiguration, nextBarButtonConfiguration: nextConfiguration)


	//		textField.inputAccessoryView?.tag = IQKeyboardManager.kIQPreviousNextButtonToolbarTag //  (Bug ID: #78)

	//	}

	//	let toolbar = textField.keyboardToolbar

	//	//Setting toolbar tintColor //  (Enhancement ID: #30)
	//	toolbar.tintColor = shouldToolbarUsesTextFieldTintColor ? textField.tintColor : toolbarTintColor

	//	//  Setting toolbar to keyboard.
	//	if let textFieldView = textField as? UITextInput {

	//		//Bar style according to keyboard appearance
	//		switch textFieldView.keyboardAppearance {

	//			case .dark ?:
	//			toolbar.barStyle = .black

	//			toolbar.barTintColor = nil

	//		default:
	//				toolbar.barStyle = .default

	//			toolbar.barTintColor = toolbarBarTintColor

	//		}
	//	}

	//	//Setting toolbar title font.   //  (Enhancement ID: #30)
	//	if shouldShowToolbarPlaceholder, !textField.shouldHideToolbarPlaceholder {

	//		//Updating placeholder font to toolbar.     //(Bug ID: #148, #272)
	//		if toolbar.titleBarButton.title == nil ||
	//			toolbar.titleBarButton.title != textField.drawingToolbarPlaceholder {
	//			toolbar.titleBarButton.title = textField.drawingToolbarPlaceholder

	//		}

	//		//Setting toolbar title font.   //  (Enhancement ID: #30)
	//		toolbar.titleBarButton.titleFont = placeholderFont

	//		//Setting toolbar title color.   //  (Enhancement ID: #880)
	//		toolbar.titleBarButton.titleColor = placeholderColor

	//		//Setting toolbar button title color.   //  (Enhancement ID: #880)
	//		toolbar.titleBarButton.selectableTitleColor = placeholderButtonColor


	//	} else
	//	{
	//		toolbar.titleBarButton.title = nil

	//	}

	//	//In case of UITableView (Special), the next/previous buttons has to be refreshed everytime.    (Bug ID: #56)

	//	textField.keyboardToolbar.previousBarButton.isEnabled = (siblings.first != textField)   //    If firstTextField, then previous should not be enabled.

	//	textField.keyboardToolbar.nextBarButton.isEnabled = (siblings.last != textField)        //    If lastTextField then next should not be enaled.


	//	let elapsedTime = CACurrentMediaTime() - startTime

	//	showLog("<<<<< \(#function) ended: \(elapsedTime) seconds <<<<<", indentation: -1)

	//}
}


