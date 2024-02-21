using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.Common.Internal;
using Tizen.UIExtensions.NUI;
using GSize = Microsoft.Maui.Graphics.Size;
using NView = Tizen.NUI.BaseComponents.View;
using TPointStateType = Tizen.NUI.PointStateType;
using TRect = Tizen.UIExtensions.Common.Rect;

namespace Microsoft.Maui.Platform
{
	public class MauiSwipeView : ContentViewGroup, IAnimatable
	{
		const float OpenSwipeThresholdPercentage = 0.6f; // 60%
		const uint SwipeAnimationDuration = 200;

		IPlatformViewHandler? _contentHandler;
		NView? _contentView;
		NView? _actionView;

		bool _isSwiping;
		bool _isSwipeEnabled;
		bool _isResettingSwipe;
		bool _isOpen;
		double _swipeOffset;
		double _swipeThreshold;
		double _itemsWidth;
		double _itemsHeight;
		Point _initialPoint;
		SwipeDirection? _swipeDirection;
		OpenSwipeItem _previousOpenSwipeItem;
		SwipeTransitionMode _swipeTransitionMode;
		readonly Dictionary<ISwipeItem, NView> _swipeItems = new Dictionary<ISwipeItem, NView>();

		public MauiSwipeView(ISwipeView view) : base(view)
		{
			GrabTouchAfterLeave = true;
			ClippingMode = Tizen.NUI.ClippingModeType.ClipToBoundingBox;
			TouchEvent += OnTouchEvent;

			Element = view;
		}

		ISwipeView Element { get; }

		IMauiContext MauiContext => Element?.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null here");

		public void UpdateSwipeTransitionMode(SwipeTransitionMode swipeTransitionMode)
		{
			_swipeTransitionMode = swipeTransitionMode;
		}

		public void UpdateContent()
		{
			if (_contentView != null)
			{
				Children.Remove(_contentView);
				_contentView?.Unparent();
				_contentView?.Dispose();
				_contentView = null;
			}

			_contentHandler?.Dispose();
			_contentHandler = null;

			if (Element?.PresentedContent is IView view)
			{
				_contentView = view.ToPlatform(MauiContext);
				if (view.Handler is IPlatformViewHandler thandler)
				{
					_contentHandler = thandler;
				}
			}
			else
			{
				_contentView = CreateEmptyContent();
			}
			Children.Add(_contentView);
		}

		public void UpdateIsSwipeEnabled(bool isEnabled)
		{
			_isSwipeEnabled = isEnabled;
		}

		public void UpdateIsVisibleSwipeItem(ISwipeItem item)
		{
			if (!_isOpen)
				return;

			_swipeItems.TryGetValue(item, out NView? view);

			if (view != null)
			{
				_swipeThreshold = 0;
				LayoutSwipeItems(GetNativeSwipeItems());
				SwipeToThreshold(false);
			}
		}

		public void OnOpenRequested(SwipeViewOpenRequest e)
		{
			if (_contentView == null)
				return;

			var openSwipeItem = e.OpenSwipeItem;
			var animated = e.Animated;

			ProgrammaticallyOpenSwipeItem(openSwipeItem, animated);
		}

		public void OnCloseRequested(SwipeViewCloseRequest e)
		{
			var animated = e.Animated;

			ResetSwipe(animated);
		}

		bool OnTouchEvent(object source, TouchEventArgs e)
		{
			if (!_isSwipeEnabled)
				return false;

			var touchPosition = e.Touch.GetLocalPosition(0);
			var point = new Point(touchPosition.X.ToScaledDP(), touchPosition.Y.ToScaledDP());

			switch (e.Touch.GetState(0))
			{
				case TPointStateType.Down:
					return ProcessTouchDown(point);
				case TPointStateType.Motion:
					return ProcessTouchMove(point);
				case TPointStateType.Up:
				case TPointStateType.Interrupted:
					return ProcessTouchUp();
				default:
					return false;
			}
		}

		bool ProcessTouchDown(Point point)
		{
			if (_isSwiping || _contentView == null)
				return false;

			ResetSwipe(true);

			_initialPoint = point;
			return true;
		}

		bool ProcessTouchMove(Point point)
		{
			if (_contentView == null || _initialPoint.IsEmpty)
				return false;

			if (!_isOpen)
			{
				ResetSwipeToInitialPosition();
				_swipeDirection = SwipeDirectionHelper.GetSwipeDirection(_initialPoint, point);
				UpdateSwipeItems();
			}

			if (_isResettingSwipe || !ValidateSwipeDirection())
				return false;

			if (!_isSwiping)
			{
				RaiseSwipeStarted();
				_isSwiping = true;
			}

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
			if (!_isSwiping)
				return false;

			_isSwiping = false;

			RaiseSwipeEnded();

			if (_isResettingSwipe || !ValidateSwipeDirection())
				return false;

			ValidateSwipeThreshold();

			return false;
		}

		void ResetSwipeToInitialPosition()
		{
			_isResettingSwipe = false;
			_isSwiping = false;
			_swipeThreshold = 0;
			_swipeDirection = null;
			DisposeSwipeItems();
		}

		void DisposeSwipeItems()
		{
			foreach (var item in _swipeItems.Keys)
			{
				if (item.Handler is IPlatformViewHandler platformViewHandler)
				{
					platformViewHandler.Dispose();
				}
				else
				{
					item.Handler?.DisconnectHandler();
					_swipeItems[item].Unparent();
					_swipeItems[item].Dispose();
				}
				item.Handler = null;
			}
			_swipeItems.Clear();

			if (_actionView != null)
			{
				Children.Remove(_actionView);
				_actionView.Dispose();
				_actionView = null;
			}
			UpdateIsOpen(false);
		}

		void UpdateIsOpen(bool isOpen)
		{
			_isOpen = isOpen;
			Element.IsOpen = isOpen;
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

		static void UpdateSwipeItemViewLayout(ISwipeItemView swipeItemView)
		{
			swipeItemView?.Handler?.ToPlatform().InvalidateMeasure(swipeItemView);
		}

		void UpdateSwipeItems()
		{
			if (_contentView == null || _actionView != null)
				return;

			ISwipeItems? items = GetSwipeItemsByDirection();

			if (items?.Count == 0 || items == null)
				return;

			_actionView = new NView
			{
				Layout = new Tizen.NUI.AbsoluteLayout(),
				WidthSpecification = Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent,
				HeightSpecification = Tizen.NUI.BaseComponents.LayoutParamPolicies.MatchParent,
			};

			var swipeItems = new List<NView>();

			foreach (var item in items)
			{
				var swipeItem = item.ToPlatform(MauiContext);
				if (swipeItem != null)
				{
					if (item is ISwipeItemView formsSwipeItemView)
					{
						UpdateSwipeItemViewLayout(formsSwipeItemView);
					}

					_actionView.Add(swipeItem);
					_swipeItems.Add(item, swipeItem);
					swipeItems.Add(swipeItem);

					swipeItem.TouchEvent += (s, e) =>
					{
						if (e.Touch.GetState(0) == TPointStateType.Up)
						{
							ExecuteSwipeItem(item);
							if (GetSwipeItemsByDirection()?.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
								ResetSwipe();
						}

						return true;
					};
				}
			}

			Children.Add(_actionView);
			_contentView.RaiseToTop();

			LayoutSwipeItems(swipeItems);
		}

		void LayoutSwipeItems(List<NView> children)
		{
			if (_actionView == null || children == null || _contentView == null)
				return;

			_itemsWidth = 0;
			_itemsHeight = 0;

			var items = GetSwipeItemsByDirection();

			if (items == null || items.Count == 0)
				return;

			int i = 0;
			int previousWidth = 0;

			foreach (var child in children)
			{
				if (child.Visibility)
				{
					var item = items[i];
					var swipeItemSize = GetSwipeItemSize(item);

					var swipeItemHeight = swipeItemSize.Height.ToScaledPixel();
					var swipeItemWidth = swipeItemSize.Width.ToScaledPixel();
					TRect bound = new TRect();

					switch (_swipeDirection)
					{
						case SwipeDirection.Left:
							bound.X = _contentView.PositionX + _contentView.SizeWidth - (swipeItemWidth + previousWidth);
							bound.Y = _contentView.PositionY;
							bound.Width = swipeItemWidth;
							bound.Height = swipeItemHeight;
							break;
						case SwipeDirection.Right:
							bound.X = _contentView.PositionX + previousWidth;
							bound.Y = _contentView.PositionY;
							bound.Width = swipeItemWidth;
							bound.Height = swipeItemHeight;
							break;
						case SwipeDirection.Down:
							bound.X = _contentView.PositionX + previousWidth;
							bound.Y = _contentView.PositionY;
							bound.Width = swipeItemWidth;
							bound.Height = swipeItemHeight;
							break;
						case SwipeDirection.Up:
							bound.X = _contentView.PositionX + previousWidth;
							bound.Y = _contentView.PositionY + _contentView.SizeHeight - swipeItemHeight;
							bound.Width = swipeItemWidth;
							bound.Height = swipeItemHeight;
							break;
					}
					child.UpdateBounds(bound);

					i++;
					previousWidth += swipeItemWidth;
					_itemsHeight = Math.Max(_itemsHeight, swipeItemHeight);
				}
			}
			_itemsWidth = previousWidth;
		}

		List<NView> GetNativeSwipeItems()
		{
			var swipeItems = new List<NView>();

			if (_actionView == null)
				return swipeItems;

			for (int i = 0; i < _actionView.ChildCount; i++)
			{
				var view = _actionView.Children[i];
				if (view == null)
					continue;

				swipeItems.Add(view);
			}

			return swipeItems;
		}

		void Swipe()
		{
			if (_contentView == null)
				return;

			var offset = AdjustSwipeOffset(_swipeOffset.ToScaledPixel());

			var contentPosition = _contentHandler?.VirtualView?.Frame.ToPixel() ?? new TRect();

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
				case SwipeDirection.Right:
					{
						var translateX = offset;
						_contentView.PositionX = (float)(contentPosition.X + translateX);
						break;
					}
				case SwipeDirection.Up:
				case SwipeDirection.Down:
					{
						var translateY = offset;
						_contentView.PositionY = (float)(contentPosition.Y + translateY);
						break;
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

		void ResetSwipe(bool animated = true)
		{
			if (_contentView == null)
				return;

			_isResettingSwipe = true;
			_isSwiping = false;
			_swipeThreshold = 0;

			var contentPosition = _contentHandler?.VirtualView?.Frame.ToPixel() ?? new TRect();

			if (animated)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:
						{
							_swipeDirection = null;
							var diffX = _contentView.PositionX - contentPosition.X;
							new Animation(v =>
							{
								_contentView.PositionX = (float)(contentPosition.X + diffX - diffX * v);

							}).Commit(this, "Swipe", (uint)SwipeAnimationDuration, finished: (v, r) =>
							{
								if (_swipeDirection == null)
									DisposeSwipeItems();

								_isResettingSwipe = false;
							});

							break;
						}
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						{
							_swipeDirection = null;
							var diffY = _contentView.PositionY - contentPosition.Y;
							new Animation(v =>
							{
								_contentView.PositionY = (float)(contentPosition.Y + diffY - diffY * v);

							}).Commit(this, "Swipe", (uint)SwipeAnimationDuration, finished: (v, r) =>
							{
								if (_swipeDirection == null)
									DisposeSwipeItems();

								_isResettingSwipe = false;
							});
							break;
						}
					default:
						_isResettingSwipe = false;
						break;
				}
			}
			else
			{
				_contentView.PositionX = (float)contentPosition.X;
				_contentView.PositionY = (float)contentPosition.Y;

				DisposeSwipeItems();
				_isResettingSwipe = false;
			}
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
			SwipeToThreshold(animated);
		}

		void SwipeToThreshold(bool animated = true)
		{
			var completeAnimationDuration = animated ? SwipeAnimationDuration : 0;
			UpdateOffset(GetSwipeThreshold());
			float swipeThreshold = _swipeOffset.ToScaledPixel();

			var contentPosition = _contentHandler?.VirtualView?.Frame.ToPixel() ?? new TRect();

			if (_contentView != null)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:
						{
							var target = swipeThreshold + contentPosition.X;
							var init = _contentView.PositionX;
							var diff = init - (float)target;
							new Animation(v =>
							{
								_contentView.PositionX = init - diff * (float)v;
							}).Commit(this, "Swipe", length: completeAnimationDuration, finished: (v, r) =>
							{
								_isSwiping = false;
							});
							break;
						}
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						{
							var target = swipeThreshold + contentPosition.Y;
							var init = _contentView.PositionY;
							var diff = init - (float)target;
							new Animation(v =>
							{
								_contentView.PositionY = init - diff * (float)v;
							}).Commit(this, "Swipe", length: completeAnimationDuration, finished: (v, r) =>
							{
								_isSwiping = false;
							});
							break;
						}
				}
			}
		}

		GSize GetSwipeItemSize(ISwipeItem swipeItem)
		{
			if (_contentView == null || Element == null)
				return GSize.Zero;

			bool isHorizontal = IsHorizontalSwipe();
			var items = GetSwipeItemsByDirection();

			if (items == null)
				return GSize.Zero;

			double threshold = Element.Threshold;
			double contentHeight = _contentView.SizeHeight.ToScaledDP();
			double contentWidth = _contentView.SizeWidth.ToScaledDP();

			if (isHorizontal)
			{
				if (swipeItem is ISwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);
					double swipeItemWidth = swipeItemViewSizeRequest.Width > 0 ? swipeItemViewSizeRequest.Width : SwipeViewExtensions.SwipeItemWidth;
					swipeItemWidth = Math.Max(threshold, swipeItemWidth);
					return new GSize(swipeItemWidth, contentHeight);
				}
				else if (swipeItem is ISwipeItem)
				{
					return new GSize(items.Mode == SwipeMode.Execute ? (threshold > 0 ? threshold : contentWidth) / items.Count : Math.Max(threshold, SwipeViewExtensions.SwipeItemWidth), contentHeight);
				}
			}
			else
			{
				if (swipeItem is ISwipeItemView verticalSwipeItemView)
				{
					var swipeItemViewSizeRequest = verticalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);
					double swipeItemHeight = swipeItemViewSizeRequest.Height > 0 ? swipeItemViewSizeRequest.Height : contentHeight;
					swipeItemHeight = Math.Max(threshold, swipeItemHeight);
					return new GSize(contentWidth / items.Count, swipeItemHeight);
				}
				else if (swipeItem is ISwipeItem)
				{
					var swipeItemHeight = GetSwipeItemHeight();
					return new GSize(contentWidth / items.Count, (threshold > 0 && threshold < swipeItemHeight) ? threshold : swipeItemHeight);
				}
			}

			return GSize.Zero;
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

		float GetSwipeItemHeight()
		{
			if (_contentView == null)
				return 0f;

			var contentHeight = _contentView.SizeHeight.ToScaledDP();
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

		bool ValidateSwipeDirection()
		{
			if (_swipeDirection == null)
				return false;

			var swipeItems = GetSwipeItemsByDirection();
			return IsValidSwipeItems(swipeItems);
		}

		static bool IsValidSwipeItems(ISwipeItems? swipeItems)
		{
			return swipeItems != null && swipeItems.Where(s => GetIsVisible(s)).Any();
		}

		static bool GetIsVisible(ISwipeItem swipeItem)
		{
			if (swipeItem is IView view)
				return view.Visibility == Maui.Visibility.Visible;
			else if (swipeItem is ISwipeItemMenuItem menuItem)
				return menuItem.Visibility == Maui.Visibility.Visible;

			return true;
		}

		double GetSwipeOffset(Point initialPoint, Point endPoint)
		{
			double swipeOffset = 0;

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
				case SwipeDirection.Right:
					swipeOffset = (int)endPoint.X - (int)initialPoint.X;
					break;
				case SwipeDirection.Up:
				case SwipeDirection.Down:
					swipeOffset = (int)endPoint.Y - (int)initialPoint.Y;
					break;
			}

			if (swipeOffset == 0)
				swipeOffset = GetSwipeContentOffset();

			return swipeOffset;
		}

		double GetSwipeContentOffset()
		{
			double swipeOffset = 0;

			if (_contentView != null)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:
						swipeOffset = _contentView.PositionX.ToScaledDP();
						break;
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						swipeOffset = _contentView.PositionY.ToScaledDP();
						break;
				}
			}
			return swipeOffset;
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
				return 0f;

			double threshold = Element.Threshold;

			if (threshold > 0)
				return (float)threshold;

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

		double CalculateSwipeThreshold()
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
					var contentWidth = _contentView.SizeWidth.ToScaledDP();
					var contentWidthSwipeThreshold = contentWidth * 0.8f;

					return contentWidthSwipeThreshold;
				}
			}

			return SwipeViewExtensions.SwipeThreshold;
		}

		double ValidateSwipeThreshold(double swipeThreshold)
		{
			if (_contentView == null)
				return swipeThreshold;

			var contentHeight = _contentView.SizeHeight.ToScaledDP();
			var contentWidth = _contentView.SizeWidth.ToScaledDP();
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

		int AdjustSwipeOffset(int offset)
		{
			if (_swipeDirection == null)
				return offset;

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
					offset = Math.Min(offset, 0);
					offset = Math.Max(offset, (int)-_itemsWidth);
					break;
				case SwipeDirection.Right:
					offset = Math.Max(offset, 0);
					offset = Math.Min(offset, (int)_itemsWidth);
					break;
				case SwipeDirection.Up:
					offset = Math.Min(offset, 0);
					offset = Math.Max(offset, (int)-_itemsHeight);
					break;
				case SwipeDirection.Down:
					offset = Math.Max(offset, 0);
					offset = Math.Min(offset, (int)_itemsHeight);
					break;
			}
			return offset;
		}

		static NView CreateEmptyContent()
		{
			return new NView
			{
				BackgroundColor = Tizen.NUI.Color.Transparent
			};
		}

		public void BatchBegin() { }

		public void BatchCommit() { }
	}
}