using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiSwipeView : ContentView
	{
		const float MinimumOpenSwipeThresholdPercentage = 0.15f; // 15%
		const float OpenSwipeThresholdPercentage = 0.6f; // 60%
		const double SwipeAnimationDuration = 0.2;

		readonly SwipeRecognizerProxy _proxy;
		readonly Dictionary<ISwipeItem, object> _swipeItems;
		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		readonly UITapGestureRecognizer _tapGestureRecognizer;
		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		readonly UIPanGestureRecognizer _panGestureRecognizer;
		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIView _contentView;
		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIStackView _actionView;
		SwipeTransitionMode _swipeTransitionMode;
		SwipeDirection? _swipeDirection;
		CGPoint _initialPoint;
		bool _isTouchDown;
		bool _isSwiping;
		double _swipeOffset;
		double _swipeThreshold;
		CGRect _originalBounds;
		List<CGRect> _swipeItemsRect;
		bool _isSwipeEnabled;
		bool _isScrollEnabled;
		bool _isResettingSwipe;
		bool _isOpen;
		OpenSwipeItem _previousOpenSwipeItem;

		internal ISwipeView? Element => CrossPlatformLayout as ISwipeView;

		public MauiSwipeView()
		{
			_proxy = new(this);
			_swipeItemsRect = new List<CGRect>();
			_contentView = new UIView();
			_actionView = new UIStackView();
			_swipeItems = new Dictionary<ISwipeItem, object>();
			_isScrollEnabled = true;

			_tapGestureRecognizer = new UITapGestureRecognizer(_proxy.HandleTap)
			{
				CancelsTouchesInView = false,
				DelaysTouchesBegan = false,
				DelaysTouchesEnded = false,
				ShouldReceiveTouch = _proxy.OnShouldReceiveTouch,
			};

			_panGestureRecognizer = new UIPanGestureRecognizer(_proxy.HandlePan)
			{
				CancelsTouchesInView = false,
				DelaysTouchesBegan = false,
				DelaysTouchesEnded = false,
				ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true,
			};

			AddGestureRecognizer(_tapGestureRecognizer);
			AddGestureRecognizer(_panGestureRecognizer);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Bounds.X < 0 || Bounds.Y < 0)
				Bounds = new CGRect(0, 0, Bounds.Width, Bounds.Height);

			if (_contentView != null && _contentView.Frame.IsEmpty)
				_contentView.Frame = Bounds;
		}

		public override void TouchesEnded(NSSet touches, UIEvent? evt)
		{
			if (_swipeOffset != 0)
			{
				TouchesCancelled(touches, evt);
				return;
			}

			base.TouchesEnded(touches, evt);
		}

		public override void TouchesCancelled(NSSet touches, UIEvent? evt)
		{
			var navigationController = GetUINavigationController(GetViewController());

			if (navigationController != null)
				navigationController.InteractivePopGestureRecognizer.Enabled = true;

			if (touches.AnyObject is UITouch anyObject)
			{
				CGPoint point = anyObject.LocationInView(this);
				HandleTouchInteractions(GestureStatus.Canceled, point);
			}

			base.TouchesCancelled(touches, evt);
		}

		public override UIView HitTest(CGPoint point, UIEvent? uievent)
		{
			if (!UserInteractionEnabled || Hidden)
				return null!;

			foreach (var subview in Subviews)
			{
				if (subview.UserInteractionEnabled)
				{
					var view = HitTest(subview, point, uievent);

					if (view != null)
						return view;
				}
			}

			return base.HitTest(point, uievent)!;
		}

		UIView? HitTest(UIView view, CGPoint point, UIEvent? uievent)
		{
			if (view.Subviews == null)
				return null;

			foreach (var subview in view.Subviews)
			{
				if (subview.UserInteractionEnabled)
				{
					CGPoint subPoint = subview.ConvertPointFromView(point, this);
					UIView? result = subview.HitTest(subPoint, uievent);

					if (result != null)
					{
						return result;
					}
				}
			}

			return null;
		}

		internal void UpdateContent(ISwipeView swipeView, IMauiContext mauiContext)
		{
			ClipsToBounds = true;
			_contentView?.RemoveFromSuperview();
			if (swipeView?.PresentedContent is IView view)
			{
				if (Subviews.Length > 0)
					_contentView = Subviews[0];

				_contentView = view.ToPlatform(mauiContext);
			}
			else
			{
				_contentView = CreateEmptyContent();
			}

			AddSubview(_contentView);

			if (_contentView != null)
				BringSubviewToFront(_contentView);
		}

		class SwipeRecognizerProxy
		{
			readonly WeakReference<MauiSwipeView> _view;

			public SwipeRecognizerProxy(MauiSwipeView view) => _view = new(view);

			public bool OnShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
			{
				return _view.TryGetTarget(out var view) && view._swipeOffset != 0;
			}

			public void HandleTap(UITapGestureRecognizer recognizer)
			{
				if (!_view.TryGetTarget(out var view))
					return;

				if (view._isSwiping)
					return;

				var state = recognizer.State;
				if (state != UIGestureRecognizerState.Cancelled)
				{
					if (view._contentView == null)
						return;

					var point = recognizer.LocationInView(view);

					if (view._isOpen)
					{
						if (!view.TouchInsideContent(point))
							view.ProcessTouchSwipeItems(point);
						else
							view.ResetSwipe();
					}
				}
			}

			public void HandlePan(UIPanGestureRecognizer panGestureRecognizer)
			{
				if (!_view.TryGetTarget(out var view))
					return;

				if (view._isSwipeEnabled && panGestureRecognizer != null)
				{
					CGPoint point = panGestureRecognizer.LocationInView(view);
					var navigationController = GetUINavigationController(view.GetViewController());

					switch (panGestureRecognizer.State)
					{
						case UIGestureRecognizerState.Began:
							if (navigationController != null)
								navigationController.InteractivePopGestureRecognizer.Enabled = false;

							view.HandleTouchInteractions(GestureStatus.Started, point);
							break;
						case UIGestureRecognizerState.Changed:
							view.HandleTouchInteractions(GestureStatus.Running, point);
							break;
						case UIGestureRecognizerState.Ended:
							if (navigationController != null)
								navigationController.InteractivePopGestureRecognizer.Enabled = true;

							view.HandleTouchInteractions(GestureStatus.Completed, point);
							break;
						case UIGestureRecognizerState.Cancelled:
							if (navigationController != null)
								navigationController.InteractivePopGestureRecognizer.Enabled = true;

							view.HandleTouchInteractions(GestureStatus.Canceled, point);
							break;
					}
				}
			}
		}

		static UIView CreateEmptyContent()
		{
			var emptyContentView = new UIView
			{
				BackgroundColor = Colors.Transparent.ToPlatform()
			};

			return emptyContentView;
		}

		internal void UpdateIsSwipeEnabled(ISwipeView swipeView)
		{
			UserInteractionEnabled = true;
			_isSwipeEnabled = swipeView.IsEnabled;

			if (swipeView?.PresentedContent is IView view)
			{
				var isContentEnabled = view.IsEnabled;
				_contentView.UserInteractionEnabled = isContentEnabled;
			}
		}

		bool IsHorizontalSwipe()
		{
			return _swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right;
		}

		static bool IsValidSwipeItems(ISwipeItems? swipeItems)
		{
			return swipeItems != null && swipeItems.Any(GetIsVisible);
		}

		void UpdateSwipeItems()
		{
			if (_contentView == null || Element?.Handler?.MauiContext == null)
				return;

			ISwipeItems? items = Element.GetSwipeItemsByDirection(_swipeDirection);

			if (items == null || items.Count == 0)
				return;

			_swipeItemsRect = new List<CGRect>();

			double swipeItemsWidth;

			if (_swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right)
				swipeItemsWidth = items.Count * SwipeViewExtensions.SwipeItemWidth;
			else
				swipeItemsWidth = _contentView.Frame.Width;

			_actionView = new UIStackView
			{
				Axis = UILayoutConstraintAxis.Horizontal,
				Frame = new CGRect(0, 0, swipeItemsWidth, _contentView.Frame.Height)
			};

			foreach (var item in items)
			{
				UIView swipeItem = item.ToPlatform(Element.Handler.MauiContext);
				_actionView.AddSubview(swipeItem);
				_swipeItems.Add(item, swipeItem);
			}

			AddSubview(_actionView);
			BringSubviewToFront(_contentView);

			LayoutSwipeItems(GetNativeSwipeItems());
		}

		void LayoutSwipeItems(List<UIView> childs)
		{
			if (_actionView == null || childs == null || Element == null)
				return;

			_swipeItemsRect.Clear();

			var items = GetSwipeItemsByDirection();

			if (items == null || items.Count == 0)
				return;

			if (_originalBounds == CGRect.Empty)
				_originalBounds = _contentView.Frame;

			int i = 0;
			float previousWidth = 0;

			foreach (var child in childs)
			{
				if (!child.Hidden)
				{
					var item = items[i];
					var swipeItemSize = Element.GetSwipeItemSize(item, _contentView, _swipeDirection);

					float swipeItemHeight = (float)swipeItemSize.Height;
					float swipeItemWidth = (float)swipeItemSize.Width;

					switch (_swipeDirection)
					{
						case SwipeDirection.Left:
							child.Frame = new CGRect(_originalBounds.X + _contentView.Frame.Width - (swipeItemWidth + previousWidth), _originalBounds.Y, swipeItemWidth, swipeItemHeight);
							break;
						case SwipeDirection.Right:
							child.Frame = new CGRect(_originalBounds.X + previousWidth, _originalBounds.Y, swipeItemWidth, swipeItemHeight);
							break;
						case SwipeDirection.Down:
							child.Frame = new CGRect(_originalBounds.X + previousWidth, _originalBounds.Y, swipeItemWidth, swipeItemHeight);
							break;
						case SwipeDirection.Up:
							child.Frame = new CGRect(_originalBounds.X + previousWidth, _contentView.Frame.Height - swipeItemHeight + _originalBounds.Y, swipeItemWidth, swipeItemHeight);
							break;
					}

					if (child is UIButton button)
					{
						UpdateSwipeItemInsets(button);
					}

					i++;
					previousWidth += swipeItemWidth;
				}

				_swipeItemsRect.Add(child.Frame);
			}
		}

		List<UIView> GetNativeSwipeItems()
		{
			var swipeItems = new List<UIView>();

			foreach (var view in _actionView.Subviews)
				swipeItems.Add(view);

			return swipeItems;
		}

		internal void UpdateIsVisibleSwipeItem(ISwipeItem item)
		{
			if (!_isOpen)
				return;

			if (item?.Handler?.PlatformView is UIView platformView)
			{
				_swipeThreshold = 0;
				LayoutSwipeItems(GetNativeSwipeItems());
				SwipeToThreshold(false);
			}
		}

		internal void UpdateSwipeTransitionMode(SwipeTransitionMode swipeTransitionMode)
		{
			_swipeTransitionMode = swipeTransitionMode;
		}

		static void UpdateSwipeItemInsets(UIButton button, float spacing = 0.0f)
		{
			if (button.ImageView?.Image == null)
				return;

			button.ContentMode = UIViewContentMode.Center;
			button.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

			var imageSize = button.ImageView.Image.Size;

			var titleEdgeInsets = new UIEdgeInsets(spacing, -imageSize.Width, -imageSize.Height, 0.0f);
#pragma warning disable CA1416 // TODO: 'TitleEdgeInsets', 'ImageEdgeInsets' has [UnsupportedOSPlatform("ios15.0")]
#pragma warning disable CA1422 // Validate platform compatibility
			button.TitleEdgeInsets = titleEdgeInsets;


			var labelString = button.TitleLabel.Text ?? string.Empty;

#pragma warning disable BI1234 // Type or member is obsolete, unsupported from version 7.0
			var titleSize = !string.IsNullOrEmpty(labelString) ? labelString.StringSize(button.TitleLabel.Font) : CGSize.Empty;
#pragma warning restore BI1234 // Type or member is obsolete
			var imageEdgeInsets = new UIEdgeInsets(-(titleSize.Height + spacing), 0.0f, 0.0f, -titleSize.Width);
			button.ImageEdgeInsets = imageEdgeInsets;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
		}

		void HandleTouchInteractions(GestureStatus status, CGPoint point)
		{
			switch (status)
			{
				case GestureStatus.Started:
					ProcessTouchDown(point);
					break;
				case GestureStatus.Running:
					ProcessTouchMove(point);
					break;
				case GestureStatus.Canceled:
				case GestureStatus.Completed:
					ProcessTouchUp();
					break;
			}

			_isTouchDown = false;
		}

		void ProcessTouchDown(CGPoint point)
		{
			if (_isSwiping || _isTouchDown || _contentView == null)
				return;

			if (TouchInsideContent(point) && _isOpen)
				ResetSwipe();

			_initialPoint = point;
			_isTouchDown = true;
		}

		void ProcessTouchMove(CGPoint point)
		{
			if (_contentView == null || !TouchInsideContent(point))
				return;

			if (!_isOpen)
			{
				ResetSwipeToInitialPosition();

				_swipeDirection = SwipeDirectionHelper.GetSwipeDirection(new Point(_initialPoint.X, _initialPoint.Y), new Point(point.X, point.Y));

				UpdateSwipeItems();
			}

			if (!_isSwiping)
			{
				RaiseSwipeStarted();
				_isSwiping = true;
			}

			if (!ValidateSwipeDirection() || _isResettingSwipe)
				return;

			_swipeOffset = GetSwipeOffset(_initialPoint, point);
			UpdateIsOpen(_swipeOffset != 0);

			if (Math.Abs(_swipeOffset) > double.Epsilon)
			{
				IsParentScrollEnabled(false);
				Swipe();
			}

			RaiseSwipeChanging();
		}

		void ProcessTouchUp()
		{
			_isTouchDown = false;

			if (!_isSwiping)
				return;

			_isSwiping = false;
			IsParentScrollEnabled(true);

			RaiseSwipeEnded();

			if (_isResettingSwipe || !ValidateSwipeDirection())
				return;

			ValidateSwipeThreshold();
		}

		void IsParentScrollEnabled(bool scrollEnabled)
		{
			var swipeThresholdPercent = MinimumOpenSwipeThresholdPercentage * GetSwipeThreshold();

			if (Math.Abs(_swipeOffset) < swipeThresholdPercent)
				return;

			if (scrollEnabled == _isScrollEnabled)
				return;

			_isScrollEnabled = scrollEnabled;

			var parent = this.GetParentOfType<UIScrollView>();

			if (parent != null)
				parent.ScrollEnabled = _isScrollEnabled;
		}

		bool TouchInsideContent(CGPoint point)
		{
			if (_contentView == null)
				return false;

			return _contentView.Frame.Contains(point);
		}

		ISwipeItems? GetSwipeItemsByDirection()
		{
			if (_swipeDirection.HasValue)
				return GetSwipeItemsByDirection(_swipeDirection.Value);

			return null;
		}

		ISwipeItems? GetSwipeItemsByDirection(SwipeDirection swipeDirection)
		{
			ISwipeItems? swipeItems = null;

			switch (swipeDirection)
			{
				case SwipeDirection.Left:
					swipeItems = Element?.RightItems;
					break;
				case SwipeDirection.Right:
					swipeItems = Element?.LeftItems;
					break;
				case SwipeDirection.Up:
					swipeItems = Element?.BottomItems;
					break;
				case SwipeDirection.Down:
					swipeItems = Element?.TopItems;
					break;
			}

			return swipeItems;
		}

		void Swipe(bool animated = false)
		{
			if (_contentView == null || Element == null)
				return;

			var offset = ValidateSwipeOffset(_swipeOffset);
			_isOpen = offset != 0;
			var swipeAnimationDuration = animated ? SwipeAnimationDuration : 0;

			if (_swipeTransitionMode == SwipeTransitionMode.Reveal)
			{
				Animate(swipeAnimationDuration, 0.0, UIViewAnimationOptions.CurveEaseOut, () =>
				{
					switch (_swipeDirection)
					{
						case SwipeDirection.Left:
							_contentView.Frame = new CGRect(_originalBounds.X + offset, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
							break;
						case SwipeDirection.Right:
							_contentView.Frame = new CGRect(_originalBounds.X + offset, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
							break;
						case SwipeDirection.Up:
							_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y + offset, _originalBounds.Width, _originalBounds.Height);
							break;
						case SwipeDirection.Down:
							_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y + offset, _originalBounds.Width, _originalBounds.Height);
							break;
					}
				}, null);
			}

			if (_swipeTransitionMode == SwipeTransitionMode.Drag)
			{
				var actionBounds = _actionView.Bounds;
				double actionSize;

				Animate(swipeAnimationDuration, 0.0, UIViewAnimationOptions.CurveEaseOut, () =>
				{
					switch (_swipeDirection)
					{
						case SwipeDirection.Left:
							_contentView.Frame = new CGRect(_originalBounds.X + offset, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
							actionSize = Element.RightItems.Count * SwipeViewExtensions.SwipeItemWidth;
							_actionView.Frame = new CGRect(actionSize + offset, actionBounds.Y, actionBounds.Width, actionBounds.Height);
							break;
						case SwipeDirection.Right:
							_contentView.Frame = new CGRect(_originalBounds.X + offset, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
							actionSize = Element.LeftItems.Count * SwipeViewExtensions.SwipeItemWidth;
							_actionView.Frame = new CGRect(-actionSize + offset, actionBounds.Y, actionBounds.Width, actionBounds.Height);
							break;
						case SwipeDirection.Up:
							_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y + offset, _originalBounds.Width, _originalBounds.Height);
							actionSize = _contentView.Frame.Height;
							_actionView.Frame = new CGRect(actionBounds.X, actionSize - Math.Abs(offset), actionBounds.Width, actionBounds.Height);
							break;
						case SwipeDirection.Down:
							_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y + offset, _originalBounds.Width, _originalBounds.Height);
							actionSize = _contentView.Frame.Height;
							_actionView.Frame = new CGRect(actionBounds.X, -actionSize + Math.Abs(offset), actionBounds.Width, actionBounds.Height);
							break;
					}
				}, null);
			}
		}

		double ValidateSwipeOffset(double offset)
		{
			var swipeThreshold = GetSwipeThreshold();

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
					if (offset > 0)
						offset = 0;

					if (_isResettingSwipe && offset < 0)
						offset = 0;

					if (Math.Abs(offset) > swipeThreshold)
						return -swipeThreshold;
					break;
				case SwipeDirection.Right:
					if (offset < 0)
						offset = 0;

					if (_isResettingSwipe && offset > 0)
						offset = 0;

					if (Math.Abs(offset) > swipeThreshold)
						return swipeThreshold;
					break;
				case SwipeDirection.Up:
					if (offset > 0)
						offset = 0;

					if (_isResettingSwipe && offset < 0)
						offset = 0;

					if (Math.Abs(offset) > swipeThreshold)
						return -swipeThreshold;
					break;
				case SwipeDirection.Down:
					if (offset < 0)
						offset = 0;

					if (_isResettingSwipe && offset > 0)
						offset = 0;

					if (Math.Abs(offset) > swipeThreshold)
						return swipeThreshold;
					break;
			}

			return offset;
		}

		void DisposeSwipeItems()
		{
			_isOpen = false;
			_swipeItems.Clear();
			_swipeThreshold = 0;
			_swipeOffset = 0;
			_originalBounds = CGRect.Empty;

			if (_actionView != null)
			{
				_actionView.RemoveFromSuperview();
			}

			if (_swipeItemsRect != null)
			{
				_swipeItemsRect.Clear();
			}

			UpdateIsOpen(false);
		}

		void ResetSwipeToInitialPosition()
		{
			_isResettingSwipe = false;
			_isSwiping = false;
			_swipeThreshold = 0;
			_swipeDirection = null;
			DisposeSwipeItems();
		}

		internal void ResetSwipe(bool animated = true)
		{
			if (_swipeItemsRect == null || _contentView == null)
				return;

			_isResettingSwipe = true;
			_isSwiping = false;
			_swipeThreshold = 0;
			_swipeDirection = null;

			if (animated)
			{
				Animate(SwipeAnimationDuration, 0.0, UIViewAnimationOptions.CurveEaseOut, () =>
				{
					if (_originalBounds != CGRect.Empty)
						_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
				},
				() =>
				{
					DisposeSwipeItems();
					_isResettingSwipe = false;
				});
			}
			else
			{
				if (_originalBounds != CGRect.Empty)
					_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);

				DisposeSwipeItems();
				_isResettingSwipe = false;
			}
		}

		void ValidateSwipeThreshold()
		{
			if (_swipeDirection == null)
				return;

			var swipeThresholdPercent = OpenSwipeThresholdPercentage * GetSwipeThreshold();

			if (Math.Abs(_swipeOffset) >= swipeThresholdPercent)
			{
				var swipeItems = GetSwipeItemsByDirection();

				if (swipeItems == null)
					return;

				if (swipeItems.Mode == SwipeMode.Execute)
				{
					foreach (var swipeItem in swipeItems)
					{
						if (GetIsVisible(swipeItem))
							MauiSwipeView.ExecuteSwipeItem(swipeItem);
					}

					if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
						ResetSwipe();
				}
				else
					SwipeToThreshold();
			}
			else
				ResetSwipe();
		}

		void SwipeToThreshold(bool animated = true)
		{
			if (Element == null)
				return;

			var completeAnimationDuration = animated ? SwipeAnimationDuration : 0;

			if (_swipeTransitionMode == SwipeTransitionMode.Reveal)
			{
				Animate(completeAnimationDuration, 0.0, UIViewAnimationOptions.CurveEaseIn,
					() =>
					{
						_swipeOffset = GetSwipeThreshold();
						double swipeThreshold = _swipeOffset;

						switch (_swipeDirection)
						{
							case SwipeDirection.Left:
								_contentView.Frame = new CGRect(_originalBounds.X - swipeThreshold, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
								break;
							case SwipeDirection.Right:
								_contentView.Frame = new CGRect(_originalBounds.X + swipeThreshold, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
								break;
							case SwipeDirection.Up:
								_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y - swipeThreshold, _originalBounds.Width, _originalBounds.Height);
								break;
							case SwipeDirection.Down:
								_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y + swipeThreshold, _originalBounds.Width, _originalBounds.Height);
								break;
						}
					},
				   () =>
				   {
					   _isSwiping = false;
				   });
			}

			if (_swipeTransitionMode == SwipeTransitionMode.Drag)
			{
				Animate(completeAnimationDuration, 0.0, UIViewAnimationOptions.CurveEaseIn,
					() =>
					{
						_swipeOffset = GetSwipeThreshold();
						double swipeThreshold = _swipeOffset;
						var actionBounds = _actionView.Bounds;
						double actionSize;

						switch (_swipeDirection)
						{
							case SwipeDirection.Left:
								_contentView.Frame = new CGRect(_originalBounds.X - swipeThreshold, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
								actionSize = Element.RightItems.Count * SwipeViewExtensions.SwipeItemWidth;
								_actionView.Frame = new CGRect(actionSize - swipeThreshold, actionBounds.Y, actionBounds.Width, actionBounds.Height);
								break;
							case SwipeDirection.Right:
								_contentView.Frame = new CGRect(_originalBounds.X + swipeThreshold, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
								actionSize = Element.LeftItems.Count * SwipeViewExtensions.SwipeItemWidth;
								_actionView.Frame = new CGRect(-actionSize + swipeThreshold, actionBounds.Y, actionBounds.Width, actionBounds.Height);
								break;
							case SwipeDirection.Up:
								_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y - swipeThreshold, _originalBounds.Width, _originalBounds.Height);
								actionSize = _contentView.Frame.Height;
								_actionView.Frame = new CGRect(actionBounds.X, actionSize - Math.Abs(swipeThreshold), actionBounds.Width, actionBounds.Height);
								break;
							case SwipeDirection.Down:
								_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y + swipeThreshold, _originalBounds.Width, _originalBounds.Height);
								actionSize = _contentView.Frame.Height;
								_actionView.Frame = new CGRect(actionBounds.X, -actionSize + Math.Abs(swipeThreshold), actionBounds.Width, actionBounds.Height);
								break;
						}
					},
				   () =>
				   {
					   _isSwiping = false;
				   });
			}

		}
		static bool GetIsVisible(ISwipeItem swipeItem)
		{
			if (swipeItem is IView view)
				return view.Visibility == Maui.Visibility.Visible;
			else if (swipeItem is ISwipeItemMenuItem menuItem)
				return menuItem.Visibility == Maui.Visibility.Visible;

			return true;
		}

		double GetSwipeThreshold()
		{
			if (Math.Abs(_swipeThreshold) > double.Epsilon)
				return _swipeThreshold;

			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems == null)
				return 0;

			_swipeThreshold = GetSwipeThreshold(swipeItems);

			return _swipeThreshold;
		}

		double GetSwipeThreshold(ISwipeItems swipeItems)
		{
			if (Element == null)
				return default(double);

			var threshold = Element.Threshold;

			if (threshold > 0)
				return threshold;

			double swipeThreshold = 0;
			bool isHorizontal = IsHorizontalSwipe();

			if (swipeItems.Mode == SwipeMode.Reveal)
			{
				if (isHorizontal)
				{
					foreach (var swipeItem in swipeItems)
					{
						if (GetIsVisible(swipeItem))
						{
							var swipeItemSize = Element.GetSwipeItemSize(swipeItem, _contentView, _swipeDirection);
							swipeThreshold += swipeItemSize.Width;
						}
					}
				}
				else
					swipeThreshold = Element.GetSwipeItemHeight(_swipeDirection, _contentView);
			}
			else
				swipeThreshold = CalculateSwipeThreshold();

			return ValidateSwipeThreshold(swipeThreshold);
		}

		double CalculateSwipeThreshold()
		{
			var swipeItems = GetSwipeItemsByDirection();
			if (swipeItems == null || Element == null)
				return SwipeViewExtensions.SwipeThreshold;

			float swipeItemsHeight = 0;
			float swipeItemsWidth = 0;
			bool useSwipeItemsSize = false;

			foreach (var swipeItem in swipeItems)
			{
				if (swipeItem is ISwipeItemView)
					useSwipeItemsSize = true;

				if (GetIsVisible(swipeItem))
				{
					var swipeItemSize = Element.GetSwipeItemSize(swipeItem, _contentView, _swipeDirection);
					swipeItemsHeight += (float)swipeItemSize.Height;
					swipeItemsWidth += (float)swipeItemSize.Width;
				}
			}

			if (useSwipeItemsSize)
			{
				var isHorizontalSwipe = IsHorizontalSwipe();

				return isHorizontalSwipe ? swipeItemsWidth : swipeItemsHeight;
			}
			else
			{
				if (_contentView != null)
				{
					var contentWidth = _contentView.Frame.Width;
					var contentWidthSwipeThreshold = contentWidth * 0.8f;

					return contentWidthSwipeThreshold;
				}
			}

			return SwipeViewExtensions.SwipeThreshold;
		}

		double ValidateSwipeThreshold(double swipeThreshold)
		{
			var swipeFrame = _contentView != null ? _contentView.Frame : Frame;

			if (IsHorizontalSwipe())
			{
				if (swipeThreshold > swipeFrame.Width)
					swipeThreshold = swipeFrame.Width;

				return swipeThreshold;
			}

			if (swipeThreshold > swipeFrame.Height)
				swipeThreshold = swipeFrame.Height;

			return swipeThreshold;
		}

		bool ValidateSwipeDirection()
		{
			if (_swipeDirection == null)
				return false;

			var swipeItems = GetSwipeItemsByDirection();
			return MauiSwipeView.IsValidSwipeItems(swipeItems);
		}

		double GetSwipeOffset(CGPoint initialPoint, CGPoint endPoint)
		{
			double swipeOffset = 0;

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
				case SwipeDirection.Right:
					swipeOffset = endPoint.X - initialPoint.X;
					break;
				case SwipeDirection.Up:
				case SwipeDirection.Down:
					swipeOffset = endPoint.Y - initialPoint.Y;
					break;
			}

			return swipeOffset;
		}

		void ProcessTouchSwipeItems(CGPoint point)
		{
			if (_isResettingSwipe)
				return;

			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems == null || _swipeItemsRect == null)
				return;

			int i = 0;

			foreach (var swipeItemRect in _swipeItemsRect)
			{
				var swipeItem = swipeItems[i];

				if (GetIsVisible(swipeItem))
				{
					var swipeItemX = swipeItemRect.Left;
					var swipeItemY = swipeItemRect.Top;

					if (swipeItemRect.Contains(point))
					{
						MauiSwipeView.ExecuteSwipeItem(swipeItem);

						if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
							ResetSwipe();

						break;
					}
				}

				i++;
			}
		}

		UIViewController? GetViewController()
		{
			var window = Element?.Handler?.MauiContext?.GetPlatformWindow() ??
				throw new InvalidOperationException("Unable to retrieve Platform Window");

			var viewController = window.RootViewController;

			while (viewController?.PresentedViewController != null)
				viewController = viewController.PresentedViewController;

			return viewController;
		}

		static UINavigationController? GetUINavigationController(UIViewController? controller)
		{
			if (controller != null)
			{
				if (controller is UINavigationController)
				{
					return (controller as UINavigationController);
				}

				if (controller.ChildViewControllers.Length != 0)
				{
					var childs = controller.ChildViewControllers.Length;

					for (int i = 0; i < childs; i++)
					{
						var child = GetUINavigationController(controller.ChildViewControllers[i]);

						if (child is UINavigationController)
						{
							return (child as UINavigationController);
						}
					}
				}
			}

			return null;
		}

		static void ExecuteSwipeItem(ISwipeItem item)
		{
			if (item == null)
				return;

			bool isEnabled = true;

			if (item is ISwipeItemMenuItem swipeItem)
				isEnabled = swipeItem.IsEnabled;

			if (item is ISwipeItemView swipeItemView)
				isEnabled = swipeItemView.IsEnabled;

			if (isEnabled)
				item.OnInvoked();
		}

		void UpdateIsOpen(bool isOpen)
		{
			if (Element == null)
				return;

			Element.IsOpen = isOpen;
		}

		internal void ProgrammaticallyOpenSwipeItem(OpenSwipeItem openSwipeItem, bool animated)
		{
			if (_isOpen)
			{
				if (_previousOpenSwipeItem == openSwipeItem)
					return;

				ResetSwipe(false);
			}

			_previousOpenSwipeItem = openSwipeItem;

			switch (openSwipeItem)
			{
				case OpenSwipeItem.BottomItems:
					_swipeDirection = SwipeDirection.Up;
					break;
				case OpenSwipeItem.LeftItems:
					_swipeDirection = SwipeDirection.Right;
					break;
				case OpenSwipeItem.RightItems:
					_swipeDirection = SwipeDirection.Left;
					break;
				case OpenSwipeItem.TopItems:
					_swipeDirection = SwipeDirection.Down;
					break;
			}

			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems is null || !swipeItems.Any(GetIsVisible))
				return;

			UpdateIsOpen(true);

			var swipeThreshold = GetSwipeThreshold();
			UpdateOffset(swipeThreshold);

			UpdateSwipeItems();
			Swipe(animated);

			_swipeOffset = Math.Abs(_swipeOffset);
		}

		void UpdateOffset(double swipeOffset)
		{
			switch (_swipeDirection)
			{
				case SwipeDirection.Right:
				case SwipeDirection.Down:
					_swipeOffset = swipeOffset;
					break;
				case SwipeDirection.Left:
				case SwipeDirection.Up:
					_swipeOffset = -swipeOffset;
					break;
			}
		}

		void RaiseSwipeStarted()
		{
			if (_swipeDirection == null || !ValidateSwipeDirection())
				return;

			Element?.SwipeStarted(new SwipeViewSwipeStarted(_swipeDirection.Value));
		}

		void RaiseSwipeChanging()
		{
			if (_swipeDirection == null)
				return;

			Element?.SwipeChanging(new SwipeViewSwipeChanging(_swipeDirection.Value, _swipeOffset));
		}

		void RaiseSwipeEnded()
		{
			if (_swipeDirection == null || !ValidateSwipeDirection())
				return;

			bool isOpen = false;

			var swipeThresholdPercent = OpenSwipeThresholdPercentage * GetSwipeThreshold();

			if (Math.Abs(_swipeOffset) >= swipeThresholdPercent)
				isOpen = true;

			Element?.SwipeEnded(new SwipeViewSwipeEnded(_swipeDirection.Value, isOpen));
		}
	}
}
