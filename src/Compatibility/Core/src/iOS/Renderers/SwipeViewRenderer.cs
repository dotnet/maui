using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.SwipeView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class SwipeViewRenderer : ViewRenderer<SwipeView, UIView>
	{
		const float MinimumOpenSwipeThresholdPercentage = 0.15f; // 15%
		const float OpenSwipeThresholdPercentage = 0.6f; // 60%
		const double SwipeThreshold = 250;
		const double SwipeItemWidth = 100;
		const double SwipeAnimationDuration = 0.2;
		const double SwipeMinimumDelta = 10;

		readonly Dictionary<ISwipeItem, object> _swipeItems;
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
		bool _isScrollEnabled;
		bool _isResettingSwipe;
		bool _isOpen;
		OpenSwipeItem _previousOpenSwipeItem;
		bool _isDisposed;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public SwipeViewRenderer()
		{
			_swipeItems = new Dictionary<ISwipeItem, object>();
			_isScrollEnabled = true;

			_tapGestureRecognizer = new UITapGestureRecognizer(HandleTap)
			{
				CancelsTouchesInView = false,
				DelaysTouchesBegan = false,
				DelaysTouchesEnded = false
			};

			_tapGestureRecognizer.ShouldReceiveTouch = OnShouldReceiveTouch;

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

			if (Bounds.X < 0 || Bounds.Y < 0)
				Bounds = new CGRect(0, 0, Bounds.Width, Bounds.Height);

			if (_contentView != null && _contentView.Frame.IsEmpty)
				_contentView.Frame = Bounds;
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

		protected override void SetBackgroundColor(Color color)
		{
			if (Element.BackgroundColor != null)
				BackgroundColor = Element.BackgroundColor.ToPlatform();
			else
				BackgroundColor = ColorExtensions.BackgroundColor;
		}

		protected override void SetBackground(Brush brush)
		{
			Brush background = Element.Background;

			if (Brush.IsNullOrEmpty(background))
				return;

			Control?.UpdateBackground(background);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			if (_swipeOffset != 0)
			{
				TouchesCancelled(touches, evt);
				return;
			}

			base.TouchesEnded(touches, evt);
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
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

					if (Element.Content != null)
						Element.Content.PropertyChanged -= OnContentPropertyChanged;
				}

				if (_scrollParent != null)
				{
					if (_scrollParent is ScrollView scrollView)
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
				if (subview.UserInteractionEnabled)
				{
					var view = HitTest(subview, point, uievent);

					if (view != null)
						return view;
				}
			}

			return base.HitTest(point, uievent);
		}

		UIView HitTest(UIView view, CGPoint point, UIEvent uievent)
		{
			if (view.Subviews == null)
				return null;

			foreach (var subview in view.Subviews)
			{
				if (subview.UserInteractionEnabled)
				{
					CGPoint subPoint = subview.ConvertPointFromView(point, this);
					UIView result = subview.HitTest(subPoint, uievent);

					if (result != null)
					{
						return result;
					}
				}
			}

			return null;
		}

		bool OnShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			return _swipeOffset != 0;
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
				Element.Content.PropertyChanged += OnContentPropertyChanged;

				if (Subviews.Length > 0)
					_contentView = Subviews[0];
			}

			if (_contentView != null)
				BringSubviewToFront(_contentView);
		}

		void OnContentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsSwipeEnabled();
		}

		void HandleTap()
		{
			if (_tapGestureRecognizer == null)
				return;

			if (_isSwiping)
				return;

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
			if (_isSwipeEnabled && panGestureRecognizer != null)
			{
				CGPoint point = panGestureRecognizer.LocationInView(this);
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
				BackgroundColor = Colors.Transparent.ToPlatform()
			};

			return emptyContentView;
		}

		void UpdateIsSwipeEnabled()
		{
			UserInteractionEnabled = true;
			_isSwipeEnabled = Element.IsEnabled;

			var isContentEnabled = Element.Content.IsEnabled;
			_contentView.UserInteractionEnabled = isContentEnabled;
		}

		bool IsHorizontalSwipe()
		{
			return _swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right;
		}

		bool IsValidSwipeItems(SwipeItems swipeItems)
		{
			return swipeItems != null && swipeItems.Where(s => s.IsVisible).Count() > 0;
		}

		void UpdateSwipeItems()
		{
			if (_contentView == null || _actionView != null)
				return;

			SwipeItems items = GetSwipeItemsByDirection();

			if (items == null || items.Count == 0)
				return;

			_swipeItemsRect = new List<CGRect>();

			double swipeItemsWidth;

			if (_swipeDirection == SwipeDirection.Left || _swipeDirection == SwipeDirection.Right)
				swipeItemsWidth = (items != null ? items.Count : 0) * SwipeItemWidth;
			else
				swipeItemsWidth = _contentView.Frame.Width;

			_actionView = new UIStackView
			{
				Axis = UILayoutConstraintAxis.Horizontal,
				Frame = new CGRect(0, 0, swipeItemsWidth, _contentView.Frame.Height)
			};

			foreach (var item in items)
			{
				UIView swipeItem = null;

				if (item is SwipeItem formsSwipeItem)
				{
					formsSwipeItem.PropertyChanged += OnSwipeItemPropertyChanged;
					swipeItem = CreateSwipeItem(formsSwipeItem);
					_actionView.AddSubview(swipeItem);
					_swipeItems.Add(formsSwipeItem, swipeItem);
				}

				if (item is SwipeItemView formsSwipeItemView)
				{
					formsSwipeItemView.PropertyChanged += OnSwipeItemPropertyChanged;
					swipeItem = CreateSwipeItemView(formsSwipeItemView);
					_actionView.AddSubview(swipeItem);
					_swipeItems.Add(formsSwipeItemView, swipeItem);
				}
			}

			AddSubview(_actionView);
			BringSubviewToFront(_contentView);

			LayoutSwipeItems(GetNativeSwipeItems());
		}

		void LayoutSwipeItems(List<UIView> childs)
		{
			if (_actionView == null || childs == null)
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
					var swipeItemSize = GetSwipeItemSize(item);

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
						UpdateSwipeItemIconImage(button, (SwipeItem)item);
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

			if (view != null && view is UIView nativeView)
			{
				bool hidden = false;

				if (item is SwipeItem swipeItem)
					hidden = !swipeItem.IsVisible;

				if (item is SwipeItemView swipeItemView)
					hidden = !swipeItemView.IsVisible;

				_swipeThreshold = 0;
				nativeView.Hidden = hidden;
				LayoutSwipeItems(GetNativeSwipeItems());
				SwipeToThreshold(false);
			}
		}

		UIButton CreateSwipeItem(SwipeItem formsSwipeItem)
		{
			var swipeItem = new UIButton(UIButtonType.Custom)
			{
				RestorationIdentifier = formsSwipeItem.Text,
				BackgroundColor = formsSwipeItem.BackgroundColor.ToPlatform()
			};

			if (!string.IsNullOrEmpty(formsSwipeItem.Text))
				swipeItem.RestorationIdentifier = formsSwipeItem.Text;

			if (!string.IsNullOrEmpty(formsSwipeItem.AutomationId))
				swipeItem.AccessibilityIdentifier = formsSwipeItem.AutomationId;

			swipeItem.SetTitle(formsSwipeItem.Text, UIControlState.Normal);

			var textColor = GetSwipeItemColor(formsSwipeItem.BackgroundColor);
			swipeItem.SetTitleColor(textColor.ToPlatform(), UIControlState.Normal);
			swipeItem.UserInteractionEnabled = false;
			swipeItem.Hidden = !formsSwipeItem.IsVisible;

			if (!string.IsNullOrEmpty(formsSwipeItem.AutomationId))
				swipeItem.AccessibilityIdentifier = formsSwipeItem.AutomationId;

			return swipeItem;
		}

		UIView CreateSwipeItemView(SwipeItemView formsSwipeItemView)
		{
			var renderer = Platform.CreateRenderer(formsSwipeItemView);
			Platform.SetRenderer(formsSwipeItemView, renderer);
			UpdateSwipeItemViewLayout(formsSwipeItemView);

			var swipeItemView = renderer?.NativeView;

			if (swipeItemView != null)
				swipeItemView.Hidden = !formsSwipeItemView.IsVisible;

			return swipeItemView;
		}

		void UpdateSwipeItemViewLayout(SwipeItemView swipeItemView)
		{
			var swipeItemSize = GetSwipeItemSize(swipeItemView);

			swipeItemView.Layout(new Rect(0, 0, swipeItemSize.Width, swipeItemSize.Height));
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

			button.ContentMode = UIViewContentMode.Center;
			button.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

			var imageSize = button.ImageView.Image.Size;

			var titleEdgeInsets = new UIEdgeInsets(spacing, -imageSize.Width, -imageSize.Height, 0.0f);
#pragma warning disable CA1416, CA1422 // TODO: TitleEdgeInsets, StringSize(...), ImageEdgeInsets unsupported on: 'ios' 15.0 and later
			button.TitleEdgeInsets = titleEdgeInsets;

			var labelString = button.TitleLabel.Text ?? string.Empty;

#pragma warning disable BI1234 // Type or member is obsolete
			var titleSize = !string.IsNullOrEmpty(labelString) ? labelString.StringSize(button.TitleLabel.Font) : CGSize.Empty;
#pragma warning restore BI1234 // Type or member is obsolete
			var imageEdgeInsets = new UIEdgeInsets(-(titleSize.Height + spacing), 0.0f, 0.0f, -titleSize.Width);
			button.ImageEdgeInsets = imageEdgeInsets;
#pragma warning restore CA1416, CA1422
		}

		Color GetSwipeItemColor(Color backgroundColor)
		{
			var luminosity = 0.2126 * backgroundColor.Red + 0.7152 * backgroundColor.Green + 0.0722 * backgroundColor.Blue;

			return luminosity < 0.75 ? Colors.White : Colors.Black;
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

				var maxWidth = swipeButton.Frame.Width * 0.5f;
				var maxHeight = swipeButton.Frame.Height * 0.5f;

				var resizedImage = MaxResizeSwipeItemIconImage(image, maxWidth, maxHeight);

				try
				{
					swipeButton.SetImage(resizedImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
					var tintColor = GetSwipeItemColor(swipeItem.BackgroundColor);
					swipeButton.TintColor = tintColor.ToPlatform();
				}
				catch (Exception)
				{
					// UIImage ctor throws on file not found if MonoTouch.ObjCRuntime.Class.ThrowOnInitFailure is true;
					Forms.MauiContext?.CreateLogger<SwipeViewRenderer>()?.LogWarning("Cannot load SwipeItem Icon");
				}
			}
		}

		UIImage MaxResizeSwipeItemIconImage(UIImage sourceImage, nfloat maxWidth, nfloat maxHeight)
		{
			if (sourceImage == null)
				return null;

			var sourceSize = sourceImage.Size;
			var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);

			if (maxResizeFactor > 1)
				return sourceImage;

			var width = maxResizeFactor * sourceSize.Width;
			var height = maxResizeFactor * sourceSize.Height;
			var renderer = new UIGraphicsImageRenderer(new CGSize((nfloat)width, (nfloat)height), new UIGraphicsImageRendererFormat()
			{
				Opaque = false,
				Scale = 0,
			});

			return renderer.CreateImage((context) =>
			{
				sourceImage.Draw(new CGRect(0, 0, (nfloat)width, (nfloat)height));
			});
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

		void Swipe(bool animated = false)
		{
			if (_contentView == null)
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
			_swipeThreshold = 0;
			_swipeOffset = 0;
			_originalBounds = CGRect.Empty;

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

		void ResetSwipe(bool animated = true)
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
				ResetSwipe();
		}

		void SwipeToThreshold(bool animated = true)
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
			var threshold = Element?.Threshold;

			if (threshold.HasValue && threshold.Value > 0)
				return threshold.Value;

			double swipeThreshold = 0;
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
							swipeThreshold += swipeItemSize.Width;
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

			float swipeItemsHeight = 0;
			float swipeItemsWidth = 0;
			bool useSwipeItemsSize = false;

			foreach (var swipeItem in swipeItems)
			{
				if (swipeItem is SwipeItemView)
					useSwipeItemsSize = true;

				if (swipeItem.IsVisible)
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
					var contentWidth = _contentView.Frame.Width;
					var contentWidthSwipeThreshold = contentWidth * 0.8f;

					return contentWidthSwipeThreshold;
				}
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

			double threshold = Element.Threshold;
			var contentHeight = _contentView.Frame.Height;
			var contentWidth = _contentView.Frame.Width;

			if (IsHorizontalSwipe())
			{
				if (swipeItem is SwipeItem)
				{
					return new Size(items.Mode == SwipeMode.Execute ? (threshold > 0 ? threshold : contentWidth) / items.Count : (threshold < SwipeItemWidth ? SwipeItemWidth : threshold), contentHeight);
				}

				if (swipeItem is SwipeItemView horizontalSwipeItemView)
				{
					var swipeItemViewSizeRequest = horizontalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);

					double swipeItemWidth;

					if (swipeItemViewSizeRequest.Request.Width > 0)
						swipeItemWidth = threshold > swipeItemViewSizeRequest.Request.Width ? threshold : swipeItemViewSizeRequest.Request.Width;
					else
						swipeItemWidth = threshold > SwipeItemWidth ? threshold : SwipeItemWidth;

					return new Size(swipeItemWidth, contentHeight);
				}
			}
			else
			{
				if (swipeItem is SwipeItem)
				{
					var swipeItemHeight = GetSwipeItemHeight();
					return new Size(contentWidth / items.Count, (threshold > 0 && threshold < swipeItemHeight) ? threshold : swipeItemHeight);
				}

				if (swipeItem is SwipeItemView verticalSwipeItemView)
				{
					var swipeItemViewSizeRequest = verticalSwipeItemView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);

					double swipeItemHeight;

					if (swipeItemViewSizeRequest.Request.Width > 0)
						swipeItemHeight = threshold > swipeItemViewSizeRequest.Request.Height ? threshold : (float)swipeItemViewSizeRequest.Request.Height;
					else
						swipeItemHeight = threshold > contentHeight ? threshold : contentHeight;

					return new Size(contentWidth / items.Count, swipeItemHeight);
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
			if (_swipeDirection == null)
				return false;

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

				if (swipeItem.IsVisible)
				{
					var swipeItemX = swipeItemRect.Left;
					var swipeItemY = swipeItemRect.Top;

					if (TouchInsideContent(swipeItemX, swipeItemY, swipeItemRect.Width, swipeItemRect.Height, point.X, point.Y))
					{
						ExecuteSwipeItem(swipeItem);

						if (swipeItems.SwipeBehaviorOnInvoked != SwipeBehaviorOnInvoked.RemainOpen)
							ResetSwipe();

						break;
					}
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

		void UpdateIsOpen(bool isOpen)
		{
			if (Element == null)
				return;

			((ISwipeViewController)Element).IsOpen = isOpen;
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

		void OnOpenRequested(object sender, OpenRequestedEventArgs e)
		{
			if (_contentView == null)
				return;

			var openSwipeItem = e.OpenSwipeItem;
			var animated = e.Animated;

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

			if (swipeItems.Where(s => s.IsVisible).Count() == 0)
				return;

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

		void OnCloseRequested(object sender, CloseRequestedEventArgs e)
		{
			var animated = e.Animated;

			ResetSwipe(animated);
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