using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AButton = AndroidX.AppCompat.Widget.AppCompatButton;
using APointF = Android.Graphics.PointF;
using ATextAlignment = Android.Views.TextAlignment;
using AView = Android.Views.View;
using Specifics = Xamarin.Forms.PlatformConfiguration.AndroidSpecific.SwipeView;

namespace Xamarin.Forms.Platform.Android
{
	public class SwipeViewRenderer : ViewRenderer<SwipeView, AView>
	{
		const float OpenSwipeThresholdPercentage = 0.6f; // 60%
		const int SwipeThreshold = 250;
		const int SwipeItemWidth = 100;
		const long SwipeAnimationDuration = 200;
		const float SwipeMinimumDelta = 10f;

		readonly Dictionary<ISwipeItem, object> _swipeItems;
		readonly Context _context;
		View _scrollParent;
		FormsViewPager _viewPagerParent;
		AView _contentView;
		LinearLayoutCompat _actionView;
		SwipeTransitionMode _swipeTransitionMode;
		float _downX;
		float _downY;
		float _density;
		bool _isTouchDown;
		bool _isSwiping;
		APointF _initialPoint;
		SwipeDirection? _swipeDirection;
		float _swipeOffset;
		float _swipeThreshold;
		double _previousScrollX;
		double _previousScrollY;
		bool _isSwipeEnabled;
		bool _isResettingSwipe;
		bool _isOpen;
		bool _isDisposed;

		public SwipeViewRenderer(Context context) : base(context)
		{
			_context = context;

			_swipeItems = new Dictionary<ISwipeItem, object>();

			this.SetClipToOutline(true);

			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SwipeView> e)
		{
			if (e.NewElement != null)
			{
				e.NewElement.OpenRequested += OnOpenRequested;
				e.NewElement.CloseRequested += OnCloseRequested;

				if (Control == null)
				{
					_density = Resources.DisplayMetrics.Density;

					SetNativeControl(CreateNativeControl());
				}

				UpdateContent();
				UpdateIsSwipeEnabled();
				UpdateSwipeTransitionMode();
			}

			if (e.OldElement != null)
			{
				e.NewElement.OpenRequested -= OnOpenRequested;
				e.OldElement.CloseRequested -= OnCloseRequested;
			}

			base.OnElementChanged(e);
		}

		protected override AView CreateNativeControl()
		{
			return new AView(_context);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ContentView.ContentProperty.PropertyName)
				UpdateContent();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsSwipeEnabled();
			else if (e.PropertyName == Specifics.SwipeTransitionModeProperty.PropertyName)
				UpdateSwipeTransitionMode();
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);

			var width = r - l;
			var height = b - t;

			var pixelWidth = _context.FromPixels(width);
			var pixelHeight = _context.FromPixels(height);

			if (changed)
			{
				if (Element.Content != null)
					Element.Content.Layout(new Rectangle(0, 0, pixelWidth, pixelHeight));

				_contentView?.Layout(0, 0, width, height);
			}
		}

		protected override Size MinimumSize()
		{
			return new Size(40, 40);
		}

		protected override void UpdateBackgroundColor()
		{
			if (Element.BackgroundColor != Color.Default)
				SetBackgroundColor(Element.BackgroundColor.ToAndroid());
			else
				Control?.SetWindowBackground();
		}

		protected override void UpdateBackground()
		{
			Brush background = Element.Background;

			if (Brush.IsNullOrEmpty(background))
				return;

			this.UpdateBackground(background);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (Control != null && Control.Parent != null && _viewPagerParent == null)
				_viewPagerParent = Control.Parent.GetParentOfType<FormsViewPager>();

			if (Element != null && _scrollParent == null)
			{
				_scrollParent = Element.FindParentOfType<ScrollView>();

				if (_scrollParent is ScrollView scrollView)
				{
					scrollView.Scrolled += OnParentScrolled;
					return;
				}

				_scrollParent = Element.FindParentOfType<ListView>();

				if (_scrollParent is ListView listView)
				{
					listView.Scrolled += OnParentScrolled;
					return;
				}

				_scrollParent = Element.FindParentOfType<Xamarin.Forms.CollectionView>();

				if (_scrollParent is Xamarin.Forms.CollectionView collectionView)
				{
					collectionView.Scrolled += OnParentScrolled;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Element != null)
				{
					Element.OpenRequested -= OnOpenRequested;
					Element.CloseRequested -= OnCloseRequested;
				}

				if (_scrollParent != null)
				{
					if (_scrollParent is ScrollView scrollView)
						scrollView.Scrolled -= OnParentScrolled;

					if (_scrollParent is ListView listView)
						listView.Scrolled -= OnParentScrolled;

					if (_scrollParent is Xamarin.Forms.CollectionView collectionView)
						collectionView.Scrolled -= OnParentScrolled;
				}

				if (_contentView != null)
				{
					_contentView.RemoveFromParent();
					_contentView.Dispose();
					_contentView = null;
				}

				if (_actionView != null)
				{
					_actionView.RemoveFromParent();
					_actionView.Dispose();
					_actionView = null;
				}

				if (_initialPoint != null)
				{
					_initialPoint.Dispose();
					_initialPoint = null;
				}
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			base.OnTouchEvent(e);

			if (e.Action == MotionEventActions.Move && !ShouldInterceptTouch(e))
				return true;

			ProcessSwipingInteractions(e);

			return true;
		}

		bool ShouldInterceptTouch(MotionEvent e)
		{
			if (_initialPoint == null)
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

			if (items == null || items.Count == 0)
				return false;

			return true;
		}

		public override bool OnInterceptTouchEvent(MotionEvent e)
		{
			return ShouldInterceptTouch(e);
		}

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			if (e.Action == MotionEventActions.Down)
			{
				_downX = e.RawX;
				_downY = e.RawY;
				_initialPoint = new APointF(e.GetX() / _density, e.GetY() / _density);
			}

			if (e.Action == MotionEventActions.Up)
			{
				var touchUpPoint = new APointF(e.GetX() / _density, e.GetY() / _density);

				if (CanProcessTouchSwipeItems(touchUpPoint))
					ProcessTouchSwipeItems(touchUpPoint);
				else
				{
					if (!_isSwiping && _isOpen && TouchInsideContent(touchUpPoint))
						ResetSwipe();

					PropagateParentTouch();
				}
			}

			return base.DispatchTouchEvent(e);
		}

		void PropagateParentTouch()
		{
			var itemContentView = _contentView.Parent.GetParentOfType<ItemContentView>();

			// If the SwipeView container is ItemContentView we are using SwipeView with a CollectionView or CarouselView.
			// When doing touch up, if the SwipeView is closed, we propagate the Touch to the parent. In this way, the parent
			// element will manage the touch (SelectionChanged, etc.).
			if (itemContentView != null && !((ISwipeViewController)Element).IsOpen)
				itemContentView.ClickOn();
		}

		void UpdateContent()
		{
			if (Element.Content == null)
				_contentView = CreateEmptyContent();
			else
				_contentView = CreateContent();

			AddView(_contentView, new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));
		}

		AView CreateEmptyContent()
		{
			var emptyContentView = new AView(_context);
			emptyContentView.SetBackgroundColor(Color.Default.ToAndroid());

			return emptyContentView;
		}

		AView CreateContent()
		{
			var renderer = Platform.CreateRenderer(Element.Content, _context);
			Platform.SetRenderer(Element.Content, renderer);

			return renderer?.View;
		}

		SwipeItems GetSwipeItemsByDirection()
		{
			if (_swipeDirection.HasValue)
				return GetSwipeItemsByDirection(_swipeDirection.Value);

			return null;
		}

		SwipeItems GetSwipeItemsByDirection(SwipeDirection swipeDirection)
		{
			SwipeItems swipeItems = null;

			switch (swipeDirection)
			{
				case SwipeDirection.Left:
					swipeItems = Element.RightItems;
					break;
				case SwipeDirection.Right:
					swipeItems = Element.LeftItems;
					break;
				case SwipeDirection.Up:
					swipeItems = Element.BottomItems;
					break;
				case SwipeDirection.Down:
					swipeItems = Element.TopItems;
					break;
			}

			return swipeItems;
		}

		bool HasSwipeItems()
		{
			return Element != null && (IsValidSwipeItems(Element.LeftItems) || IsValidSwipeItems(Element.RightItems) || IsValidSwipeItems(Element.TopItems) || IsValidSwipeItems(Element.BottomItems));
		}

		bool IsHorizontalSwipe()
		{
			return _swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right;
		}

		bool IsValidSwipeItems(SwipeItems swipeItems)
		{
			return swipeItems != null && swipeItems.Where(s => s.IsVisible).Count() > 0;
		}

		bool ProcessSwipingInteractions(MotionEvent e)
		{
			bool? handled = true;
			var point = new APointF(e.GetX() / _density, e.GetY() / _density);

			switch (e.Action)
			{
				case MotionEventActions.Down:
					_downX = e.RawX;
					_downY = e.RawY;

					handled = HandleTouchInteractions(GestureStatus.Started, point);

					if (handled == true)
						Parent.RequestDisallowInterceptTouchEvent(true);

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

			if (TouchInsideContent(point) && _isOpen)
				ResetSwipe();

			_initialPoint = point;
			_isTouchDown = true;

			return true;
		}

		bool ProcessTouchMove(APointF point)
		{
			if (_contentView == null || !TouchInsideContent(point))
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

		bool TouchInsideContent(double x1, double y1, double x2, double y2, double x, double y)
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
			if (_contentView == null || _actionView != null)
				return;

			SwipeItems items = GetSwipeItemsByDirection();

			if (items == null || items.Count == 0)
				return;

			_actionView = new LinearLayoutCompat(_context);

			using (var layoutParams = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent))
				_actionView.LayoutParameters = layoutParams;

			_actionView.Orientation = LinearLayoutCompat.Horizontal;

			var swipeItems = new List<AView>();

			foreach (var item in items)
			{
				AView swipeItem = null;

				if (item is SwipeItem formsSwipeItem)
				{
					formsSwipeItem.PropertyChanged += OnSwipeItemPropertyChanged;

					swipeItem = CreateSwipeItem(formsSwipeItem);
					_actionView.AddView(swipeItem);
					_swipeItems.Add(formsSwipeItem, swipeItem);
				}

				if (item is SwipeItemView formsSwipeItemView)
				{
					formsSwipeItemView.PropertyChanged += OnSwipeItemPropertyChanged;

					swipeItem = CreateSwipeItemView(formsSwipeItemView);
					_actionView.AddView(swipeItem);
					UpdateSwipeItemViewLayout(formsSwipeItemView);
					_swipeItems.Add(formsSwipeItemView, swipeItem);
				}

				swipeItems.Add(swipeItem);
			}

			AddView(_actionView);
			_contentView?.BringToFront();

			_actionView.Layout(0, 0, _contentView.Width, _contentView.Height);
			LayoutSwipeItems(swipeItems);
			swipeItems.Clear();
		}

		void LayoutSwipeItems(List<AView> childs)
		{
			if (_actionView == null || childs == null)
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
					var swipeItemHeight = (int)_context.ToPixels(swipeItemSize.Height);
					var swipeItemWidth = (int)_context.ToPixels(swipeItemSize.Width);

					switch (_swipeDirection)
					{
						case SwipeDirection.Left:
							child.Layout(_contentView.Width - (swipeItemWidth + previousWidth), 0, _contentView.Width - previousWidth, swipeItemHeight);
							break;
						case SwipeDirection.Right:
						case SwipeDirection.Up:
						case SwipeDirection.Down:
							child.Layout(previousWidth, 0, (i + 1) * swipeItemWidth, swipeItemHeight);
							break;
					}

					i++;
					previousWidth += swipeItemWidth;
				}
			}
		}

		void OnSwipeItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var swipeItem = (ISwipeItem)sender;

			if (e.PropertyName == SwipeItem.IsVisibleProperty.PropertyName)
			{
				UpdateIsVisibleSwipeItem(swipeItem);
			}
		}

		void UpdateIsVisibleSwipeItem(ISwipeItem item)
		{
			if (!_isOpen)
				return;

			_swipeItems.TryGetValue(item, out object view);

			if (view != null && view is AView nativeView)
			{
				bool hidden = false;

				if (item is SwipeItem swipeItem)
					hidden = !swipeItem.IsVisible;

				if (item is SwipeItemView swipeItemView)
					hidden = !swipeItemView.IsVisible;

				_swipeThreshold = 0;
				nativeView.Visibility = hidden ? ViewStates.Gone : ViewStates.Visible;
				LayoutSwipeItems(GetNativeSwipeItems());
				SwipeToThreshold(false);
			}
		}

		List<AView> GetNativeSwipeItems()
		{
			var swipeItems = new List<AView>();

			for (int i = 0; i < _actionView.ChildCount; i++)
				swipeItems.Add(_actionView.GetChildAt(i));

			return swipeItems;
		}

		AView CreateSwipeItem(SwipeItem formsSwipeItem)
		{
			var swipeButton = new AButton(_context)
			{
				Background = new ColorDrawable(formsSwipeItem.BackgroundColor.ToAndroid()),
				Text = formsSwipeItem.Text ?? string.Empty
			};

			if (!string.IsNullOrEmpty(formsSwipeItem.AutomationId))
				swipeButton.ContentDescription = formsSwipeItem.AutomationId;

			var textColor = GetSwipeItemColor(formsSwipeItem.BackgroundColor);
			swipeButton.SetTextColor(textColor.ToAndroid());
			swipeButton.TextAlignment = ATextAlignment.Center;

			int contentHeight = _contentView.Height;
			int contentWidth = (int)_context.ToPixels(SwipeItemWidth);
			int iconSize = formsSwipeItem.IconImageSource != null ? Math.Min(contentHeight, contentWidth) / 2 : 0;

			_ = this.ApplyDrawableAsync(formsSwipeItem, MenuItem.IconImageSourceProperty, Context, drawable =>
			{
				int drawableWidth = drawable.IntrinsicWidth;
				int drawableHeight = drawable.IntrinsicHeight;

				if (drawableWidth > drawableHeight)
				{
					var iconWidth = iconSize;
					var iconHeight = drawableHeight * iconWidth / drawableWidth;
					drawable.SetBounds(0, 0, iconWidth, iconHeight);
				}
				else
				{
					var iconHeight = iconSize;
					var iconWidth = drawableWidth * iconHeight / drawableHeight;
					drawable.SetBounds(0, 0, iconWidth, iconHeight);
				}

				drawable.SetColorFilter(textColor.ToAndroid(), FilterMode.SrcAtop);
				swipeButton.SetCompoundDrawables(null, drawable, null, null);
			});

			var textSize = !string.IsNullOrEmpty(swipeButton.Text) ? (int)swipeButton.TextSize : 0;
			var buttonPadding = (contentHeight - (iconSize + textSize + 6)) / 2;
			swipeButton.SetPadding(0, buttonPadding, 0, buttonPadding);
			swipeButton.SetOnTouchListener(null);
			swipeButton.Visibility = formsSwipeItem.IsVisible ? ViewStates.Visible : ViewStates.Gone;

			if (!string.IsNullOrEmpty(formsSwipeItem.AutomationId))
				swipeButton.ContentDescription = formsSwipeItem.AutomationId;

			return swipeButton;
		}

		AView CreateSwipeItemView(SwipeItemView swipeItemView)
		{
			var renderer = Platform.CreateRenderer(swipeItemView, _context);
			Platform.SetRenderer(swipeItemView, renderer);
			var swipeItem = renderer?.View;
			swipeItem.Visibility = swipeItemView.IsVisible ? ViewStates.Visible : ViewStates.Gone;

			return swipeItem;
		}

		void UpdateSwipeItemViewLayout(SwipeItemView swipeItemView)
		{
			var swipeItemSize = GetSwipeItemSize(swipeItemView);
			var swipeItemHeight = swipeItemSize.Height;
			var swipeItemWidth = swipeItemSize.Width;

			swipeItemView.Layout(new Rectangle(0, 0, swipeItemWidth, swipeItemHeight));
			swipeItemView.Content?.Layout(new Rectangle(0, 0, swipeItemWidth, swipeItemHeight));
		}

		void UpdateIsSwipeEnabled()
		{
			_isSwipeEnabled = Element.IsEnabled;
		}

		void UpdateSwipeTransitionMode()
		{
			if (Element.IsSet(Specifics.SwipeTransitionModeProperty))
				_swipeTransitionMode = Element.OnThisPlatform().GetSwipeTransitionMode();
			else
				_swipeTransitionMode = SwipeTransitionMode.Reveal;
		}

		Color GetSwipeItemColor(Color backgroundColor)
		{
			var luminosity = 0.2126 * backgroundColor.R + 0.7152 * backgroundColor.G + 0.0722 * backgroundColor.B;

			return luminosity < 0.75 ? Color.White : Color.Black;
		}

		void UnsubscribeSwipeItemEvents()
		{
			var items = GetSwipeItemsByDirection();

			if (items == null)
				return;

			foreach (var item in items)
			{
				if (item is SwipeItem formsSwipeItem)
					formsSwipeItem.PropertyChanged -= OnSwipeItemPropertyChanged;

				if (item is SwipeItemView formsSwipeItemView)
					formsSwipeItemView.PropertyChanged -= OnSwipeItemPropertyChanged;
			}
		}

		void DisposeSwipeItems()
		{
			_isOpen = false;
			UnsubscribeSwipeItemEvents();
			_swipeItems.Clear();

			if (_actionView != null)
			{
				_actionView.RemoveFromParent();
				_actionView.Dispose();
				_actionView = null;
			}

			UpdateIsOpen(false);
		}

		void Swipe()
		{
			var offset = _context.ToPixels(ValidateSwipeOffset(_swipeOffset));
			_isOpen = offset != 0;

			if (_swipeTransitionMode == SwipeTransitionMode.Reveal)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
					case SwipeDirection.Right:
						_contentView.TranslationX = offset;
						break;
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						_contentView.TranslationY = offset;
						break;
				}
			}

			if (_swipeTransitionMode == SwipeTransitionMode.Drag)
			{
				int actionSize;
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						_contentView.TranslationX = offset;
						actionSize = (int)_context.ToPixels(Element.RightItems.Count * SwipeItemWidth);
						_actionView.TranslationX = actionSize - Math.Abs(offset);
						break;
					case SwipeDirection.Right:
						_contentView.TranslationX = offset;
						actionSize = (int)_context.ToPixels(Element.LeftItems.Count * SwipeItemWidth);
						_actionView.TranslationX = -actionSize + offset;
						break;
					case SwipeDirection.Up:
						_contentView.TranslationY = offset;
						actionSize = _contentView.Height;
						_actionView.TranslationY = actionSize - Math.Abs(offset);
						break;
					case SwipeDirection.Down:
						_contentView.TranslationY = offset;
						actionSize = _contentView.Height;
						_actionView.TranslationY = -actionSize + Math.Abs(offset);
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

		void ResetSwipe(bool animated = true)
		{
			if (_contentView == null)
				return;

			var resetAnimationDuration = animated ? SwipeAnimationDuration : 0;

			_isResettingSwipe = true;
			_isSwiping = false;
			_swipeThreshold = 0;

			switch (_swipeDirection)
			{
				case SwipeDirection.Left:
				case SwipeDirection.Right:

					_swipeDirection = null;

					_contentView.Animate().TranslationX(0).SetDuration(resetAnimationDuration).WithEndAction(new Java.Lang.Runnable(() =>
					{
						if (_swipeDirection == null)
							DisposeSwipeItems();

						_isResettingSwipe = false;
					}));
					break;
				case SwipeDirection.Up:
				case SwipeDirection.Down:

					_swipeDirection = null;

					_contentView.Animate().TranslationY(0).SetDuration(resetAnimationDuration).WithEndAction(new Java.Lang.Runnable(() =>
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

		void SwipeToThreshold(bool animated = true)
		{
			var completeAnimationDuration = animated ? SwipeAnimationDuration : 0;
			_swipeOffset = GetSwipeThreshold();
			float swipeThreshold = _context.ToPixels(_swipeOffset);

			if (_swipeTransitionMode == SwipeTransitionMode.Reveal)
			{
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						_contentView.Animate().TranslationX(-swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
					case SwipeDirection.Right:
						_contentView.Animate().TranslationX(swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
					case SwipeDirection.Up:
						_contentView.Animate().TranslationY(-swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
					case SwipeDirection.Down:
						_contentView.Animate().TranslationY(swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						break;
				}
			}

			if (_swipeTransitionMode == SwipeTransitionMode.Drag)
			{
				int actionSize;
				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						_contentView.Animate().TranslationX(-swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = (int)_context.ToPixels(Element.RightItems.Count * SwipeItemWidth);
						_actionView.Animate().TranslationX(actionSize - swipeThreshold).SetDuration(completeAnimationDuration);
						break;
					case SwipeDirection.Right:
						_contentView.Animate().TranslationX(swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = (int)_context.ToPixels(Element.LeftItems.Count * SwipeItemWidth);
						_actionView.Animate().TranslationX(-actionSize + swipeThreshold).SetDuration(completeAnimationDuration);
						break;
					case SwipeDirection.Up:
						_contentView.Animate().TranslationY(-swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = _contentView.Height;
						_actionView.Animate().TranslationY(actionSize - Math.Abs(swipeThreshold)).SetDuration(completeAnimationDuration);
						break;
					case SwipeDirection.Down:
						_contentView.Animate().TranslationY(swipeThreshold).SetDuration(completeAnimationDuration).WithEndAction(new Java.Lang.Runnable(() => { _isSwiping = false; }));
						actionSize = _contentView.Height;
						_actionView.Animate().TranslationY(-actionSize + Math.Abs(swipeThreshold)).SetDuration(completeAnimationDuration);
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
						if (swipeItem.IsVisible)
							ExecuteSwipeItem(swipeItem);
					}

					if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
						ResetSwipe();
				}
				else
					SwipeToThreshold();
			}
			else
			{
				ResetSwipe();
			}
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

		float GetSwipeThreshold(SwipeItems swipeItems)
		{
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
						if (swipeItem.IsVisible)
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
			{
				if (isHorizontal)
					swipeThreshold = CalculateSwipeThreshold();
				else
				{
					var contentHeight = (float)_context.FromPixels(_contentView.Height);
					swipeThreshold = (SwipeThreshold > contentHeight) ? contentHeight : SwipeThreshold;
				}
			}

			return ValidateSwipeThreshold(swipeThreshold);
		}

		float CalculateSwipeThreshold()
		{
			if (_contentView != null)
			{
				var contentWidth = (float)_context.FromPixels(_contentView.Width);
				var swipeThreshold = contentWidth * 0.8f;

				return swipeThreshold;
			}

			return SwipeThreshold;
		}

		float ValidateSwipeThreshold(float swipeThreshold)
		{
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
			bool isHorizontal = IsHorizontalSwipe();
			var items = GetSwipeItemsByDirection();

			double threshold = Element.Threshold;
			double contentHeight = _context.FromPixels(_contentView.Height);
			double contentWidth = _context.FromPixels(_contentView.Width);

			if (isHorizontal)
			{
				if (swipeItem is SwipeItem)
				{
					return new Size(items.Mode == SwipeMode.Execute ? (threshold > 0 ? threshold : contentWidth) / items.Count : (threshold < SwipeItemWidth ? SwipeItemWidth : threshold), contentHeight);
				}

				if (swipeItem is SwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
					return new Size(swipeItemViewSizeRequest.Request.Width > 0 ? (float)swipeItemViewSizeRequest.Request.Width : ((threshold > 0 && threshold < SwipeItemWidth) ? SwipeItemWidth : threshold), contentHeight);
				}
			}
			else
			{
				if (swipeItem is SwipeItem)
				{
					var swipeItemHeight = GetSwipeItemHeight();
					return new Size(contentWidth / items.Count, (threshold > 0 && threshold < swipeItemHeight) ? threshold : swipeItemHeight);
				}

				if (swipeItem is SwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
					return new Size(contentWidth / items.Count, swipeItemViewSizeRequest.Request.Height > 0 ? (float)swipeItemViewSizeRequest.Request.Height : contentHeight);
				}
			}

			return Size.Zero;
		}

		float GetSwipeItemHeight()
		{
			var contentHeight = (float)_context.FromPixels(_contentView.Height);
			var items = GetSwipeItemsByDirection();

			if (items.Any(s => s is SwipeItemView))
			{
				var itemsHeight = new List<float>();

				foreach (var swipeItem in items)
				{
					if (swipeItem is SwipeItemView swipeItemView)
					{
						var swipeItemViewSizeRequest = swipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
						itemsHeight.Add((float)swipeItemViewSizeRequest.Request.Height);
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

				if (swipeButton.Visibility == ViewStates.Visible)
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

		void ExecuteSwipeItem(ISwipeItem item)
		{
			if (item == null)
				return;

			bool isEnabled = true;

			if (item is SwipeItem swipeItem)
				isEnabled = swipeItem.IsEnabled;

			if (item is SwipeItemView swipeItemView)
				isEnabled = swipeItemView.IsEnabled;

			if (isEnabled)
				item.OnInvoked();
		}

		void EnableParentGesture(bool isGestureEnabled)
		{
			if (_viewPagerParent != null)
				_viewPagerParent.EnableGesture = isGestureEnabled;
		}

		void OnOpenRequested(object sender, OpenSwipeEventArgs e)
		{
			if (_contentView == null)
				return;

			var openSwipeItem = e.OpenSwipeItem;
			ProgrammaticallyOpenSwipeItem(openSwipeItem);
		}

		void ProgrammaticallyOpenSwipeItem(OpenSwipeItem openSwipeItem)
		{
			if (_isOpen)
				return;

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

			if (swipeItems.Count == 0)
				return;

			var swipeThreshold = GetSwipeThreshold();
			_swipeOffset = swipeThreshold;

			UpdateSwipeItems();
			Swipe();
		}

		void UpdateIsOpen(bool isOpen)
		{
			if (Element == null)
				return;

			((ISwipeViewController)Element).IsOpen = isOpen;
		}

		void OnCloseRequested(object sender, EventArgs e)
		{
			ResetSwipe();
		}

		void OnParentScrolled(object sender, ScrolledEventArgs e)
		{
			var horizontalDelta = e.ScrollX - _previousScrollX;
			var verticalDelta = e.ScrollY - _previousScrollY;

			if (horizontalDelta > SwipeMinimumDelta || verticalDelta > SwipeMinimumDelta)
				ResetSwipe();

			_previousScrollX = e.ScrollX;
			_previousScrollY = e.ScrollY;
		}

		void OnParentScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			if (e.HorizontalDelta > SwipeMinimumDelta || e.VerticalDelta > SwipeMinimumDelta)
				ResetSwipe();
		}

		void RaiseSwipeStarted()
		{
			if (_swipeDirection == null || !ValidateSwipeDirection())
				return;

			var swipeStartedEventArgs = new SwipeStartedEventArgs(_swipeDirection.Value);
			((ISwipeViewController)Element).SendSwipeStarted(swipeStartedEventArgs);
		}

		void RaiseSwipeChanging()
		{
			if (_swipeDirection == null)
				return;

			var swipeChangingEventArgs = new SwipeChangingEventArgs(_swipeDirection.Value, _swipeOffset);
			((ISwipeViewController)Element).SendSwipeChanging(swipeChangingEventArgs);
		}

		void RaiseSwipeEnded()
		{
			if (_swipeDirection == null || !ValidateSwipeDirection())
				return;

			bool isOpen = false;

			var swipeThresholdPercent = OpenSwipeThresholdPercentage * GetSwipeThreshold();

			if (Math.Abs(_swipeOffset) >= swipeThresholdPercent)
				isOpen = true;

			var swipeEndedEventArgs = new SwipeEndedEventArgs(_swipeDirection.Value, isOpen);
			((ISwipeViewController)Element).SendSwipeEnded(swipeEndedEventArgs);
		}
	}
}