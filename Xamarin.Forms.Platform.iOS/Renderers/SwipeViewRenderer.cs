using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using UIKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.iOSSpecific.SwipeView;

namespace Xamarin.Forms.Platform.iOS
{
	public class SwipeViewRenderer : ViewRenderer<SwipeView, UIView>
	{
		const double SwipeThreshold = 250;
		const double SwipeItemWidth = 100;
		const double SwipeAnimationDuration = 0.2;
		const double SwipeMinimumDelta = 10;

		UITapGestureRecognizer _tapGestureRecognizer;
		UIPanGestureRecognizer _panGestureRecognizer;
		View _scrollParent;
		UIView _contentView;
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
		double _previousScrollX;
		double _previousScrollY;
		int _previousFirstVisibleIndex;
		bool _isSwipeEnabled;
		bool _isResettingSwipe;
		bool _isOpen;
		bool _isDisposed;

		[Preserve(Conditional = true)]
		public SwipeViewRenderer()
		{
			SwipeView.VerifySwipeViewFlagEnabled(nameof(SwipeViewRenderer));

			_tapGestureRecognizer = new UITapGestureRecognizer(HandleTap)
			{
				CancelsTouchesInView = false,
				DelaysTouchesBegan = false,
				DelaysTouchesEnded = false
			};

			_panGestureRecognizer = new UIPanGestureRecognizer(HandlePan)
			{
				CancelsTouchesInView = false,
				DelaysTouchesBegan = false,
				DelaysTouchesEnded = false
			};

			_panGestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true;

			AddGestureRecognizer(_tapGestureRecognizer);
			AddGestureRecognizer(_panGestureRecognizer);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<SwipeView> e)
		{
			if (e.NewElement != null)
			{
				e.NewElement.OpenRequested += OnOpenRequested;
				e.NewElement.CloseRequested += OnCloseRequested;

				if (Control == null)
				{
					SetNativeControl(CreateNativeControl());
				}

				UpdateContent();
				UpdateIsSwipeEnabled();
				UpdateSwipeTransitionMode();
				SetBackgroundColor(Element.BackgroundColor);
			}

			if (e.OldElement != null)
			{
				e.OldElement.OpenRequested -= OnOpenRequested;
				e.OldElement.CloseRequested -= OnCloseRequested;
			}

			base.OnElementChanged(e);
		}

		protected override UIView CreateNativeControl()
		{
			return new UIView();
		}

		public override void WillMoveToWindow(UIWindow window)
		{
			base.WillMoveToWindow(window);

			if (window != null && _scrollParent == null)
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

				_scrollParent = Element.FindParentOfType<CollectionView>();

				if (_scrollParent is CollectionView collectionView)
				{
					collectionView.Scrolled += OnParentScrolled;
				}
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Element.Content != null)
				Element.Content.Layout(Bounds.ToRectangle());
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == ContentView.ContentProperty.PropertyName)
				UpdateContent();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				SetBackgroundColor(Element.BackgroundColor);
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsSwipeEnabled();
			else if (e.PropertyName == Specifics.SwipeTransitionModeProperty.PropertyName)
				UpdateSwipeTransitionMode();
		}

		protected override void SetBackgroundColor(Color color)
		{
			UIColor backgroundColor = ColorExtensions.BackgroundColor;

			if (Element.BackgroundColor != Color.Default)
			{
				BackgroundColor = Element.BackgroundColor.ToUIColor();

				if (_contentView != null && (Element.Content == null || (Element.Content != null && Element.Content.BackgroundColor == Color.Default)))
					_contentView.BackgroundColor = Element.BackgroundColor.ToUIColor();
			}
			else
				BackgroundColor = backgroundColor;

			if (_contentView != null && _contentView.BackgroundColor == UIColor.Clear)
				_contentView.BackgroundColor = backgroundColor;
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
					if(_scrollParent is ScrollView scrollView)
						scrollView.Scrolled -= OnParentScrolled;

					if (_scrollParent is ListView listView)
						listView.Scrolled -= OnParentScrolled;

					if (_scrollParent is CollectionView collectionView)
						collectionView.Scrolled -= OnParentScrolled;
				}

				if (_tapGestureRecognizer != null)
				{
					Control.RemoveGestureRecognizer(_tapGestureRecognizer);
					_tapGestureRecognizer.Dispose();
					_tapGestureRecognizer = null;
				}

				if (_panGestureRecognizer != null)
				{
					Control.RemoveGestureRecognizer(_panGestureRecognizer);
					_panGestureRecognizer.Dispose();
					_panGestureRecognizer = null;
				}

				if (_contentView != null)
				{
					_contentView.Dispose();
					_contentView = null;
				}

				if (_actionView != null)
				{
					_actionView.Dispose();
					_actionView = null;
				}

				if (_swipeItemsRect != null)
				{
					_swipeItemsRect.Clear();
					_swipeItemsRect = null;
				}
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		public override UIView HitTest(CGPoint point, UIEvent uievent)
		{
			if (!UserInteractionEnabled || Hidden)
				return null;

			foreach (var subview in Subviews)
			{
				var view = HitTest(subview, point, uievent);

				if (view != null)
					return view;
			}

			return base.HitTest(point, uievent);
		}

		UIView HitTest(UIView view, CGPoint point, UIEvent uievent)
		{
			if (view.Subviews == null)
				return null;

			foreach (var subview in view.Subviews)
			{
				CGPoint subPoint = subview.ConvertPointFromView(point, this);
				UIView result = subview.HitTest(subPoint, uievent);

				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		void UpdateContent()
		{
			ClipsToBounds = true;

			if (Element.Content == null)
			{
				_contentView = CreateEmptyContent();
				AddSubview(_contentView);
			}
			else
			{
				if (Subviews.Length > 0)
					_contentView = Subviews[0];
			}
		}

		void HandleTap()
		{
			if (_tapGestureRecognizer == null)
			{
				return;
			}

			var state = _tapGestureRecognizer.State;

			if (state != UIGestureRecognizerState.Cancelled)
			{
				if (_contentView == null)
					return;

				var point = _tapGestureRecognizer.LocationInView(this);

				if (_isOpen)
				{
					if (!TouchInsideContent(point))
						ProcessTouchSwipeItems(point);
					else
						ResetSwipe();
				}
			}
		}

		void HandlePan(UIPanGestureRecognizer panGestureRecognizer)
		{
			if (panGestureRecognizer != null)
			{
				var point = panGestureRecognizer.LocationInView(Control);
				var navigationController = GetUINavigationController(GetViewController());

				switch (panGestureRecognizer.State)
				{
					case UIGestureRecognizerState.Began:
						if (navigationController != null)
							navigationController.InteractivePopGestureRecognizer.Enabled = false;

						HandleTouchInteractions(GestureStatus.Started, point);
						break;
					case UIGestureRecognizerState.Changed:
						HandleTouchInteractions(GestureStatus.Running, point);
						break;
					case UIGestureRecognizerState.Ended:
						if (navigationController != null)
							navigationController.InteractivePopGestureRecognizer.Enabled = true;

						HandleTouchInteractions(GestureStatus.Completed, point);
						break;
					case UIGestureRecognizerState.Cancelled:
						if (navigationController != null)
							navigationController.InteractivePopGestureRecognizer.Enabled = true;

						HandleTouchInteractions(GestureStatus.Canceled, point);
						break;
				}
			}
		}

		UIView CreateEmptyContent()
		{
			var emptyContentView = new UIView
			{
				BackgroundColor = Color.Default.ToUIColor()
			};

			return emptyContentView;
		}

		void UpdateIsSwipeEnabled()
		{
			_isSwipeEnabled = Element.IsEnabled;
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
			return swipeItems != null && swipeItems.Count > 0;
		}

		void UpdateSwipeItems()
		{
			if (_contentView == null || _actionView != null)
				return;

			_swipeItemsRect = new List<CGRect>();

			SwipeItems items = GetSwipeItemsByDirection();
			double swipeItemsWidth;

			if (_swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right)
				swipeItemsWidth = (items != null ? items.Count : 0) * SwipeItemWidth;
			else
				swipeItemsWidth = _contentView.Frame.Width;

			if (items == null || items.Count == 0)
				return;

			_actionView = new UIStackView
			{
				Axis = UILayoutConstraintAxis.Horizontal,
				Frame = new CGRect(0, 0, swipeItemsWidth, _contentView.Frame.Height)
			};

			int i = 0;
			float previousWidth = 0;

			foreach (var item in items)
			{
				var swipeItem = (item is SwipeItem) ? CreateSwipeItem((SwipeItem)item) : CreateSwipeItemView((SwipeItemView)item);

				var swipeItemSize = GetSwipeItemSize(item);
				float swipeItemHeight = (float)swipeItemSize.Height;
				float swipeItemWidth = (float)swipeItemSize.Width;

				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						swipeItem.Frame = new CGRect(_contentView.Frame.Width - (swipeItemWidth + previousWidth), 0, i + 1 * swipeItemWidth, swipeItemHeight);
						break;
					case SwipeDirection.Right:
					case SwipeDirection.Up:
					case SwipeDirection.Down:
						swipeItem.Frame = new CGRect(previousWidth, 0, i + 1 * swipeItemWidth, swipeItemHeight);
						break;
				}

				_actionView.AddSubview(swipeItem);
				_swipeItemsRect.Add(swipeItem.Frame);
				previousWidth += swipeItemWidth;

				i++;
			}

			AddSubview(_actionView);
			BringSubviewToFront(_contentView);
		}

		UIButton CreateSwipeItem(SwipeItem formsSwipeItem)
		{
			var swipeItem = new UIButton(UIButtonType.Custom)
			{
				BackgroundColor = formsSwipeItem.BackgroundColor.ToUIColor()
			};

			swipeItem.SetTitle(formsSwipeItem.Text, UIControlState.Normal);

			UpdateSwipeItemIconImage(swipeItem, formsSwipeItem);

			var textColor = GetSwipeItemColor(formsSwipeItem.BackgroundColor);
			swipeItem.SetTitleColor(textColor.ToUIColor(), UIControlState.Normal);
			swipeItem.UserInteractionEnabled = false;
			UpdateSwipeItemInsets(swipeItem);

			return swipeItem;
		}

		UIView CreateSwipeItemView(SwipeItemView formsSwipeItemView)
		{
			var renderer = Platform.CreateRenderer(formsSwipeItemView);
			Platform.SetRenderer(formsSwipeItemView, renderer);
			UpdateSwipeItemViewLayout(formsSwipeItemView);
			return renderer?.NativeView;
		}

		void UpdateSwipeItemViewLayout(SwipeItemView swipeItemView)
		{
			var swipeItemSize = GetSwipeItemSize(swipeItemView);

			swipeItemView.Layout(new Rectangle(0, 0, swipeItemSize.Width, swipeItemSize.Height));
		}

		void UpdateSwipeTransitionMode()
		{
			if (Element.IsSet(Specifics.SwipeTransitionModeProperty))
				_swipeTransitionMode = Element.OnThisPlatform().GetSwipeTransitionMode();
			else
				_swipeTransitionMode = SwipeTransitionMode.Reveal;
		}

		void UpdateSwipeItemInsets(UIButton button, float spacing = 0.0f)
		{
			if (button.ImageView?.Image == null)
				return;

			var imageSize = button.ImageView.Image.Size;

			var titleEdgeInsets = new UIEdgeInsets(spacing, -imageSize.Width, -imageSize.Height, 0.0f);
			button.TitleEdgeInsets = titleEdgeInsets;

			var labelString = button.TitleLabel.Text ?? string.Empty;
			var titleSize = !string.IsNullOrEmpty(labelString) ? labelString.StringSize(button.TitleLabel.Font) : CGSize.Empty;
			var imageEdgeInsets = new UIEdgeInsets(-(titleSize.Height + spacing), 0.0f, 0.0f, -titleSize.Width);
			button.ImageEdgeInsets = imageEdgeInsets;
		}

		Color GetSwipeItemColor(Color backgroundColor)
		{
			var luminosity = 0.2126 * backgroundColor.R + 0.7152 * backgroundColor.G + 0.0722 * backgroundColor.B;

			return luminosity < 0.75 ? Color.White : Color.Black;
		}

		async void UpdateSwipeItemIconImage(UIButton swipeButton, SwipeItem swipeItem)
		{
			if (swipeButton == null)
				return;

			if (swipeItem.IconImageSource == null || swipeItem.IconImageSource.IsEmpty)
			{
				swipeButton.SetImage(null, UIControlState.Normal);
			}
			else
			{
				var image = await swipeItem.IconImageSource.GetNativeImageAsync();

				try
				{
					swipeButton.SetImage(image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
					var tintColor = GetSwipeItemColor(swipeItem.BackgroundColor);
					swipeButton.TintColor = tintColor.ToUIColor();
				}
				catch (Exception)
				{
					// UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
					Log.Warning("SwipeView", "Can not load SwipeItem Icon.");
				}
			}
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
					ProcessTouchUp(point);
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

			var swipeDirection = _swipeDirection;

			if (!_isOpen && !_isResettingSwipe)
				swipeDirection = SwipeDirectionHelper.GetSwipeDirection(new Point(_initialPoint.X, _initialPoint.Y), new Point(point.X, point.Y));

			if (_swipeDirection != swipeDirection)
			{
				_swipeDirection = swipeDirection;

				if (!_isOpen)
				{
					ResetSwipe(false);
					UpdateSwipeItems();
				}
			}

			if (!_isSwiping)
			{
				RaiseSwipeStarted();
				_isSwiping = true;
			}

			if (!ValidateSwipeDirection() || _isResettingSwipe)
				return;

			_swipeOffset = GetSwipeOffset(_initialPoint, point);

			if (Math.Abs(_swipeOffset) > double.Epsilon)
				Swipe();

			RaiseSwipeChanging();
		}

		void ProcessTouchUp(CGPoint point)
		{
			_isTouchDown = false;

			if (CanProcessTouchSwipeItems(point))
				ProcessTouchSwipeItems(point);

			if (!_isSwiping)
				return;

			_isSwiping = false;

			RaiseSwipeEnded();

			if (_isResettingSwipe || !ValidateSwipeDirection())
				return;

			ValidateSwipeThreshold();
		}

		bool CanProcessTouchSwipeItems(CGPoint point)
		{
			// We only invoke the SwipeItem command if we tap on the SwipeItems area
			// and the SwipeView is fully open.
			if (TouchInsideContent(point))
				return false;

			if (_swipeOffset == _swipeThreshold)
				return true;

			return false;
		}

		bool TouchInsideContent(CGPoint point)
		{
			if (_contentView == null)
				return false;

			bool touchContent = TouchInsideContent(_contentView.Frame.Left, _contentView.Frame.Top, _contentView.Frame.Width, _contentView.Frame.Height, point.X, point.Y);

			return touchContent;
		}

		bool TouchInsideContent(double x1, double y1, double x2, double y2, double x, double y)
		{
			if (x > x1 && x < (x1 + x2) && y > y1 && y < (y1 + y2))
				return true;

			return false;
		}

		SwipeItems GetSwipeItemsByDirection()
		{
			SwipeItems swipeItems = null;

			switch (_swipeDirection)
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

		void Swipe()
		{
			if (_contentView == null)
				return;

			_originalBounds = _contentView.Bounds;
			var offset = ValidateSwipeOffset(_swipeOffset);
			_isOpen = offset != 0;

			if (_swipeTransitionMode == SwipeTransitionMode.Reveal)
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
			}

			if (_swipeTransitionMode == SwipeTransitionMode.Drag)
			{
				var actionBounds = _actionView.Bounds;
				double actionSize;

				switch (_swipeDirection)
				{
					case SwipeDirection.Left:
						_contentView.Frame = new CGRect(_originalBounds.X + offset, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
						actionSize = Element.RightItems.Count * SwipeItemWidth;
						_actionView.Frame = new CGRect(actionSize + offset, actionBounds.Y, actionBounds.Width, actionBounds.Height);
						break;
					case SwipeDirection.Right:
						_contentView.Frame = new CGRect(_originalBounds.X + offset, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
						actionSize = Element.LeftItems.Count * SwipeItemWidth;
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
			_swipeThreshold = 0;

			if (_actionView != null)
			{
				_actionView.RemoveFromSuperview();
				_actionView.Dispose();
				_actionView = null;
			}

			if (_swipeItemsRect != null)
			{
				_swipeItemsRect.Clear();
				_swipeItemsRect = null;
			}
		}

		void ResetSwipe(bool animated = true)
		{
			if (_swipeItemsRect == null || _contentView == null)
				return;

			_isResettingSwipe = true;
			_isSwiping = false;
			_swipeThreshold = 0;
			_swipeDirection = null;

			var resetAnimationDuration = animated ? SwipeAnimationDuration : 0;

			Animate(resetAnimationDuration, 0.0, UIViewAnimationOptions.CurveEaseOut, () =>
			{
				_contentView.Frame = new CGRect(_originalBounds.X, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
			},
			() =>
			{
				DisposeSwipeItems();
				_isResettingSwipe = false;
			});
		}

		void ValidateSwipeThreshold()
		{
			if (_swipeDirection == null)
				return;

			var swipeThresholdPercent = 0.6 * GetSwipeThreshold();

			if (Math.Abs(_swipeOffset) >= swipeThresholdPercent)
			{
				var swipeItems = GetSwipeItemsByDirection();

				if (swipeItems == null)
					return;

				if (swipeItems.Mode == SwipeMode.Execute)
				{
					foreach (var swipeItem in swipeItems)
					{
						ExecuteSwipeItem(swipeItem);
					}

					if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
						ResetSwipe();
				}
				else
					CompleteSwipe();
			}
			else
				ResetSwipe();
		}

		void CompleteSwipe(bool animated = true)
		{
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
								actionSize = Element.RightItems.Count * SwipeItemWidth;
								_actionView.Frame = new CGRect(actionSize - swipeThreshold, actionBounds.Y, actionBounds.Width, actionBounds.Height);
								break;
							case SwipeDirection.Right:
								_contentView.Frame = new CGRect(_originalBounds.X + swipeThreshold, _originalBounds.Y, _originalBounds.Width, _originalBounds.Height);
								actionSize = Element.LeftItems.Count * SwipeItemWidth;
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

		double GetSwipeThreshold(SwipeItems swipeItems)
		{
			double swipeThreshold = 0;

			bool isHorizontal = IsHorizontalSwipe();

			if (swipeItems.Mode == SwipeMode.Reveal)
			{
				if (isHorizontal)
				{
					foreach (var swipeItem in swipeItems)
					{
						var swipeItemSize = GetSwipeItemSize(swipeItem);
						swipeThreshold += swipeItemSize.Width;
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
					var contentHeight = _contentView.Frame.Height;
					swipeThreshold = (SwipeThreshold > contentHeight) ? contentHeight : SwipeThreshold;
				}
			}

			return ValidateSwipeThreshold(swipeThreshold);
		}

		double CalculateSwipeThreshold()
		{
			if (_contentView != null)
			{
				var swipeThreshold = _contentView.Frame.Width * 0.8;

				return swipeThreshold;
			}

			return SwipeThreshold;
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

		Size GetSwipeItemSize(ISwipeItem swipeItem)
		{
			var items = GetSwipeItemsByDirection();
			var swipeFrame = _contentView != null ? _contentView.Frame : Frame;

			if (IsHorizontalSwipe())
			{
				if (swipeItem is SwipeItem)
					return new Size(items.Mode == SwipeMode.Execute ? swipeFrame.Width / items.Count : SwipeItemWidth, swipeFrame.Height);

				if (swipeItem is SwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
					return new Size(swipeItemViewSizeRequest.Request.Width > 0 ? (float)swipeItemViewSizeRequest.Request.Width : SwipeItemWidth, swipeFrame.Height);
				}
			}
			else
			{
				if (swipeItem is SwipeItem)
					return new Size(swipeFrame.Width / items.Count, GetSwipeItemHeight()); 

				if (swipeItem is SwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
					return new Size(swipeFrame.Width / items.Count, swipeItemViewSizeRequest.Request.Height > 0 ? (float)swipeItemViewSizeRequest.Request.Height : swipeFrame.Height);
				}
			}

			return Size.Zero;
		}

		double GetSwipeItemHeight()
		{
			var items = GetSwipeItemsByDirection();

			if (items.Any(s => s is SwipeItemView))
			{
				var itemsHeight = new List<double>();

				foreach (var swipeItem in items)
				{
					if (swipeItem is SwipeItemView swipeItemView)
					{
						var swipeItemViewSizeRequest = swipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
						itemsHeight.Add(swipeItemViewSizeRequest.Request.Height);
					}
				}

				return itemsHeight.Max();
			}

			return _contentView.Frame.Height;
		}

		bool ValidateSwipeDirection()
		{
			var swipeItems = GetSwipeItemsByDirection();
			return IsValidSwipeItems(swipeItems);
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

				var swipeItemX = swipeItemRect.Left;
				var swipeItemY = swipeItemRect.Top;

				if (TouchInsideContent(swipeItemX, swipeItemY, swipeItemRect.Width, swipeItemRect.Height, point.X, point.Y))
				{
					ExecuteSwipeItem(swipeItem);

					if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
						ResetSwipe();

					break;
				}

				i++;
			}
		}

		UIViewController GetViewController()
		{
			var window = UIApplication.SharedApplication.GetKeyWindow();
			var viewController = window.RootViewController;

			while (viewController.PresentedViewController != null)
				viewController = viewController.PresentedViewController;

			return viewController;
		}

		UINavigationController GetUINavigationController(UIViewController controller)
		{
			if (controller != null)
			{
				if (controller is UINavigationController)
				{
					return (controller as UINavigationController);
				}

				if (controller.ChildViewControllers.Any())
				{
					var childs = controller.ChildViewControllers.Count();

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

		void ExecuteSwipeItem(ISwipeItem swipeItem)
		{
			if (swipeItem == null)
				return;

			swipeItem.OnInvoked();
		}

		void OnParentScrolled(object sender, ScrolledEventArgs e)
		{
			var horizontalDelta = e.ScrollX - _previousScrollX;
			var verticalDelta = e.ScrollY - _previousScrollY;

			if (horizontalDelta >= SwipeMinimumDelta || verticalDelta >= SwipeMinimumDelta)
				ResetSwipe();

			_previousScrollX = e.ScrollX;
			_previousScrollY = e.ScrollY;
		}

		void OnParentScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			var firstVisibleIndexDelta = e.FirstVisibleItemIndex - _previousFirstVisibleIndex;

			if (firstVisibleIndexDelta != 0)
				ResetSwipe();

			_previousFirstVisibleIndex = e.FirstVisibleItemIndex;
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

		void OnCloseRequested(object sender, EventArgs e)
		{
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

			var swipeEndedEventArgs = new SwipeEndedEventArgs(_swipeDirection.Value);
			((ISwipeViewController)Element).SendSwipeEnded(swipeEndedEventArgs);
		}
	}
}