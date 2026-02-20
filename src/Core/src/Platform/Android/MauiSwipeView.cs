using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Graphics;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;
using APointF = Android.Graphics.PointF;
using ARect = Android.Graphics.Rect;
using ATextAlignment = Android.Views.TextAlignment;
using AView = Android.Views.View;
using AWebView = Android.Webkit.WebView;
using SDebug = System.Diagnostics.Debug;

namespace Microsoft.Maui.Platform
{
	public class MauiSwipeView : ContentViewGroup
	{
		const float OpenSwipeThresholdPercentage = 0.6f; // 60%
		const long SwipeAnimationDuration = 200;

		readonly Dictionary<ISwipeItem, object> _swipeItems;
		readonly Context _context;
		SwipeViewPager? _viewPagerParent;
		AView? _contentView;
		LinearLayoutCompat? _actionView;
		SwipeTransitionMode _swipeTransitionMode;
		float _density;
		bool _isTouchDown;
		bool _isSwiping;
		APointF? _initialPoint;
		SwipeDirection? _swipeDirection;
		float _swipeOffset;
		float _swipeThreshold;
		bool _isSwipeEnabled;
		bool _isResettingSwipe;
		bool _isOpen;
		OpenSwipeItem _previousOpenSwipeItem;
		IMauiContext MauiContext => Element?.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null here");

		internal AView Control { get; }
		internal ISwipeView? Element { get; private set; }

		public MauiSwipeView(Context context) : base(context)
		{
			_context = context;

			_swipeItems = new Dictionary<ISwipeItem, object>();

			SetClipChildren(false);
			SetClipToPadding(false);

			_density = context.GetActivity()?.Resources?.DisplayMetrics?.Density ?? 0;
			Control = new AView(_context);
			AddView(Control, LayoutParams.MatchParent);
		}

		// temporary workaround to make it work
		internal void SetElement(ISwipeView swipeView)
		{
			Element = swipeView;

			bool clipToOutline = Element?.Shadow is null && (Element?.Content as IView)?.Shadow is null;
			this.SetClipToOutline(clipToOutline);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (Control != null && Control.Parent != null && _viewPagerParent == null)
				_viewPagerParent = Control.Parent.GetParentOfType<SwipeViewPager>();
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);

			if (_contentView is null || _actionView is null || GetNativeSwipeItems() is not { Count: > 0 } swipeItems)
				return;

			LayoutSwipeItems(swipeItems);
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			base.OnTouchEvent(e);

			if (e?.Action == MotionEventActions.Move && !ShouldInterceptTouch(e))
				return true;

			ProcessSwipingInteractions(e);

			return true;
		}

		bool ShouldInterceptTouch(MotionEvent? e)
		{
			if (_initialPoint == null || e == null)
				return false;

			var interceptPoint = new Point(e.GetX() / _density, e.GetY() / _density);

			var diffX = interceptPoint.X - _initialPoint.X;
			var diffY = interceptPoint.Y - _initialPoint.Y;

			SwipeDirection swipeDirection;

			if (Math.Abs(diffX) > Math.Abs(diffY))
				swipeDirection = diffX > 0 ? SwipeDirection.Right : SwipeDirection.Left;
			else
				swipeDirection = diffY > 0 ? SwipeDirection.Down : SwipeDirection.Up;

			var items = GetSwipeItemsByDirection(swipeDirection);

			if (items == null || items?.Count == 0)
				return false;

			return ShouldInterceptScrollChildrenTouch(swipeDirection);
		}

		bool ShouldInterceptScrollChildrenTouch(SwipeDirection swipeDirection)
		{
			if (_contentView is null || _initialPoint is null)
				return false;

			var viewGroup = _contentView as ViewGroup;

			if (viewGroup is not null)
			{
				int x = (int)(_initialPoint.X * _density);
				int y = (int)(_initialPoint.Y * _density);

				bool isHorizontal = swipeDirection == SwipeDirection.Left || swipeDirection == SwipeDirection.Right;

				for (int i = 0; i < viewGroup.ChildCount; i++)
				{
					var child = viewGroup.GetChildAt(i);

					if (child != null && IsViewInBounds(child, x, y))
					{
						switch (child)
						{
							case AbsListView absListView:
								return ShouldInterceptScrollChildrenTouch(absListView, isHorizontal);
							case RecyclerView recyclerView:
								return ShouldInterceptScrollChildrenTouch(recyclerView, isHorizontal);
							case NestedScrollView scrollView:
								return ShouldInterceptScrollChildrenTouch(scrollView, isHorizontal);
							case AWebView webView:
								return ShouldInterceptScrollChildrenTouch(webView, isHorizontal);
						}
					}
				}
			}

			return true;
		}

		static bool ShouldInterceptScrollChildrenTouch(ViewGroup scrollView, bool isHorizontal)
		{
			AView? scrollViewContent = scrollView.GetChildAt(0);

			if (scrollViewContent != null)
			{
				if (isHorizontal)
					return scrollView.ScrollX == 0 || scrollView.Width == scrollViewContent.Width + scrollView.PaddingLeft + scrollView.PaddingRight;
				else
					return scrollView.ScrollY == 0 || scrollView.Height == scrollViewContent.Height + scrollView.PaddingTop + scrollView.PaddingBottom;
			}

			return true;
		}

		static bool IsViewInBounds(AView view, int x, int y)
		{
			ARect outRect = new ARect();
			view.GetHitRect(outRect);

			return x > outRect.Left && x < outRect.Right && y > outRect.Top && y < outRect.Bottom;
		}

		public override bool OnInterceptTouchEvent(MotionEvent? e)
		{
			return ShouldInterceptTouch(e);
		}

		public override bool DispatchTouchEvent(MotionEvent? e)
		{
			if (e?.Action == MotionEventActions.Down)
			{
				_initialPoint = new APointF(e.GetX() / _density, e.GetY() / _density);
			}

			if (e?.Action == MotionEventActions.Move)
			{
				ResetSwipe(e);
			}

			if (e?.Action == MotionEventActions.Up)
			{
				var touchUpPoint = new APointF(e.GetX() / _density, e.GetY() / _density);

				if (CanProcessTouchSwipeItems(touchUpPoint))
					ProcessTouchSwipeItems(touchUpPoint);
				else
				{
					ResetSwipe(e);

					// Prevent parent touch propagation during swipe gestures to avoid interference with swipe functionality.
					if (!_isSwiping)
					{
						PropagateParentTouch();
					}
				}
			}

			return base.DispatchTouchEvent(e);
		}

		void PropagateParentTouch()
		{
			if (_contentView == null)
				return;

			AView? itemContentView = null;

			var parentFound = _contentView.FindParent(parent =>
			{
				if (parent is RecyclerView)
					return true;

				itemContentView = parent as AView;
				return false;
			});

			if (parentFound != null)
			{
				itemContentView?.CallOnClick();
			}
		}

		internal void UpdateContent()
		{
			if (_contentView != null)
			{
				if (!_contentView.IsDisposed())
				{
					_contentView.RemoveFromParent();
					_contentView.Dispose();
				}
				_contentView = null;
			}


			if (Element?.PresentedContent is IView view)
				_contentView = view.ToPlatform(MauiContext);
			else
				_contentView = CreateEmptyContent();

			_contentView.RemoveFromParent();
			AddView(_contentView);
		}

		AView CreateEmptyContent()
		{
			var emptyContentView = new AView(_context);
			emptyContentView.SetBackgroundColor(Colors.Transparent.ToPlatform());

			return emptyContentView;
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

		bool IsHorizontalSwipe()
		{
			return _swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right;
		}

		static bool IsValidSwipeItems(ISwipeItems? swipeItems)
		{
			return swipeItems != null && swipeItems.Any(GetIsVisible);
		}

		bool ProcessSwipingInteractions(MotionEvent? e)
		{
			if (e == null)
				return false;

			bool? handled = true;
			var point = new APointF(e.GetX() / _density, e.GetY() / _density);

			switch (e.Action)
			{
				case MotionEventActions.Down:
					handled = HandleTouchInteractions(GestureStatus.Started, point);

					if (handled == true)
						Parent?.RequestDisallowInterceptTouchEvent(true);

					break;
				case MotionEventActions.Up:
					handled = HandleTouchInteractions(GestureStatus.Completed, point);

					if (Parent == null)
						break;

					Parent.RequestDisallowInterceptTouchEvent(false);
					break;
				case MotionEventActions.Move:
					handled = HandleTouchInteractions(GestureStatus.Running, point);

					if (handled == true || Parent == null)
						break;

					Parent.RequestDisallowInterceptTouchEvent(true);
					break;
				case MotionEventActions.Cancel:
					handled = HandleTouchInteractions(GestureStatus.Canceled, point);

					if (Parent == null)
						break;

					Parent.RequestDisallowInterceptTouchEvent(false);
					break;
			}

			if (handled.HasValue)
				return !handled.Value;

			return false;
		}

		bool HandleTouchInteractions(GestureStatus status, APointF point)
		{
			if (!_isSwipeEnabled)
				return false;

			switch (status)
			{
				case GestureStatus.Started:
					return !ProcessTouchDown(point);
				case GestureStatus.Running:
					return !ProcessTouchMove(point);
				case GestureStatus.Canceled:
				case GestureStatus.Completed:
					ProcessTouchUp();
					break;
			}

			_isTouchDown = false;

			return true;
		}

		bool ProcessTouchDown(APointF point)
		{
			if (_isSwiping || _isTouchDown || _contentView == null)
				return false;

			_initialPoint = point;
			_isTouchDown = true;

			return true;
		}

		bool ProcessTouchMove(APointF point)
		{
			if (_contentView == null || !TouchInsideContent(point) || _initialPoint == null)
				return false;

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
				return false;

			EnableParentGesture(false);
			_swipeOffset = GetSwipeOffset(_initialPoint, point);
			UpdateIsOpen(_swipeOffset != 0);

			UpdateSwipeItems();

			if (Math.Abs(_swipeOffset) > double.Epsilon)
				Swipe();

			RaiseSwipeChanging();

			return true;
		}

		bool ProcessTouchUp()
		{
			_isTouchDown = false;

			EnableParentGesture(true);

			if (!_isSwiping)
				return false;

			_isSwiping = false;

			RaiseSwipeEnded();

			if (_isResettingSwipe || !ValidateSwipeDirection())
				return false;

			ValidateSwipeThreshold();

			return false;
		}

		bool CanProcessTouchSwipeItems(APointF point)
		{
			// We only invoke the SwipeItem command if we tap on the SwipeItems area
			// and the SwipeView is fully open.
			if (TouchInsideContent(point))
				return false;

			if (_swipeOffset == _swipeThreshold)
				return true;

			return false;
		}

		bool TouchInsideContent(APointF point)
		{
			if (_contentView == null)
				return false;

			bool touchContent = TouchInsideContent(_contentView.Left + _contentView.TranslationX, _contentView.Top + _contentView.TranslationY, _contentView.Width, _contentView.Height, _context.ToPixels(point.X), _context.ToPixels(point.Y));

			return touchContent;
		}

		static bool TouchInsideContent(double x1, double y1, double x2, double y2, double x, double y)
		{
			if (x > x1 && x < (x1 + x2) && y > y1 && y < (y1 + y2))
				return true;

			return false;
		}

		bool ValidateSwipeDirection()
		{
			if (_swipeDirection == null)
				return false;

			var swipeItems = GetSwipeItemsByDirection();
			return IsValidSwipeItems(swipeItems);
		}

		float GetSwipeOffset(APointF initialPoint, APointF endPoint)
		{
			float swipeOffset = 0;

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

			if (swipeOffset == 0)
				swipeOffset = GetSwipeContentOffset();

			return swipeOffset;
		}

		float GetSwipeContentOffset()
		{
			float swipeOffset = 0;

			if (_contentView != null)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:
						swipeOffset = _contentView.TranslationX;
						break;
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						swipeOffset = _contentView.TranslationY;
						break;
				}
			}

			return swipeOffset;
		}

		void UpdateSwipeItems()
		{
			if (_contentView == null || _contentView.IsDisposed() || _actionView != null)
				return;

			ISwipeItems? items = GetSwipeItemsByDirection();

			if (items?.Count == 0 || items == null)
				return;

			_actionView = new LinearLayoutCompat(_context);

			using (var layoutParams = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent))
				_actionView.LayoutParameters = layoutParams;

			_actionView.Orientation = LinearLayoutCompat.Horizontal;

			var swipeItems = new List<AView>();

			foreach (var item in items)
			{
				AView? swipeItem = item?.ToPlatform(MauiContext);

				if (swipeItem is not null)
				{
					if (item is ISwipeItemView formsSwipeItemView)
					{
						_actionView.AddView(swipeItem);
						UpdateSwipeItemViewLayout(formsSwipeItemView);
						_swipeItems.Add(formsSwipeItemView, swipeItem);
					}
					else if (item is ISwipeItemMenuItem menuItem)
					{
						_actionView.AddView(swipeItem);
						_swipeItems.Add(item, swipeItem);
					}

					if (swipeItem != null)
						swipeItems.Add(swipeItem);
				}
			}

			AddView(_actionView);
			if (_contentView != null)
			{
				_contentView.BringToFront();
				int contextX = (int)_contentView.GetX();
				int contentY = (int)_contentView.GetY();
				int contentWidth = _contentView.Width;
				int contentHeight = _contentView.Height;
				_actionView.Layout(contextX, contentY, contextX + contentWidth, contentY + contentHeight);
			}
			LayoutSwipeItems(swipeItems);
			swipeItems.Clear();
		}

		void LayoutSwipeItems(List<AView> childs)
		{
			if (_actionView == null || childs == null || _contentView == null)
				return;

			var items = GetSwipeItemsByDirection();

			if (items == null || items.Count == 0)
				return;

			int i = 0;
			int previousWidth = 0;

			foreach (var child in childs)
			{
				if (child.Visibility == ViewStates.Visible)
				{
					var item = items[i];
					var swipeItemSize = GetSwipeItemSize(item);

					int contentWidth = _contentView.Width;
					int contentHeight = _contentView.Height;
					var swipeItemHeight = (int)_context.ToPixels(swipeItemSize.Height);
					var swipeItemWidth = (int)_context.ToPixels(swipeItemSize.Width);

					int l, t, r, b;
					switch (_swipeDirection)
					{
						case SwipeDirection.Left:
							// Filling from right to left, align to the top
							l = contentWidth - previousWidth - swipeItemWidth;
							t = 0;
							r = contentWidth - previousWidth;
							b = swipeItemHeight;
							break;
						case SwipeDirection.Right:
						case SwipeDirection.Down:
							// Filling from left to right, align to the top
							l = previousWidth;
							t = 0;
							r = previousWidth + swipeItemWidth;
							b = swipeItemHeight;
							break;
						default:
							SDebug.Assert(_swipeDirection == SwipeDirection.Up);
							// Filling from left to right, align to the bottom
							l = previousWidth;
							t = contentHeight - swipeItemHeight;
							r = previousWidth + swipeItemWidth;
							b = contentHeight;
							break;
					}

					child.Measure(
						MeasureSpec.MakeMeasureSpec(swipeItemWidth, MeasureSpecMode.AtMost),
						MeasureSpec.MakeMeasureSpec(swipeItemHeight, MeasureSpecMode.AtMost)
					);

					child.Layout(l, t, r, b);

					i++;
					previousWidth += swipeItemWidth;
				}
			}
		}

		internal void UpdateIsVisibleSwipeItem(ISwipeItem item)
		{
			if (!_isOpen)
				return;

			_swipeItems.TryGetValue(item, out object? view);

			if (view != null && view is AView platformView)
			{
				_swipeThreshold = 0;
				LayoutSwipeItems(GetNativeSwipeItems());
				SwipeToThreshold(false);
			}
		}

		List<AView> GetNativeSwipeItems()
		{
			var swipeItems = new List<AView>();

			if (_actionView == null)
				return swipeItems;

			for (int i = 0; i < _actionView.ChildCount; i++)
			{
				var view = _actionView.GetChildAt(i);
				if (view == null)
					continue;

				swipeItems.Add(view);
			}

			return swipeItems;
		}

		void UpdateSwipeItemViewLayout(ISwipeItemView swipeItemView)
		{
			if (swipeItemView?.Handler is not IPlatformViewHandler handler)
				return;

			var swipeItemSize = GetSwipeItemSize(swipeItemView);
			handler.LayoutVirtualView(0, 0, (int)swipeItemSize.Width, (int)swipeItemSize.Height);

			swipeItemView?.Handler?.ToPlatform().InvalidateMeasure(swipeItemView);
		}

		internal void UpdateIsSwipeEnabled(bool isEnabled)
		{
			_isSwipeEnabled = isEnabled;
		}

		internal void UpdateSwipeTransitionMode(SwipeTransitionMode swipeTransitionMode)
		{
			_swipeTransitionMode = swipeTransitionMode;
		}

		void DisposeSwipeItems()
		{
			_isOpen = false;

			foreach (var item in _swipeItems.Keys)
			{
				item.Handler?.DisconnectHandler();
			}

			_swipeItems.Clear();

			if (_actionView != null)
			{
				_actionView.RemoveFromParent();
				_actionView.Dispose();
				_actionView = null;
			}

			UpdateIsOpen(false);
		}

		void Swipe(bool animated = false)
		{
			if (_contentView == null)
				return;

			var offset = _context.ToPixels(ValidateSwipeOffset(_swipeOffset));
			_isOpen = offset != 0;
			var swipeAnimationDuration = animated ? SwipeAnimationDuration : 0;

			if (_swipeTransitionMode == SwipeTransitionMode.Reveal)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:
						_contentView.Animate()?.TranslationX(offset)?.SetDuration(swipeAnimationDuration);
						break;
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						_contentView.Animate()?.TranslationY(offset)?.SetDuration(swipeAnimationDuration);
						break;
				}
			}

			if (_swipeTransitionMode == SwipeTransitionMode.Drag && Element != null && _actionView != null)
			{
				int actionSize;
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						_contentView.Animate()?.TranslationX(offset)?.SetDuration(swipeAnimationDuration);
						actionSize = (int)_context.ToPixels(Element.RightItems.Count * SwipeViewExtensions.SwipeItemWidth);
						_actionView.Animate()?.TranslationX(actionSize - Math.Abs(offset))?.SetDuration(swipeAnimationDuration);
						break;
					case SwipeDirection.Right:
						_contentView.Animate()?.TranslationX(offset)?.SetDuration(swipeAnimationDuration);
						actionSize = (int)_context.ToPixels(Element.LeftItems.Count * SwipeViewExtensions.SwipeItemWidth);
						_actionView.Animate()?.TranslationX(-actionSize + offset)?.SetDuration(swipeAnimationDuration);
						break;
					case SwipeDirection.Up:
						_contentView.Animate()?.TranslationY(offset)?.SetDuration(swipeAnimationDuration);
						actionSize = _contentView.Height;
						_actionView.Animate()?.TranslationY(actionSize - Math.Abs(offset))?.SetDuration(swipeAnimationDuration);
						break;
					case SwipeDirection.Down:
						_contentView.Animate()?.TranslationY(offset)?.SetDuration(swipeAnimationDuration);
						actionSize = _contentView.Height;
						_actionView.Animate()?.TranslationY(-actionSize + Math.Abs(offset))?.SetDuration(swipeAnimationDuration);
						break;
				}
			}
		}

		void ResetSwipeToInitialPosition()
		{
			_isResettingSwipe = false;
			_isSwiping = false;
			_swipeThreshold = 0;
			_swipeDirection = null;
			DisposeSwipeItems();
		}

		void ResetSwipe(MotionEvent e, bool animated = true)
		{
			if (!_isSwiping && _isOpen)
			{
				var touchPoint = new APointF(e.GetX() / _density, e.GetY() / _density);

				if (TouchInsideContent(touchPoint))
					ResetSwipe(animated);
			}
		}

		void ResetSwipe(bool animated = true)
		{
			if (_contentView == null)
				return;

			_isResettingSwipe = true;
			_isSwiping = false;
			_swipeThreshold = 0;

			if (animated)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:

						_swipeDirection = null;

						_contentView.Animate()?.TranslationX(0)?.SetDuration(SwipeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() =>
						{
							if (_swipeDirection == null)
								DisposeSwipeItems();

							_isResettingSwipe = false;
						}));
						break;
					case SwipeDirection.Up:
					case SwipeDirection.Down:

						_swipeDirection = null;

						_contentView.Animate()?.TranslationY(0)?.SetDuration(SwipeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() =>
						{
							if (_swipeDirection == null)
								DisposeSwipeItems();

							_isResettingSwipe = false;
						}));
						break;
					default:
						_isResettingSwipe = false;
						break;
				}
			}
			else
			{
				_contentView.TranslationX = 0;
				_contentView.TranslationY = 0;

				DisposeSwipeItems();
				_isResettingSwipe = false;
			}
		}

		void SwipeToThreshold(bool animated = true)
		{
			var completeAnimationDuration = animated ? SwipeAnimationDuration : 0;
			_swipeOffset = GetSwipeThreshold();
			float swipeThreshold = _context.ToPixels(_swipeOffset);

			if (_swipeTransitionMode == SwipeTransitionMode.Reveal && _contentView != null)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						_contentView.Animate()?.TranslationX(-swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
					case SwipeDirection.Right:
						_contentView.Animate()?.TranslationX(swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
					case SwipeDirection.Up:
						_contentView.Animate()?.TranslationY(-swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
					case SwipeDirection.Down:
						_contentView.Animate()?.TranslationY(swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
				}
			}

			if (_swipeTransitionMode == SwipeTransitionMode.Drag && _contentView != null && _actionView != null && Element != null)
			{
				int actionSize;
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						_contentView.Animate()?.TranslationX(-swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = (int)_context.ToPixels(Element.RightItems.Count * SwipeViewExtensions.SwipeItemWidth);
						_actionView.Animate()?.TranslationX(actionSize - swipeThreshold)?.SetDuration(completeAnimationDuration);
						break;
					case SwipeDirection.Right:
						_contentView.Animate()?.TranslationX(swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = (int)_context.ToPixels(Element.LeftItems.Count * SwipeViewExtensions.SwipeItemWidth);
						_actionView.Animate()?.TranslationX(-actionSize + swipeThreshold)?.SetDuration(completeAnimationDuration);
						break;
					case SwipeDirection.Up:
						_contentView.Animate()?.TranslationY(-swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = _contentView.Height;
						_actionView.Animate()?.TranslationY(actionSize - Math.Abs(swipeThreshold))?.SetDuration(completeAnimationDuration);
						break;
					case SwipeDirection.Down:
						_contentView.Animate()?.TranslationY(swipeThreshold)?.SetDuration(completeAnimationDuration)?.WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = _contentView.Height;
						_actionView.Animate()?.TranslationY(-actionSize + Math.Abs(swipeThreshold))?.SetDuration(completeAnimationDuration);
						break;
				}
			}
		}

		float ValidateSwipeOffset(float offset)
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
							ExecuteSwipeItem(swipeItem);
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

		float GetSwipeThreshold()
		{
			if (Math.Abs(_swipeThreshold) > double.Epsilon)
				return _swipeThreshold;

			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems == null)
				return 0;

			_swipeThreshold = GetSwipeThreshold(swipeItems);

			return _swipeThreshold;
		}

		float GetSwipeThreshold(ISwipeItems swipeItems)
		{
			if (Element == null)
				return 0f;

			double threshold = Element.Threshold;

			if (threshold > 0)
				return (float)threshold;

			float swipeThreshold = 0;

			bool isHorizontal = IsHorizontalSwipe();

			if (swipeItems.Mode == SwipeMode.Reveal)
			{
				if (isHorizontal)
				{
					foreach (var swipeItem in swipeItems)
					{
						if (GetIsVisible(swipeItem))
						{
							var swipeItemSize = GetSwipeItemSize(swipeItem);
							swipeThreshold += (float)swipeItemSize.Width;
						}
					}
				}
				else
					swipeThreshold = GetSwipeItemHeight();
			}
			else
				swipeThreshold = CalculateSwipeThreshold();

			return ValidateSwipeThreshold(swipeThreshold);
		}

		static bool GetIsVisible(ISwipeItem swipeItem)
		{
			if (swipeItem is IView view)
				return view.Visibility == Maui.Visibility.Visible;
			else if (swipeItem is ISwipeItemMenuItem menuItem)
				return menuItem.Visibility == Maui.Visibility.Visible;

			return true;
		}

		float CalculateSwipeThreshold()
		{
			var swipeItems = GetSwipeItemsByDirection();
			if (swipeItems == null)
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
					var swipeItemSize = GetSwipeItemSize(swipeItem);
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
					var contentWidth = (float)_context.FromPixels(_contentView.Width);
					var contentWidthSwipeThreshold = contentWidth * 0.8f;

					return contentWidthSwipeThreshold;
				}
			}

			return SwipeViewExtensions.SwipeThreshold;
		}


		float GetRevealModeSwipeThreshold()
		{
			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems == null)
				return SwipeViewExtensions.SwipeThreshold;

			bool isHorizontal = IsHorizontalSwipe();

			float swipeItemsSize = 0;
			bool hasSwipeItemView = false;

			foreach (var swipeItem in swipeItems)
			{
				if (swipeItem is ISwipeItemView)
					hasSwipeItemView = true;

				if (GetIsVisible(swipeItem))
				{
					var swipeItemSize = GetSwipeItemSize(swipeItem);

					if (isHorizontal)
						swipeItemsSize += (float)swipeItemSize.Width;
					else
						swipeItemsSize += (float)swipeItemSize.Height;
				}
			}

			if (hasSwipeItemView)
			{
				var swipeItemsWidthSwipeThreshold = swipeItemsSize * 0.8f;

				return swipeItemsWidthSwipeThreshold;
			}
			else
			{
				if (_contentView != null)
				{
					var contentSize = isHorizontal ? _contentView.Width : _contentView.Height;
					var contentSizeSwipeThreshold = contentSize * 0.8f;

					return contentSizeSwipeThreshold;
				}
			}

			return SwipeViewExtensions.SwipeThreshold;
		}


		float ValidateSwipeThreshold(float swipeThreshold)
		{
			if (_contentView == null)
				return swipeThreshold;

			var contentHeight = (float)_context.FromPixels(_contentView.Height);
			var contentWidth = (float)_context.FromPixels(_contentView.Width);
			bool isHorizontal = IsHorizontalSwipe();

			if (isHorizontal)
			{
				if (swipeThreshold > contentWidth)
					swipeThreshold = contentWidth;

				return swipeThreshold;
			}

			if (swipeThreshold > contentHeight)
				swipeThreshold = contentHeight;

			return swipeThreshold;
		}

		Size GetSwipeItemSize(ISwipeItem swipeItem)
		{
			if (_contentView == null || Element == null)
				return Size.Zero;

			bool isHorizontal = IsHorizontalSwipe();
			var items = GetSwipeItemsByDirection();

			if (items == null)
				return Size.Zero;

			double threshold = Element.Threshold;
			double contentHeight = _context.FromPixels(_contentView.Height);
			double contentWidth = _context.FromPixels(_contentView.Width);

			if (isHorizontal)
			{
				if (swipeItem is ISwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);

					double swipeItemWidth;

					if (swipeItemViewSizeRequest.Width > 0)
						swipeItemWidth = threshold > swipeItemViewSizeRequest.Width ? threshold : (float)swipeItemViewSizeRequest.Width;
					else
						swipeItemWidth = threshold > SwipeViewExtensions.SwipeItemWidth ? threshold : SwipeViewExtensions.SwipeItemWidth;

					return new Size(swipeItemWidth, contentHeight);
				}

				if (swipeItem is ISwipeItem)
				{
					return new Size(items.Mode == SwipeMode.Execute ? (threshold > 0 ? threshold : contentWidth) / items.Count : (threshold < SwipeViewExtensions.SwipeItemWidth ? SwipeViewExtensions.SwipeItemWidth : threshold), contentHeight);
				}
			}
			else
			{
				if (swipeItem is ISwipeItemView verticalSwipeItemView)
				{
					var swipeItemViewSizeRequest = verticalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);

					double swipeItemHeight;

					if (swipeItemViewSizeRequest.Width > 0)
						swipeItemHeight = threshold > swipeItemViewSizeRequest.Height ? threshold : (float)swipeItemViewSizeRequest.Height;
					else
						swipeItemHeight = threshold > contentHeight ? threshold : contentHeight;

					return new Size(contentWidth / items.Count, swipeItemHeight);
				}

				if (swipeItem is ISwipeItem)
				{
					var swipeItemHeight = GetSwipeItemHeight();
					return new Size(contentWidth / items.Count, (threshold > 0 && threshold < swipeItemHeight) ? threshold : swipeItemHeight);
				}
			}

			return Size.Zero;
		}

		float GetSwipeItemHeight()
		{
			if (_contentView == null)
				return 0f;

			var contentHeight = (float)_context.FromPixels(_contentView.Height);
			var items = GetSwipeItemsByDirection();

			if (items == null)
				return 0f;

			if (items.Any(s => s is ISwipeItemView))
			{
				var itemsHeight = new List<float>();

				foreach (var swipeItem in items)
				{
					if (swipeItem is ISwipeItemView swipeItemView)
					{
						var swipeItemViewSizeRequest = swipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);
						itemsHeight.Add((float)swipeItemViewSizeRequest.Height);
					}
				}

				return itemsHeight.Max();
			}

			return contentHeight;
		}

		void ProcessTouchSwipeItems(APointF point)
		{
			if (_isResettingSwipe)
				return;

			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems == null || _actionView == null)
				return;

			for (int i = 0; i < _actionView.ChildCount; i++)
			{
				var swipeButton = _actionView.GetChildAt(i);

				if (swipeButton?.Visibility == ViewStates.Visible)
				{
					var swipeItemX = swipeButton.Left / _density;
					var swipeItemY = swipeButton.Top / _density;
					var swipeItemHeight = swipeButton.Height / _density;
					var swipeItemWidth = swipeButton.Width / _density;

					if (TouchInsideContent(swipeItemX, swipeItemY, swipeItemWidth, swipeItemHeight, point.X, point.Y))
					{
						var swipeItem = swipeItems[i];

						ExecuteSwipeItem(swipeItem);

						if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
							ResetSwipe();

						break;
					}
				}
			}
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

		void EnableParentGesture(bool isGestureEnabled)
		{
			_viewPagerParent?.EnableGesture = isGestureEnabled;
		}

		internal void OnOpenRequested(SwipeViewOpenRequest e)
		{
			if (_contentView == null)
				return;

			var openSwipeItem = e.OpenSwipeItem;
			var animated = e.Animated;

			UpdateIsOpen(true);
			ProgrammaticallyOpenSwipeItem(openSwipeItem, animated);
		}

		void ProgrammaticallyOpenSwipeItem(OpenSwipeItem openSwipeItem, bool animated)
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

			if (swipeItems == null || swipeItems.Count == 0)
				return;

			UpdateSwipeItems();

			var swipeThreshold = GetSwipeThreshold();
			UpdateOffset(swipeThreshold);

			Swipe(animated);

			_swipeOffset = Math.Abs(_swipeOffset);
		}

		void UpdateOffset(float swipeOffset)
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

		void UpdateIsOpen(bool isOpen)
		{
			if (Element == null)
				return;

			Element.IsOpen = isOpen;
		}

		internal void OnCloseRequested(SwipeViewCloseRequest e)
		{
			var animated = e.Animated;

			UpdateIsOpen(false);
			ResetSwipe(animated);
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
