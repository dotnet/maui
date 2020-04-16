using CoreGraphics;
using MediaPlayer;
using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellFlyoutRenderer : UIViewController, IShellFlyoutRenderer, IFlyoutBehaviorObserver
	{
		#region IShellFlyoutRenderer

		UIView IShellFlyoutRenderer.View => View;

		UIViewController IShellFlyoutRenderer.ViewController => this;

		void IShellFlyoutRenderer.AttachFlyout(IShellContext context, UIViewController content)
		{
			Context = context;
			Shell = Context.Shell;
			Detail = content;

			Shell.PropertyChanged += OnShellPropertyChanged;

			PanGestureRecognizer = new UIPanGestureRecognizer(HandlePanGesture);
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
		}

		bool IsSwipeView(UIView view)
		{
			if (view == null)
				return false;

			if (view is SwipeViewRenderer)
				return true;

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
		}

		#endregion IFlyoutBehaviorObserver

		const string FlyoutAnimationName = "Flyout";
		bool _disposed;
		FlyoutBehavior _flyoutBehavior;
		bool _gestureActive;
		bool _isOpen;
		public UIViewAnimationCurve AnimationCurve { get; set; } = UIViewAnimationCurve.EaseOut;

		public int AnimationDuration { get; set; } = 250;

		public IShellFlyoutTransition FlyoutTransition { get; set; }

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
			}
		}

		UIPanGestureRecognizer PanGestureRecognizer { get; set; }

		Shell Shell { get; set; }

		UIView TapoffView { get; set; }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

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
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (!_disposed)
				{
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
					LayoutSidebar(true);
				}
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection(true);
			}
		}

		void UpdateFlowDirection(bool readdViews = false)
		{
			bool update = View.UpdateFlowDirection(Shell);
			update = Flyout?.ViewController?.View.UpdateFlowDirection(Shell) == true || update;
			update = Detail?.View?.UpdateFlowDirection(Shell) == true || update;

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

		void AddTapoffView()
		{
			if (TapoffView != null)
				return;

			TapoffView = new UIView(View.Bounds);
			View.InsertSubviewBelow(TapoffView, Flyout.ViewController.View);
			TapoffView.AddGestureRecognizer(new UITapGestureRecognizer(t =>
			{
				IsOpen = false;
				LayoutSidebar(true);
			}));
		}

		public void FocusSearch(bool forwardDirection)
		{
			var element = Shell.CurrentItem as ITabStopElement;
			var tabIndexes = element?.GetTabIndexesOnParentPage(out _);
			if (tabIndexes == null)
				return;

			int tabIndex = element.TabIndex;
			element = element.FindNextElement(forwardDirection, tabIndexes, ref tabIndex);
			if (element is ShellItem item)
				Shell.CurrentItem = item;
			else if (element is VisualElement ve)
				ve.Focus();
		}

		UIKeyCommand[] tabCommands = {
			UIKeyCommand.Create ((Foundation.NSString)"\t", 0, new ObjCRuntime.Selector ("tabForward:")),
			UIKeyCommand.Create ((Foundation.NSString)"\t", UIKeyModifierFlags.Shift, new ObjCRuntime.Selector ("tabBackward:"))
		};

		public override UIKeyCommand[] KeyCommands => tabCommands;

		public UIView NativeView => throw new NotImplementedException();

		public UIViewController ViewController => throw new NotImplementedException();

		[Foundation.Export("tabForward:")]
		void TabForward(UIKeyCommand cmd) => FocusSearch(forwardDirection: true);

		[Foundation.Export("tabBackward:")]
		void TabBackward(UIKeyCommand cmd) => FocusSearch(forwardDirection: false);

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

		void LayoutSidebar(bool animate)
		{
			if (_gestureActive)
				return;

			if (animate)
				UIView.BeginAnimations(FlyoutAnimationName);

			FlyoutTransition.LayoutViews(View.Bounds, IsOpen ? 1 : 0, Flyout.ViewController.View, Detail.View, _flyoutBehavior);

			if (animate)
			{
				UIView.SetAnimationCurve(AnimationCurve);
				UIView.SetAnimationDuration(AnimationDuration);
				UIView.CommitAnimations();
				View.LayoutIfNeeded();
			}

			if (IsOpen && _flyoutBehavior == FlyoutBehavior.Flyout)
				AddTapoffView();
			else
				RemoveTapoffView();
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