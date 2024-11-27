#nullable disable
using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using MediaPlayer;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellFlyoutRenderer : UIViewController, IShellFlyoutRenderer, IFlyoutBehaviorObserver, IAppearanceObserver
	{
		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
			{
				_backdropBrush = Brush.Default;
			}
			else
			{
				_backdropBrush = appearance.FlyoutBackdrop;

				if (SlideFlyoutTransition?.UpdateFlyoutSize(appearance.FlyoutHeight, appearance.FlyoutWidth) ==
					true)
				{
					if (_layoutOccured)
						LayoutSidebar(false, true);
				}
			}

			UpdateTapoffViewBackgroundColor();
		}

		#endregion IAppearanceObserver

		#region IShellFlyoutRenderer

		UIView IShellFlyoutRenderer.View => View;

		UIViewController IShellFlyoutRenderer.ViewController => this;

		public override bool PrefersHomeIndicatorAutoHidden => Detail.PrefersHomeIndicatorAutoHidden;

		public override bool PrefersStatusBarHidden() => Detail.PrefersStatusBarHidden();

		public override UIStatusBarAnimation PreferredStatusBarUpdateAnimation => Detail.PreferredStatusBarUpdateAnimation;

		void IShellFlyoutRenderer.AttachFlyout(IShellContext context, UIViewController content)
		{
			Context = context;
			Shell = Context.Shell;
			Detail = content;

			Shell.PropertyChanged += OnShellPropertyChanged;

			PanGestureRecognizer = new UIPanGestureRecognizer(HandlePanGesture);
			PanGestureRecognizer.ShouldRecognizeSimultaneously += (a, b) =>
			{
				// This handles tapping outside the open flyout
				if (a is UIPanGestureRecognizer pr && pr.State == UIGestureRecognizerState.Failed &&
					b is UITapGestureRecognizer && b.State == UIGestureRecognizerState.Ended && IsOpen)
				{
					IsOpen = false;
					LayoutSidebar(true);
				}

				return false;
			};

			PanGestureRecognizer.ShouldReceiveTouch += (sender, touch) =>
			{
				if (!context.AllowFlyoutGesture || _flyoutBehavior != FlyoutBehavior.Flyout)
					return false;
				var view = View;
				CGPoint loc = touch.LocationInView(View);
				if (touch.View is UISlider ||
					touch.View is MPVolumeView ||
					IsSwipeView(touch.View) ||
					(loc.X > view.Frame.Width * 0.1 && !IsOpen))
					return false;

				return true;
			};

			ShellController.AddAppearanceObserver(this, Shell);
			IsOpen = Shell.FlyoutIsPresented;
		}

		bool IsSwipeView(UIView view)
		{
			if (view == null)
				return false;

			// TODO MAUI
			//if (view is SwipeView)
			//return true;

			return IsSwipeView(view.Superview);
		}

		#endregion IShellFlyoutRenderer

		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			if (behavior == FlyoutBehavior.Locked)
				IsOpen = true;
			else if (behavior == FlyoutBehavior.Disabled)
				IsOpen = false;
			LayoutSidebar(false);
			UpdateFlyoutAccessibility();
		}

		#endregion IFlyoutBehaviorObserver

		const string FlyoutAnimationName = "Flyout";
		bool _disposed;
		FlyoutBehavior _flyoutBehavior;
		bool _gestureActive;
		bool _isOpen;
		UIViewPropertyAnimator _flyoutAnimation;
		Brush _backdropBrush;
		bool _layoutOccured;

		public UIViewAnimationCurve AnimationCurve { get; set; } = UIViewAnimationCurve.EaseOut;

		public int AnimationDuration { get; set; } = 250;

		double AnimationDurationInSeconds => ((double)AnimationDuration) / 1000.0;

		public IShellFlyoutTransition FlyoutTransition
		{
			get => _flyoutTransition;
			set
			{
				_flyoutTransition = value;
				SlideFlyoutTransition = value as SlideFlyoutTransition;
			}
		}

		SlideFlyoutTransition SlideFlyoutTransition { get; set; }

		IShellContext Context { get; set; }

		UIViewController Detail { get; set; }

		IShellFlyoutContentRenderer Flyout { get; set; }

		bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				if (_isOpen == value)
					return;

				_isOpen = value;
				Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, value);
				UpdateFlyoutAccessibility();
			}
		}

		void UpdateFlyoutAccessibility()
		{
			bool flyoutElementsHidden = false;
			bool detailsElementsHidden = false;

			switch (_flyoutBehavior)
			{
				case FlyoutBehavior.Flyout:
					flyoutElementsHidden = !IsOpen;
					detailsElementsHidden = IsOpen;

					break;

				case FlyoutBehavior.Locked:
					flyoutElementsHidden = false;
					detailsElementsHidden = false;

					break;

				case FlyoutBehavior.Disabled:
					flyoutElementsHidden = true;
					detailsElementsHidden = false;

					break;
			}

			if (Flyout?.ViewController?.View != null)
				Flyout.ViewController.View.AccessibilityElementsHidden = flyoutElementsHidden;

			if (Detail?.View != null)
				Detail.View.AccessibilityElementsHidden = detailsElementsHidden;
		}

		UIPanGestureRecognizer PanGestureRecognizer { get; set; }

		Shell Shell { get; set; }

		IShellController ShellController => Shell;

		UIView TapoffView { get; set; }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (_flyoutAnimation == null)
				LayoutSidebar(false);
		}

		public override void ViewWillAppear(bool animated)
		{
			UpdateFlowDirection();
			base.ViewWillAppear(animated);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			AddChildViewController(Detail);
			View.AddSubview(Detail.View);

			Flyout = Context.CreateShellFlyoutContentRenderer();
			AddChildViewController(Flyout.ViewController);
			View.AddSubview(Flyout.ViewController.View);
			View.AddGestureRecognizer(PanGestureRecognizer);

			((IShellController)Shell).AddFlyoutBehaviorObserver(this);
			UpdateFlowDirection();
			UpdateFlyoutAccessibility();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (!_disposed)
				{
					ShellController.RemoveAppearanceObserver(this);

					_disposed = true;

					Shell.PropertyChanged -= OnShellPropertyChanged;
					((IShellController)Shell).RemoveFlyoutBehaviorObserver(this);

					Context = null;
					Shell = null;
					Detail = null;
				}
			}
		}

		protected virtual void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				var isPresented = Shell.FlyoutIsPresented;
				if (IsOpen != isPresented)
				{
					IsOpen = isPresented;
					LayoutSidebar(true, true);
				}
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection(true);
			}
		}

		void UpdateFlowDirection(bool readdViews = false)
		{
			var originalValue = View.SemanticContentAttribute;
			var originalFlyoutValue = Flyout?.ViewController?.View?.SemanticContentAttribute;
			var originalDetailValue = Detail?.View?.SemanticContentAttribute;

			View.UpdateFlowDirection(Shell);
			Flyout?.ViewController?.View.UpdateFlowDirection(Shell);
			Detail?.View?.UpdateFlowDirection(Shell);

			bool update = originalValue == View.SemanticContentAttribute;
			update = Flyout?.ViewController?.View?.SemanticContentAttribute == originalFlyoutValue || update;
			update = Detail?.View?.SemanticContentAttribute == originalDetailValue || update;

			if (update && readdViews)
			{
				if (Detail?.View != null)
					Detail.View.RemoveFromSuperview();

				if (Flyout?.ViewController?.View != null)
					Flyout.ViewController.View.RemoveFromSuperview();

				if (Detail?.View != null)
					View.AddSubview(Detail.View);

				if (Flyout?.ViewController?.View != null)
					View.AddSubview(Flyout.ViewController.View);
			}
		}

		void UpdateTapoffViewBackgroundColor()
		{
			if (TapoffView == null)
				return;

			TapoffView.UpdateBackground(_backdropBrush);

			if (Brush.IsNullOrEmpty(_backdropBrush))
			{
				if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				{
					TapoffView.BackgroundColor = UIColor.Clear;
				}
				else
				{
					TapoffView.BackgroundColor = Microsoft.Maui.Platform.ColorExtensions.BackgroundColor.ColorWithAlpha(0.5f);
				}
			}
		}

		void AddTapoffView()
		{
			if (TapoffView != null)
				return;

			TapoffView = new UIView(View.Bounds);
			TapoffView.Layer.Opacity = 0;
			View.InsertSubviewBelow(TapoffView, Flyout.ViewController.View);
			UpdateTapoffViewBackgroundColor();
			var recognizer = new UITapGestureRecognizer(t =>
			{
				IsOpen = false;
				LayoutSidebar(true);
			});

			TapoffView.AddGestureRecognizer(recognizer);
		}

		private IShellFlyoutTransition _flyoutTransition;

		public UIView NativeView => throw new NotImplementedException();

		public UIViewController ViewController => throw new NotImplementedException();

		void HandlePanGesture(UIPanGestureRecognizer pan)
		{
			var translation = pan.TranslationInView(View).X;
			double openProgress = 0;
			double openLimit = Flyout.ViewController.View.Frame.Width;

			if (IsOpen)
			{
				openProgress = 1 - (-translation / openLimit);
			}
			else
			{
				openProgress = translation / openLimit;
			}

			openProgress = Math.Min(Math.Max(openProgress, 0.0), 1.0);
			var openPixels = openLimit * openProgress;

			switch (pan.State)
			{
				case UIGestureRecognizerState.Changed:
					_gestureActive = true;

					if (TapoffView == null)
						AddTapoffView();

					if (_flyoutAnimation != null)
					{
						TapoffView.Layer.RemoveAllAnimations();
						_flyoutAnimation?.StopAnimation(true);
						_flyoutAnimation = null;
					}

					TapoffView.Layer.Opacity = (float)openProgress;

					FlyoutTransition.LayoutViews(View.Bounds, (nfloat)openProgress, Flyout.ViewController.View, Detail.View, _flyoutBehavior);
					break;

				case UIGestureRecognizerState.Ended:
					_gestureActive = false;
					if (IsOpen)
					{
						if (openProgress < .8)
							IsOpen = false;
					}
					else
					{
						if (openProgress > 0.2)
						{
							IsOpen = true;
						}
					}
					LayoutSidebar(true);
					break;
			}
		}

		void LayoutSidebar(bool animate, bool cancelExisting = false)
		{
			_layoutOccured = true;
			if (_gestureActive)
				return;

			if (cancelExisting && _flyoutAnimation != null)
			{
				_flyoutAnimation.StopAnimation(true);
				_flyoutAnimation = null;
			}

			if (animate && _flyoutAnimation != null)
				return;

			if (!animate && _flyoutAnimation != null)
			{
				_flyoutAnimation.StopAnimation(true);
				_flyoutAnimation = null;
			}

			if (IsOpen)
				UpdateTapoffView();

			if (animate && TapoffView != null)
			{
				var tapOffViewAnimation = CABasicAnimation.FromKeyPath(@"opacity");
				tapOffViewAnimation.BeginTime = 0;
				tapOffViewAnimation.Duration = AnimationDurationInSeconds;
				tapOffViewAnimation.SetFrom(NSNumber.FromFloat(TapoffView.Layer.Opacity));
				tapOffViewAnimation.SetTo(NSNumber.FromFloat(IsOpen ? 1 : 0));
				tapOffViewAnimation.FillMode = CAFillMode.Forwards;
				tapOffViewAnimation.RemovedOnCompletion = false;

				_flyoutAnimation = new UIViewPropertyAnimator(AnimationDurationInSeconds, UIViewAnimationCurve.EaseOut, () =>
				{
					FlyoutTransition.LayoutViews(View.Bounds, IsOpen ? 1 : 0, Flyout.ViewController.View, Detail.View, _flyoutBehavior);

					if (TapoffView != null)
					{
						TapoffView.Layer.AddAnimation(tapOffViewAnimation, "opacity");
					}
				});

				_flyoutAnimation.AddCompletion((p) =>
				{
					if (p == UIViewAnimatingPosition.End)
					{
						if (TapoffView != null)
						{
							TapoffView.Layer.Opacity = IsOpen ? 1 : 0;
							TapoffView.Layer.RemoveAllAnimations();
						}

						UpdateTapoffView();
						_flyoutAnimation = null;

						UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, null);
					}
				});

				_flyoutAnimation.StartAnimation();
				View.LayoutIfNeeded();
			}
			else if (_flyoutAnimation == null)
			{
				FlyoutTransition.LayoutViews(View.Bounds, IsOpen ? 1 : 0, Flyout.ViewController.View, Detail.View, _flyoutBehavior);
				UpdateTapoffView();

				if (TapoffView != null)
				{
					TapoffView.Layer.Opacity = IsOpen ? 1 : 0;
				}

				UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, null);
			}

			void UpdateTapoffView()
			{
				if (IsOpen && _flyoutBehavior == FlyoutBehavior.Flyout)
					AddTapoffView();
				else
					RemoveTapoffView();
			}
		}

		void RemoveTapoffView()
		{
			if (TapoffView == null)
				return;

			TapoffView.RemoveFromSuperview();
			TapoffView = null;
		}
	}
}
