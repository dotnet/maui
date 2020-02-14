using System;
using System.ComponentModel;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ChildViewController : UIViewController
	{
		public override void ViewDidLayoutSubviews()
		{
			foreach (var vc in ChildViewControllers)
				vc.View.Frame = View.Bounds;
		}
	}

	internal class EventedViewController : ChildViewController
	{
		MasterView _masterView;

		event EventHandler _didAppear;
		event EventHandler _willDisappear;

		public EventedViewController()
		{
			_masterView = new MasterView();
		}


		public event EventHandler DidAppear
		{
			add
			{
				_masterView.DidAppear += value;
				_didAppear += value;
			}
			remove
			{
				_masterView.DidAppear -= value;
				_didAppear -= value;
			}
		}

		public event EventHandler WillDisappear
		{
			add
			{
				_masterView.WillDisappear += value;
				_willDisappear += value;
			}
			remove
			{
				_masterView.WillDisappear -= value;
				_willDisappear -= value;
			}
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			_didAppear?.Invoke(this, EventArgs.Empty);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			_willDisappear?.Invoke(this, EventArgs.Empty);
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			_willDisappear?.Invoke(this, EventArgs.Empty);
		}

		public override void LoadView()
		{
			View = _masterView;
		}

		public class MasterView : UIView
		{
			public bool IsCollapsed => Center.X <= 0;
			bool _previousIsCollapsed = true;

			public event EventHandler DidAppear;
			public event EventHandler WillDisappear;

			// this only gets called on iOS12 everytime it's collapsed or expanded
			// I haven't found an override on iOS13 that gets called but it doesn't seem
			// to matter because the DidAppear and WillDisappear seem more consistent on iOS 13
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				UpdateCollapsedSetting();
			}

			void UpdateCollapsedSetting()
			{
				if (_previousIsCollapsed != IsCollapsed)
				{
					_previousIsCollapsed = IsCollapsed;

					if (IsCollapsed)
						WillDisappear?.Invoke(this, EventArgs.Empty);
					else
						DidAppear?.Invoke(this, EventArgs.Empty);
				}
			}
		}
	}

	public class TabletMasterDetailRenderer : UISplitViewController, IVisualElementRenderer, IEffectControlProvider
	{
		UIViewController _detailController;

		bool _disposed;
		EventTracker _events;
		InnerDelegate _innerDelegate;
		nfloat _masterWidth = 0;
		EventedViewController _masterController;
		MasterDetailPage _masterDetailPage;
		VisualElementTracker _tracker;
		CGSize _previousSize = CGSize.Empty;
		CGSize _previousViewDidLayoutSize = CGSize.Empty;
		UISplitViewControllerDisplayMode _previousDisplayMode  = UISplitViewControllerDisplayMode.Automatic;

		Page PageController => Element as Page;
		Element ElementController => Element as Element;
		bool IsMasterVisible => !(_masterController?.View as EventedViewController.MasterView).IsCollapsed;

		protected MasterDetailPage MasterDetailPage => _masterDetailPage ?? (_masterDetailPage = (MasterDetailPage)Element);

		[Internals.Preserve(Conditional = true)]
		public TabletMasterDetailRenderer()
		{

		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Element != null)
				{
					PageController.SendDisappearing();
					Element.PropertyChanged -= HandlePropertyChanged;

					if (MasterDetailPage?.Master != null)
					{
						MasterDetailPage.Master.PropertyChanged -= HandleMasterPropertyChanged;
					}

					Element = null;
				}

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}

				if (_masterController != null)
				{
					_masterController.DidAppear -= MasterControllerDidAppear;
					_masterController.WillDisappear -= MasterControllerWillDisappear;
				}

				ClearControllers();
			}

			base.Dispose(disposing);
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

			ViewControllers = new[] { _masterController = new EventedViewController(), _detailController = new ChildViewController() };
			
			if (!Forms.IsiOS9OrNewer)
				Delegate = _innerDelegate = new InnerDelegate(MasterDetailPage.MasterBehavior);

			UpdateControllers();

			_masterController.DidAppear += MasterControllerDidAppear;
			_masterController.WillDisappear += MasterControllerWillDisappear;

			PresentsWithGesture = MasterDetailPage.IsGestureEnabled;
			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (element != null)
				element.SendViewInitialized(NativeView);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Width, size.Width, size.Height));
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		public override void ViewDidAppear(bool animated)
		{
			PageController.SendAppearing();
			base.ViewDidAppear(animated);
			ToggleMaster();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			PageController?.SendDisappearing();
		}


		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			bool layoutMaster = false;
			bool layoutDetails = false;

			if (Forms.IsiOS13OrNewer)
			{
				layoutMaster = _masterController?.View?.Superview != null;
				layoutDetails = _detailController?.View?.Superview != null;
			}
			else if (View.Subviews.Length < 2)
			{
				return;
			}
			else
			{
				layoutMaster = true;
				layoutDetails = true;
			}

			if (layoutMaster)
			{
				var masterBounds = _masterController.View.Frame;

				if (Forms.IsiOS13OrNewer)
					_masterWidth = masterBounds.Width;
				else
					_masterWidth = (nfloat)Math.Max(_masterWidth, masterBounds.Width);

				if (!masterBounds.IsEmpty)
					MasterDetailPage.MasterBounds = new Rectangle(0, 0, _masterWidth, masterBounds.Height);
			}

			if (layoutDetails)
			{
				var detailsBounds = _detailController.View.Frame;
				if (!detailsBounds.IsEmpty)
					MasterDetailPage.DetailBounds = new Rectangle(0, 0, detailsBounds.Width, detailsBounds.Height);
			}

			if (_previousViewDidLayoutSize == CGSize.Empty)
				_previousViewDidLayoutSize = View.Bounds.Size;

			// Is this being called from a rotation
			if (_previousViewDidLayoutSize != View.Bounds.Size)
			{
				_previousViewDidLayoutSize = View.Bounds.Size;

				// make sure IsPresented matches state of Master View
				if (MasterDetailPage.CanChangeIsPresented && MasterDetailPage.IsPresented != IsMasterVisible)
					ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, IsMasterVisible);
			}

			if(_previousDisplayMode != PreferredDisplayMode)
			{
				_previousDisplayMode = PreferredDisplayMode;

				// make sure IsPresented matches state of Master View
				if (MasterDetailPage.CanChangeIsPresented && MasterDetailPage.IsPresented != IsMasterVisible)
					ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, IsMasterVisible);
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			UpdateBackground();
			UpdateFlowDirection();
			UpdateMasterBehavior(View.Bounds.Size);
			_tracker = new VisualElementTracker(this);
			_events = new EventTracker(this);
			_events.LoadEvents(NativeView);
		}

		void UpdateMasterBehavior(CGSize newBounds)
		{
			MasterDetailPage masterDetailPage = _masterDetailPage ?? Element as MasterDetailPage;

			if (masterDetailPage == null)
				return;

			bool isPortrait = newBounds.Height > newBounds.Width;
			var previous = PreferredDisplayMode;

			switch (masterDetailPage.MasterBehavior)
			{
				case MasterBehavior.Split:
					PreferredDisplayMode = UISplitViewControllerDisplayMode.AllVisible;
					break;
				case MasterBehavior.Popover:
					PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryHidden;
					break;
				case MasterBehavior.SplitOnPortrait:
					PreferredDisplayMode = (isPortrait) ? UISplitViewControllerDisplayMode.AllVisible : UISplitViewControllerDisplayMode.PrimaryHidden;
					break;
				case MasterBehavior.SplitOnLandscape:
					PreferredDisplayMode = (!isPortrait) ? UISplitViewControllerDisplayMode.AllVisible : UISplitViewControllerDisplayMode.PrimaryHidden;
					break;
				default:
					PreferredDisplayMode = UISplitViewControllerDisplayMode.Automatic;
					break;
			}

			if (previous == PreferredDisplayMode)
				return;

			if (!MasterDetailPage.ShouldShowSplitMode)
				MasterDetailPage.CanChangeIsPresented = true;

			MasterDetailPage.UpdateMasterBehavior();
		}

		public override void ViewWillDisappear(bool animated)
		{
			if (IsMasterVisible && !MasterDetailPage.ShouldShowSplitMode)
				PerformButtonSelector();

			base.ViewWillDisappear(animated);
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();
			_masterController.View.BackgroundColor = UIColor.White;
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			// I tested this code on iOS9+ and it's never called
			if (!Forms.IsiOS9OrNewer)
			{
				if (!MasterDetailPage.ShouldShowSplitMode && IsMasterVisible)
				{
					MasterDetailPage.CanChangeIsPresented = true;
					PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryHidden;
					PreferredDisplayMode = UISplitViewControllerDisplayMode.Automatic;
				}

				MasterDetailPage.UpdateMasterBehavior();
				MessagingCenter.Send<IVisualElementRenderer>(this, NavigationRenderer.UpdateToolbarButtons);
			}

			base.WillRotate(toInterfaceOrientation, duration);
		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			if (((MasterDetailPage)Element).Detail != null)
				return (UIViewController)Platform.GetRenderer(((MasterDetailPage)Element).Detail);
			else
				return base.ChildViewControllerForStatusBarHidden();
		}

		public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden
		{
			get
			{
				if (((MasterDetailPage)Element).Detail != null)
					return (UIViewController)Platform.GetRenderer(((MasterDetailPage)Element).Detail);
				else
					return base.ChildViewControllerForHomeIndicatorAutoHidden;
			}
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
				e.OldElement.PropertyChanged -= HandlePropertyChanged;

			if (e.NewElement != null)
				e.NewElement.PropertyChanged += HandlePropertyChanged;

			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);

			_masterWidth = 0;
		}

		void ClearControllers()
		{
			foreach (var controller in _masterController.ChildViewControllers)
			{
				controller.View.RemoveFromSuperview();
				controller.RemoveFromParentViewController();
			}

			foreach (var controller in _detailController.ChildViewControllers)
			{
				controller.View.RemoveFromSuperview();
				controller.RemoveFromParentViewController();
			}
		}

		void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.IconImageSourceProperty.PropertyName || e.PropertyName == Page.TitleProperty.PropertyName)
				MessagingCenter.Send<IVisualElementRenderer>(this, NavigationRenderer.UpdateToolbarButtons);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_tracker == null)
				return;

			if (e.PropertyName == "Master" || e.PropertyName == "Detail")
				UpdateControllers();
			else if (e.PropertyName == Xamarin.Forms.MasterDetailPage.IsPresentedProperty.PropertyName)
				ToggleMaster();
			else if (e.PropertyName == Xamarin.Forms.MasterDetailPage.IsGestureEnabledProperty.PropertyName)
				base.PresentsWithGesture = this.MasterDetailPage.IsGestureEnabled;
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (e.Is(MasterDetailPage.MasterBehaviorProperty))
				UpdateMasterBehavior(View.Bounds.Size);

			MessagingCenter.Send<IVisualElementRenderer>(this, NavigationRenderer.UpdateToolbarButtons);
		}

		public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
		{
			base.ViewWillTransitionToSize(toSize, coordinator);

			if (_previousSize != toSize)
			{
				_previousSize = toSize;
				UpdateMasterBehavior(toSize);
			}
		}

		void MasterControllerDidAppear(object sender, EventArgs e)
		{
			if (MasterDetailPage.CanChangeIsPresented && IsMasterVisible)
				ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, true);
		}

		void MasterControllerWillDisappear(object sender, EventArgs e)
		{
			if (MasterDetailPage.CanChangeIsPresented && !IsMasterVisible)
				ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, false);
		}

		void PerformButtonSelector()
		{
			DisplayModeButtonItem.Target.PerformSelector(DisplayModeButtonItem.Action, DisplayModeButtonItem, 0);
		}

		void ToggleMaster()
		{
			if (IsMasterVisible == MasterDetailPage.IsPresented || MasterDetailPage.ShouldShowSplitMode)
				return;

			PerformButtonSelector();
		}

		void UpdateBackground()
		{
			_ = this.ApplyNativeImageAsync(Page.BackgroundImageSourceProperty, bgImage =>
			{
				if (bgImage != null)
					View.BackgroundColor = UIColor.FromPatternImage(bgImage);
				else if (Element.BackgroundColor == Color.Default)
					View.BackgroundColor = UIColor.White;
				else
					View.BackgroundColor = Element.BackgroundColor.ToUIColor();
			});
		}

		void UpdateControllers()
		{
			MasterDetailPage.Master.PropertyChanged -= HandleMasterPropertyChanged;

			if (Platform.GetRenderer(MasterDetailPage.Master) == null)
				Platform.SetRenderer(MasterDetailPage.Master, Platform.CreateRenderer(MasterDetailPage.Master));
			if (Platform.GetRenderer(MasterDetailPage.Detail) == null)
				Platform.SetRenderer(MasterDetailPage.Detail, Platform.CreateRenderer(MasterDetailPage.Detail));

			ClearControllers();

			MasterDetailPage.Master.PropertyChanged += HandleMasterPropertyChanged;

			var master = Platform.GetRenderer(MasterDetailPage.Master).ViewController;
			var detail = Platform.GetRenderer(MasterDetailPage.Detail).ViewController;

			_masterController.View.AddSubview(master.View);
			_masterController.AddChildViewController(master);

			_detailController.View.AddSubview(detail.View);
			_detailController.AddChildViewController(detail);
		}

		void UpdateFlowDirection()
		{
			if(NativeView.UpdateFlowDirection(Element) && Forms.IsiOS13OrNewer && NativeView.Superview != null)
			{
				var view = NativeView.Superview;
				NativeView.RemoveFromSuperview();
				view.AddSubview(NativeView);
			}
		}

		class InnerDelegate : UISplitViewControllerDelegate
		{
			readonly MasterBehavior _masterPresentedDefaultState;

			public InnerDelegate(MasterBehavior masterPresentedDefaultState)
			{
				_masterPresentedDefaultState = masterPresentedDefaultState;
			}

			public override bool ShouldHideViewController(UISplitViewController svc, UIViewController viewController, UIInterfaceOrientation inOrientation)
			{		
				bool willHideViewController;
				switch (_masterPresentedDefaultState)
				{
					case MasterBehavior.Split:
						willHideViewController = false;
						break;
					case MasterBehavior.Popover:
						willHideViewController = true;
						break;
					case MasterBehavior.SplitOnPortrait:
						willHideViewController = !(inOrientation == UIInterfaceOrientation.Portrait || inOrientation == UIInterfaceOrientation.PortraitUpsideDown);
						break;
					default:
						willHideViewController = inOrientation == UIInterfaceOrientation.Portrait || inOrientation == UIInterfaceOrientation.PortraitUpsideDown;
						break;
				}
				return willHideViewController;
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, View);
		}
	}
}