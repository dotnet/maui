using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	public class MauiSwipeView : UserControl, IDisposable
	{
		const double OpenSwipeThresholdPercentage = 0.6f; // 60%

		readonly Dictionary<ISwipeItem, object> _swipeItems;

		readonly Panel? _container;
		FrameworkElement? _content;
		Panel? _actionView;

		Point? _initialPoint;
		bool _isTouchDown;
		bool _isSwiping;
		SwipeDirection? _swipeDirection;
		bool _isResettingSwipe;
		bool _isOpen;
		double _swipeOffset;
		double _swipeThreshold;
		OpenSwipeItem _previousOpenSwipeItem;
		bool _isSwipeEnabled;

		public MauiSwipeView()
		{
			_swipeItems = new Dictionary<ISwipeItem, object>();

			_container = new Grid();
			Content = _container;

			ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
			ManipulationStarted += OnSwipeControlManipulationStarted;
			ManipulationDelta += OnSwipeControlManipulationDelta;
			ManipulationCompleted += OnSwipeControlManipulationCompleted;
			Tapped += OnSwipeViewTapped;
		}

		internal ISwipeView? Element { get; private set; }
		internal IMauiContext MauiContext => Element?.Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null here");

		protected override Size ArrangeOverride(Size finalSize)
		{
			Clip = new RectangleGeometry { Rect = new Rect(0, 0, finalSize.Width, finalSize.Height) };

			return base.ArrangeOverride(finalSize);
		}

		public void Dispose()
		{
			ManipulationStarted -= OnSwipeControlManipulationStarted;
			ManipulationDelta -= OnSwipeControlManipulationDelta;
			ManipulationCompleted -= OnSwipeControlManipulationCompleted;
			Tapped += OnSwipeViewTapped;

			if (_content is not null)
				_content.Tapped -= OnContentTapped;
		}

		internal void SetElement(ISwipeView swipeView)
		{
			Element = swipeView;
		}

		internal void OnCloseRequested(SwipeViewCloseRequest e) 
			=> ResetSwipe(); 
		
		internal void OnOpenRequested(SwipeViewOpenRequest e)
		{
			if (_content is null)
				return;

			var openSwipeItem = e.OpenSwipeItem;

			ProgrammaticallyOpenSwipeItem(openSwipeItem);
		}

		internal void UpdateContent()
		{
			if (_content is not null)
			{
				_content.Tapped -= OnContentTapped;
				_container?.Children.Remove(_content);
				_content = null;
			}

			if (Element?.PresentedContent is IView view)
				_content = view.ToPlatform(MauiContext);

			if (_content is not null)
			{
				_content.Tapped += OnContentTapped;
				_container?.Children.Add(_content);
			}
		}

		internal void UpdateIsVisibleSwipeItem(ISwipeItem item)
		{
			if (!_isOpen)
				return;

			_swipeItems.TryGetValue(item, out object? view);

			if (view is not null && view is FrameworkElement)
			{
				_swipeThreshold = 0;
				LayoutSwipeItems(GetNativeSwipeItems());
				SwipeToThreshold();
			}
		}

		internal void UpdateIsSwipeEnabled(bool isEnabled)
		{
			_isSwipeEnabled = isEnabled;
		}

		List<FrameworkElement> GetNativeSwipeItems()
		{
			var swipeItems = new List<FrameworkElement>();

			if (_actionView is null)
				return swipeItems;

			foreach (var children in _actionView.Children)
			{
				if (children is FrameworkElement swipeItem)
					swipeItems.Add(swipeItem);
			}

			return swipeItems;
		}

		void OnSwipeControlManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			if (_isSwiping || _isTouchDown || _content is null)
				return;

			_initialPoint = e.Position;
			_isTouchDown = true;
		}

		void OnSwipeControlManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			if (!_isSwipeEnabled)
				return;

			if (_content is null || _initialPoint is null)
				return;

			var point = e.Position;

			if (!_isOpen)
			{
				ResetSwipeToInitialPosition();

				_swipeDirection = SwipeDirectionHelper.GetSwipeDirection(new Graphics.Point(_initialPoint.Value.X, _initialPoint.Value.Y), new Graphics.Point(point.X, point.Y));

				UpdateSwipeItems();
			}

			if (!_isSwiping)
			{
				RaiseSwipeStarted();
				_isSwiping = true;
			}

			if (!ValidateSwipeDirection() || _isResettingSwipe)
				return;

			_swipeOffset = GetSwipeOffset(_initialPoint.Value, point);
			UpdateIsOpen(_swipeOffset != 0);

			if (Math.Abs(_swipeOffset) > double.Epsilon)
				Swipe();

			RaiseSwipeChanging();
		}

		void OnSwipeControlManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			_isTouchDown = false;

			if (!_isSwiping)
				return;

			_isSwiping = false;

			RaiseSwipeEnded();

			if (_isResettingSwipe || !ValidateSwipeDirection())
				return;

			ValidateSwipeThreshold();
		}

		void OnSwipeViewTapped(object sender, TappedRoutedEventArgs e)
		{
			var point = e.GetPosition(this);

			if (CanProcessTouchSwipeItems(point))
				ProcessTouchSwipeItems(point);
		}

		void OnContentTapped(object sender, TappedRoutedEventArgs e)
		{
			ResetSwipe();
		}

		void ResetSwipeToInitialPosition()
		{
			_isResettingSwipe = false;
			_isSwiping = false;
			_swipeThreshold = 0;
			_swipeDirection = null;
			DisposeSwipeItems();
		}

		void ResetSwipe()
		{
			if (_content is null)
				return;

			_isResettingSwipe = true;
			_isSwiping = false;
			_swipeThreshold = 0;

			var translateTransform = _content.RenderTransform as TranslateTransform;

			if (translateTransform is not null)
			{
				translateTransform.X = 0;
				translateTransform.Y = 0;
			}

			DisposeSwipeItems();
			_isResettingSwipe = false;
		}

		void DisposeSwipeItems()
		{
			_isOpen = false;

			foreach (var item in _swipeItems.Keys)
			{
				item.Handler?.DisconnectHandler();
			}

			_swipeItems.Clear();

			if (_actionView is not null)
			{
				_container?.Children.Remove(_actionView);
				_actionView = null;
			}

			UpdateIsOpen(false);
		}

		void UpdateSwipeItems()
		{
			if (_content is null || _actionView is not null)
				return;

			ISwipeItems? items = GetSwipeItemsByDirection();

			if (items?.Count == 0 || items is null)
				return;

			_actionView = new Canvas();

			var swipeItems = new List<FrameworkElement>();

			foreach (var item in items)
			{
				FrameworkElement swipeItem = item.ToPlatform(MauiContext);

				if (item is ISwipeItemView swipeItemView)
				{
					_actionView.Children.Add(swipeItem);
					UpdateSwipeItemViewLayout(swipeItemView);
					_swipeItems.Add(swipeItemView, swipeItem);
				}
				else if (item is ISwipeItemMenuItem swipeItemMenuItem)
				{
					_actionView.Children.Add(swipeItem);
					_swipeItems.Add(swipeItemMenuItem, swipeItem);
				}

				if (swipeItem is not null)
					swipeItems.Add(swipeItem);
			}

			_container?.Children.Add(_actionView);

			if (_content is not null)
			{
				int zIndex = Canvas.GetZIndex(_actionView);
				Canvas.SetZIndex(_content, zIndex + 1);

				double contentHeight = _content.ActualHeight;
				double contentWidth = _content.ActualWidth;

				_actionView.Height = contentHeight;
				_actionView.Width = contentWidth;
			}

			LayoutSwipeItems(swipeItems);
			swipeItems.Clear();
		}

		void UpdateSwipeItemViewLayout(ISwipeItemView swipeItemView)
		{
			if (swipeItemView?.Handler is not IPlatformViewHandler handler)
				return;

			var swipeItemSize = GetSwipeItemSize(swipeItemView);
			handler.LayoutVirtualView(new Size(swipeItemSize.Width, swipeItemSize.Height));
		}

		void LayoutSwipeItems(List<FrameworkElement> childs)
		{
			if (_actionView is null || childs is null || _content is null)
				return;

			var items = GetSwipeItemsByDirection();

			if (items is null || items.Count == 0)
				return;

			int i = 0;
			double previousWidth = 0;

			foreach (var child in childs)
			{
				if (child.Visibility == UI.Xaml.Visibility.Visible)
				{
					var item = items[i];
					var swipeItemSize = GetSwipeItemSize(item);

					double contentWidth = _content.ActualWidth;
					double contentHeight = _content.ActualHeight;

					double swipeItemHeight = swipeItemSize.Height;
					double swipeItemWidth = swipeItemSize.Width;

					double left, top;
					switch (_swipeDirection)
					{
						case SwipeDirection.Left:
							left = contentWidth - previousWidth - swipeItemWidth;
							top = 0;
							break;
						case SwipeDirection.Right:
						case SwipeDirection.Down:
							left = previousWidth;
							top = 0;
							break;
						default:
							left = previousWidth;
							top = contentHeight - swipeItemHeight;
	
							break;
					}
					
					Canvas.SetLeft(child, left);
					Canvas.SetTop(child, top);
					child.Height = swipeItemHeight;
					child.Width = swipeItemWidth;

					i++;
					previousWidth += swipeItemWidth;
				}
			}
		}

		void Swipe()
		{
			if (_content is null)
				return;

			var offset = ValidateSwipeOffset(_swipeOffset);
			_isOpen = offset != 0;

			var translateTransform = _content.RenderTransform as TranslateTransform;
			if (translateTransform is null)
				translateTransform = new TranslateTransform();
			_content.RenderTransform = translateTransform;

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
				case SwipeDirection.Right:
					translateTransform.X = offset;
					break;
				case SwipeDirection.Up:
				case SwipeDirection.Down:
					translateTransform.Y = offset;
					break;
			}
		}

		bool ValidateSwipeDirection()
		{
			if (_swipeDirection is null)
				return false;

			var swipeItems = GetSwipeItemsByDirection();
			return IsValidSwipeItems(swipeItems);
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

		bool IsValidSwipeItems(ISwipeItems? swipeItems)
		{
			return swipeItems is not null && swipeItems.Any(GetIsVisible);
		}

		bool GetIsVisible(ISwipeItem swipeItem)
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

		double GetSwipeContentOffset()
		{
			double swipeOffset = 0;

			if (_content is not null)
			{
				var transform = _content.RenderTransform as UI.Xaml.Media.TranslateTransform;

				if (transform is null)
					return swipeOffset;

				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:
						swipeOffset = transform.X;
						break;
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						swipeOffset = transform.Y;
						break;
				}
			}

			return swipeOffset;
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

		double GetSwipeThreshold()
		{
			if (Math.Abs(_swipeThreshold) > double.Epsilon)
				return _swipeThreshold;

			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems is null)
				return 0;

			_swipeThreshold = GetSwipeThreshold(swipeItems);

			return _swipeThreshold;
		}

		double GetSwipeThreshold(ISwipeItems swipeItems)
		{
			if (Element is null)
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
			if (swipeItems is null)
				return SwipeViewExtensions.SwipeThreshold;

			double swipeItemsHeight = 0;
			double swipeItemsWidth = 0;
			bool useSwipeItemsSize = false;

			foreach (var swipeItem in swipeItems)
			{
				if (swipeItem is ISwipeItemView)
					useSwipeItemsSize = true;

				if (GetIsVisible(swipeItem))
				{
					var swipeItemSize = GetSwipeItemSize(swipeItem);
					swipeItemsHeight += swipeItemSize.Height;
					swipeItemsWidth += swipeItemSize.Width;
				}
			}

			if (useSwipeItemsSize)
			{
				var isHorizontalSwipe = IsHorizontalSwipe();

				return isHorizontalSwipe ? swipeItemsWidth : swipeItemsHeight;
			}
			else
			{
				if (_content is not null)
				{
					var contentWidth = _content.ActualWidth;
					var contentWidthSwipeThreshold = contentWidth * 0.8f;

					return contentWidthSwipeThreshold;
				}
			}

			return SwipeViewExtensions.SwipeThreshold;
		}

		void ValidateSwipeThreshold()
		{
			if (_swipeDirection is null)
				return;

			var swipeThresholdPercent = OpenSwipeThresholdPercentage * GetSwipeThreshold();

			if (Math.Abs(_swipeOffset) >= swipeThresholdPercent)
			{
				var swipeItems = GetSwipeItemsByDirection();

				if (swipeItems is null)
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
				{
					SwipeToThreshold();
				}
			}
			else
				ResetSwipe();
		}

		double ValidateSwipeThreshold(double swipeThreshold)
		{
			if (_content is null)
				return swipeThreshold;

			var contentHeight = _content.ActualHeight;
			var contentWidth = _content.ActualWidth;

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

		void SwipeToThreshold()
		{
			if (_content is null)
				return;

			_swipeOffset = GetSwipeThreshold();
			var swipeThreshold = _swipeOffset;

			var translateTransform = _content.RenderTransform as TranslateTransform;
			if (translateTransform is null)
				translateTransform = new TranslateTransform();
			_content.RenderTransform = translateTransform;

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
					translateTransform.X = -swipeThreshold;
					break;
				case SwipeDirection.Right:
					translateTransform.X = swipeThreshold;
					break;
				case SwipeDirection.Up:
					translateTransform.Y = -swipeThreshold;
					break;
				case SwipeDirection.Down:
					translateTransform.Y = swipeThreshold;
					break;
			}
		}

		bool IsHorizontalSwipe()
		{
			return _swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right;
		}

		Graphics.Size GetSwipeItemSize(ISwipeItem swipeItem)
		{
			if (_content is null || Element is null)
				return Graphics.Size.Zero;

			bool isHorizontal = IsHorizontalSwipe();
			var items = GetSwipeItemsByDirection();

			if (items is null)
				return Graphics.Size.Zero;

			double threshold = Element.Threshold;
			double contentHeight = _content.ActualHeight;
			double contentWidth = _content.ActualWidth;

			if (isHorizontal)
			{
				if (swipeItem is ISwipeItem)
				{
					return new Graphics.Size(items.Mode == SwipeMode.Execute ? (threshold > 0 ? threshold : contentWidth) / items.Count : (threshold < SwipeViewExtensions.SwipeItemWidth ? SwipeViewExtensions.SwipeItemWidth : threshold), contentHeight);
				}

				if (swipeItem is ISwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);

					double swipeItemWidth;

					if (swipeItemViewSizeRequest.Width > 0)
						swipeItemWidth = threshold > swipeItemViewSizeRequest.Width ? threshold : (float)swipeItemViewSizeRequest.Width;
					else
						swipeItemWidth = threshold > SwipeViewExtensions.SwipeItemWidth ? threshold : SwipeViewExtensions.SwipeItemWidth;

					return new Graphics.Size(swipeItemWidth, contentHeight);
				}
			}
			else
			{
				if (swipeItem is ISwipeItem)
				{
					var swipeItemHeight = GetSwipeItemHeight();
					return new Graphics.Size(contentWidth / items.Count, (threshold > 0 && threshold < swipeItemHeight) ? threshold : swipeItemHeight);
				}

				if (swipeItem is ISwipeItemView verticalSwipeItemView)
				{
					var swipeItemViewSizeRequest = verticalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity);

					double swipeItemHeight;

					if (swipeItemViewSizeRequest.Width > 0)
						swipeItemHeight = threshold > swipeItemViewSizeRequest.Height ? threshold : (float)swipeItemViewSizeRequest.Height;
					else
						swipeItemHeight = threshold > contentHeight ? threshold : contentHeight;

					return new Graphics.Size(contentWidth / items.Count, swipeItemHeight);
				}
			}

			return Graphics.Size.Zero;
		}

		double GetSwipeItemHeight()
		{
			if (_content is null)
				return 0f;

			var contentHeight = _content.ActualHeight;
			var items = GetSwipeItemsByDirection();

			if (items is null)
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

		bool CanProcessTouchSwipeItems(Point point)
		{
			// We only invoke the SwipeItem command if we tap on the SwipeItems area
			// and the SwipeView is fully open.

			//if (TouchInsideContent(point))
			//	return false;

			if (_swipeOffset == _swipeThreshold)
				return true;

			return false;
		}

		void ProcessTouchSwipeItems(Point point)
		{
			if (_isResettingSwipe)
				return;

			var swipeItems = GetSwipeItemsByDirection();

			if (swipeItems is null || _actionView is null)
				return;

			foreach(var child in _actionView.Children)
			{
				var button = child as FrameworkElement;

				if (button is not null && button.Visibility == UI.Xaml.Visibility.Visible)
				{
					var swipeItemX = Canvas.GetLeft(button);
					var swipeItemY = Canvas.GetTop(button);
					var swipeItemHeight = button.ActualHeight;
					var swipeItemWidth = button.ActualWidth;

					if (TouchInsideContent(swipeItemX, swipeItemY, swipeItemWidth, swipeItemHeight, point.X, point.Y))
					{
						if (button is ISwipeItem swipeItem)
							ExecuteSwipeItem(swipeItem);

						if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
							ResetSwipe();

						break;
					}
				}
			}
		}

		bool TouchInsideContent(double x1, double y1, double x2, double y2, double x, double y)
		{
			if (x > x1 && x < (x1 + x2) && y > y1 && y < (y1 + y2))
				return true;

			return false;
		}

		void ExecuteSwipeItem(ISwipeItem item)
		{
			if (item is null)
				return;

			bool isEnabled = true;

			if (item is ISwipeItemMenuItem swipeItem)
				isEnabled = swipeItem.IsEnabled;

			if (item is ISwipeItemView swipeItemView)
				isEnabled = swipeItemView.IsEnabled;

			if (isEnabled)
				item.OnInvoked();
		}

		void ProgrammaticallyOpenSwipeItem(OpenSwipeItem openSwipeItem)
		{
			if (_isOpen)
			{
				if (_previousOpenSwipeItem == openSwipeItem)
					return;

				ResetSwipe();
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

			if (swipeItems is null || swipeItems.Count == 0)
				return;

			var swipeThreshold = GetSwipeThreshold();
			UpdateOffset(swipeThreshold);

			UpdateSwipeItems();
			Swipe();

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

		void UpdateIsOpen(bool isOpen)
		{
			if (Element is null)
				return;

			Element.IsOpen = isOpen;
		}

		void RaiseSwipeStarted()
		{
			if (_swipeDirection is null || !ValidateSwipeDirection())
				return;

			Element?.SwipeStarted(new SwipeViewSwipeStarted(_swipeDirection.Value));
		}

		void RaiseSwipeChanging()
		{
			if (_swipeDirection is null)
				return;

			Element?.SwipeChanging(new SwipeViewSwipeChanging(_swipeDirection.Value, _swipeOffset));
		}

		void RaiseSwipeEnded()
		{
			if (_swipeDirection is null || !ValidateSwipeDirection())
				return;

			bool isOpen = false;

			var swipeThresholdPercent = OpenSwipeThresholdPercentage * GetSwipeThreshold();

			if (Math.Abs(_swipeOffset) >= swipeThresholdPercent)
				isOpen = true;

			Element?.SwipeEnded(new SwipeViewSwipeEnded(_swipeDirection.Value, isOpen));
		}
	}
}