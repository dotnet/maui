using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer instead")]
	public class PhoneFlyoutPageRenderer : UIViewController, IVisualElementRenderer, IEffectControlProvider
	{
		UIView _clickOffView;
		UIViewController _detailController;

		bool _disposed;
		EventTracker _events;

		UIViewController _flyoutController;

		UIPanGestureRecognizer _panGesture;

		bool _presented;
		UIGestureRecognizer _tapGesture;

		VisualElementTracker _tracker;
		bool _applyShadow;

		Page Page => Element as Page;
		IFlyoutPageController FlyoutPageController => FlyoutPage;


		[Preserve(Conditional = true)]
		public PhoneFlyoutPageRenderer()
		{
		}

		FlyoutPage FlyoutPage => Element as FlyoutPage;

		bool Presented
		{
			get { return _presented; }
			set
			{
				if (_presented == value)
					return;
				_presented = value;
				LayoutChildren(true);

				if (value)
					AddClickOffView();
				else
					RemoveClickOffView();

				ToggleAccessibilityElementsHidden();

				((IElementController)Element).SetValueFromRenderer(Microsoft.Maui.Controls.FlyoutPage.IsPresentedProperty, value);
			}
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public UIView NativeView
		{
			get { return View; }
		}

		public void SetElement(VisualElement element)
		{
			var oldElement = Element;
			Element = element;
			Element.SizeChanged += PageOnSizeChanged;

			_flyoutController = new ChildViewController();
			_detailController = new ChildViewController();

			_clickOffView = new UIView();
			_clickOffView.BackgroundColor = new Color(0, 0, 0, 0).ToPlatform();

			Presented = ((FlyoutPage)Element).IsPresented;

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			element?.SendViewInitialized(NativeView);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rect(Element.X, Element.Y, size.Width, size.Height));
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			Page.SendAppearing();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			Page?.SendDisappearing();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();


			// TODO MAUI: Is this correct?
			if (Element.Width == -1 && Element.Height == -1)
				Element.Layout(new Rect(Element.X, Element.Y, View.Bounds.Width, View.Bounds.Height));

			LayoutChildren(false);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_tracker = new VisualElementTracker(this);

			((FlyoutPage)Element).PropertyChanged += HandlePropertyChanged;

			_tapGesture = new UITapGestureRecognizer(() =>
			{
				if (Presented)
					Presented = false;
			});
			_clickOffView.AddGestureRecognizer(_tapGesture);

			PackContainers();
			UpdateFlyoutPageContainers();

			UpdateBackground();

			UpdatePanGesture();
			UpdateApplyShadow(((FlyoutPage)Element).OnThisPlatform().GetApplyShadow());

		}

		[System.Runtime.Versioning.UnsupportedOSPlatform("ios8.0")]
		[System.Runtime.Versioning.UnsupportedOSPlatform("tvos")]
		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			if (!FlyoutPageController.ShouldShowSplitMode && _presented)
				Presented = false;

			base.WillRotate(toInterfaceOrientation, duration);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				Element.SizeChanged -= PageOnSizeChanged;
				Element.PropertyChanged -= HandlePropertyChanged;

				_tracker?.Dispose();
				_tracker = null;

				_events?.Dispose();
				_events = null;

				if (_tapGesture != null)
				{
					if (_clickOffView != null && _clickOffView.GestureRecognizers.Contains(_tapGesture))
					{
						_clickOffView.GestureRecognizers.Remove(_tapGesture);
						_clickOffView.Dispose();
					}
					_tapGesture.Dispose();
				}
				if (_panGesture != null)
				{
					if (View != null && View.GestureRecognizers.Contains(_panGesture))
						View.GestureRecognizers.Remove(_panGesture);
					_panGesture.Dispose();
				}

				EmptyContainers();

				Page.SendDisappearing();

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		void AddClickOffView()
		{
			View.Add(_clickOffView);
			_clickOffView.Frame = _detailController.View.Frame;
		}

		void EmptyContainers()
		{
			foreach (var child in _detailController.View.Subviews.Concat(_flyoutController.View.Subviews))
				child.RemoveFromSuperview();

			foreach (var vc in _detailController.ChildViewControllers.Concat(_flyoutController.ChildViewControllers))
				vc.RemoveFromParentViewController();
		}

		void HandleFlyoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.IconImageSourceProperty.PropertyName || e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateLeftBarButton();
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Flyout" || e.PropertyName == "Detail")
				UpdateFlyoutPageContainers();
			else if (e.PropertyName == Microsoft.Maui.Controls.FlyoutPage.IsPresentedProperty.PropertyName)
				Presented = ((FlyoutPage)Element).IsPresented;
			else if (e.PropertyName == Microsoft.Maui.Controls.FlyoutPage.IsGestureEnabledProperty.PropertyName)
				UpdatePanGesture();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.FlyoutPage.ApplyShadowProperty.PropertyName)
				UpdateApplyShadow(((FlyoutPage)Element).OnThisPlatform().GetApplyShadow());
		}

		void LayoutChildren(bool animated)
		{
			var frame = Element.Bounds.ToRectangleF();
			var flyoutFrame = frame;
			nfloat opacity = 1;
			flyoutFrame.Width = (int)(Math.Min(flyoutFrame.Width, flyoutFrame.Height) * 0.8);
			var detailRenderer = Platform.GetRenderer(FlyoutPage.Detail);
			if (detailRenderer == null)
				return;
			var detailView = detailRenderer.ViewController.View;

			var isRTL = (Element as IVisualElementController)?.EffectiveFlowDirection.IsRightToLeft() == true;
			if (isRTL)
			{
				flyoutFrame.X = (int)(flyoutFrame.Width * .25);
			}

			_flyoutController.View.Frame = flyoutFrame;

			var target = frame;
			if (Presented)
			{
				target.X += flyoutFrame.Width;
				if (_applyShadow)
					opacity = 0.5f;
			}

			if (isRTL)
			{
				target.X = target.X * -1;
			}

			if (animated)
			{
				UIView.BeginAnimations("Flyout");
				var view = _detailController.View;
				view.Frame = target;
				detailView.Layer.Opacity = (float)opacity;
#pragma warning disable CA1416, CA1422// TODO: SetAnimationCurve(...), SetAnimationDuration(250), CommitAnimations() is unsupported on: 'ios' 13.0 and later
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseOut);
				UIView.SetAnimationDuration(250);
				UIView.CommitAnimations();
#pragma warning restore CA1416, CA1422
			}
			else
			{
				_detailController.View.Frame = target;
				detailView.Layer.Opacity = (float)opacity;
			}

			FlyoutPageController.FlyoutBounds = new Rect(flyoutFrame.X, 0, flyoutFrame.Width, flyoutFrame.Height);
			FlyoutPageController.DetailBounds = new Rect(0, 0, frame.Width, frame.Height);

			if (Presented)
				_clickOffView.Frame = _detailController.View.Frame;
		}

		void PackContainers()
		{
			_detailController.View.BackgroundColor = new UIColor(1, 1, 1, 1);
			View.AddSubview(_flyoutController.View);
			View.AddSubview(_detailController.View);

			AddChildViewController(_flyoutController);
			AddChildViewController(_detailController);
		}

		void PageOnSizeChanged(object sender, EventArgs eventArgs)
		{
			LayoutChildren(false);
		}

		void RemoveClickOffView()
		{
			_clickOffView.RemoveFromSuperview();
		}

		void UpdateBackground()
		{
			_ = this.ApplyNativeImageAsync(Page.BackgroundImageSourceProperty, bgImage =>
			{
				if (bgImage != null)
					View.BackgroundColor = UIColor.FromPatternImage(bgImage);
				else
				{
					Brush background = Element.Background;

					if (!Brush.IsNullOrEmpty(background))
						View.UpdateBackground(Element.Background);
					else
					{
						if (Element.BackgroundColor == null)
							View.BackgroundColor = UIColor.White;
						else
							View.BackgroundColor = Element.BackgroundColor.ToPlatform();
					}
				}
			});
		}

		void UpdateFlyoutPageContainers()
		{
			((FlyoutPage)Element).Flyout.PropertyChanged -= HandleFlyoutPropertyChanged;

			EmptyContainers();

			if (Platform.GetRenderer(((FlyoutPage)Element).Flyout) == null)
				Platform.SetRenderer(((FlyoutPage)Element).Flyout, Platform.CreateRenderer(((FlyoutPage)Element).Flyout));
			if (Platform.GetRenderer(((FlyoutPage)Element).Detail) == null)
				Platform.SetRenderer(((FlyoutPage)Element).Detail, Platform.CreateRenderer(((FlyoutPage)Element).Detail));

			var flyoutRenderer = Platform.GetRenderer(((FlyoutPage)Element).Flyout);
			var detailRenderer = Platform.GetRenderer(((FlyoutPage)Element).Detail);

			((FlyoutPage)Element).Flyout.PropertyChanged += HandleFlyoutPropertyChanged;

			UIView flyoutView = flyoutRenderer.NativeView;

			_flyoutController.View.AddSubview(flyoutView);
			_flyoutController.AddChildViewController(flyoutRenderer.ViewController);

			UIView detailView = detailRenderer.NativeView;

			_detailController.View.AddSubview(detailView);
			_detailController.AddChildViewController(detailRenderer.ViewController);

			SetNeedsStatusBarAppearanceUpdate();
			if (OperatingSystem.IsIOSVersionAtLeast(11))
				SetNeedsUpdateOfHomeIndicatorAutoHidden();

			detailRenderer.ViewController.View.Superview?.BackgroundColor = Microsoft.Maui.Graphics.Colors.Black.ToPlatform();

			ToggleAccessibilityElementsHidden();
		}

		void UpdateLeftBarButton()
		{
			var FlyoutPage = Element as FlyoutPage;
			if (!(FlyoutPage?.Detail is NavigationPage))
				return;

			var detailRenderer = Platform.GetRenderer(FlyoutPage.Detail) as UINavigationController;

			UIViewController firstPage = detailRenderer?.ViewControllers.FirstOrDefault();
			if (firstPage != null)
#pragma warning disable CS0618 // Type or member is obsolete
				NavigationRenderer.SetFlyoutLeftBarButton(firstPage, FlyoutPage);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateApplyShadow(bool value)
		{
			_applyShadow = value;
		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			if (((FlyoutPage)Element).Detail != null)
				return Platform.GetRenderer(((FlyoutPage)Element).Detail).ViewController;
			else
				return base.ChildViewControllerForStatusBarHidden();
		}

		public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden
		{
			get
			{
				if (((FlyoutPage)Element).Detail != null)
					return Platform.GetRenderer(((FlyoutPage)Element).Detail).ViewController;
				else
					return base.ChildViewControllerForStatusBarHidden();
			}
		}

		void ToggleAccessibilityElementsHidden()
		{
			var flyoutView = _flyoutController?.View;
			flyoutView?.AccessibilityElementsHidden = !Presented;

			var detailView = _detailController?.View;
			detailView?.AccessibilityElementsHidden = Presented;
		}

		void UpdatePanGesture()
		{
			var model = (FlyoutPage)Element;
			if (!model.IsGestureEnabled)
			{
				if (_panGesture != null)
					View.RemoveGestureRecognizer(_panGesture);
				return;
			}

			if (_panGesture != null)
			{
				View.AddGestureRecognizer(_panGesture);
				return;
			}

			bool shouldReceive(UIGestureRecognizer g, UITouch t)
			{
				return !(t.View is UISlider) && !IsSwipeView(t.View);
			}

			var center = new PointF();
			_panGesture = new UIPanGestureRecognizer(g =>
			{
				var isRTL = (Element as IVisualElementController)?.EffectiveFlowDirection.IsRightToLeft() == true;

				int directionModifier = isRTL ? -1 : 1;

				switch (g.State)
				{
					case UIGestureRecognizerState.Began:
						center = g.LocationInView(g.View);
						break;
					case UIGestureRecognizerState.Changed:
						var currentPosition = g.LocationInView(g.View);
						var motion = currentPosition.X - center.X;

						motion = motion * directionModifier;

						var detailView = _detailController.View;
						var targetFrame = detailView.Frame;
						if (Presented)
							targetFrame.X = (nfloat)Math.Max(0, _flyoutController.View.Frame.Width + Math.Min(0, motion));
						else
							targetFrame.X = (nfloat)Math.Min(_flyoutController.View.Frame.Width, Math.Max(0, motion));

						targetFrame.X = targetFrame.X * directionModifier;
						if (_applyShadow)
						{
							var openProgress = targetFrame.X / _flyoutController.View.Frame.Width;
							ApplyDetailShadow((nfloat)openProgress);
						}

						detailView.Frame = targetFrame;
						break;
					case UIGestureRecognizerState.Ended:
						var detailFrame = _detailController.View.Frame;
						var flyoutFrame = _flyoutController.View.Frame;
						if (Presented)
						{
							if (detailFrame.X * directionModifier < flyoutFrame.Width * .75)
								Presented = false;
							else
								LayoutChildren(true);
						}
						else
						{
							if (detailFrame.X * directionModifier > flyoutFrame.Width * .25)
								Presented = true;
							else
								LayoutChildren(true);
						}
						break;
				}
			});
			_panGesture.CancelsTouchesInView = false;
			_panGesture.ShouldReceiveTouch = shouldReceive;
			_panGesture.MaximumNumberOfTouches = 2;
			View.AddGestureRecognizer(_panGesture);
		}

		bool IsSwipeView(UIView view)
		{
			if (view == null)
				return false;

			if (view.Superview is SwipeViewRenderer)
				return true;

			return IsSwipeView(view.Superview);
		}

		class ChildViewController : UIViewController
		{
			public override void ViewDidLayoutSubviews()
			{
				foreach (var vc in ChildViewControllers)
					vc.View.Frame = View.Bounds;
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, View);
		}

		void ApplyDetailShadow(nfloat percent)
		{
			var detailView = Platform.GetRenderer(FlyoutPage.Detail).ViewController.View;
			var opacity = (nfloat)(0.5 + (0.5 * (1 - percent)));
			detailView.Layer.Opacity = (float)opacity;
		}
	}
}