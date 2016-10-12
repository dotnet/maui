using System;
using System.ComponentModel;
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
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			var eh = WillAppear;
			if (eh != null)
				eh(this, EventArgs.Empty);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			var eh = WillDisappear;
			if (eh != null)
				eh(this, EventArgs.Empty);
		}

		public event EventHandler WillAppear;

		public event EventHandler WillDisappear;
	}

	public class TabletMasterDetailRenderer : UISplitViewController, IVisualElementRenderer, IEffectControlProvider
	{
		UIViewController _detailController;

		bool _disposed;
		EventTracker _events;
		InnerDelegate _innerDelegate;

		EventedViewController _masterController;

		MasterDetailPage _masterDetailPage;

		bool _masterVisible;

		VisualElementTracker _tracker;

		IPageController PageController => Element as IPageController;
		IElementController ElementController => Element as IElementController;

		protected MasterDetailPage MasterDetailPage => _masterDetailPage ?? (_masterDetailPage = (MasterDetailPage)Element);

		IMasterDetailPageController MasterDetailPageController => MasterDetailPage as IMasterDetailPageController;

		UIBarButtonItem PresentButton
		{
			get { return _innerDelegate == null ? null : _innerDelegate.PresentButton; }
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
					_masterController.WillAppear -= MasterControllerWillAppear;
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

			Delegate = _innerDelegate = new InnerDelegate(MasterDetailPage.MasterBehavior);

			UpdateControllers();

			_masterController.WillAppear += MasterControllerWillAppear;
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
			PageController.SendDisappearing();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (View.Subviews.Length < 2)
				return;

			var detailsBounds = _detailController.View.Frame;
			var masterBounds = _masterController.View.Frame;

			if (!masterBounds.IsEmpty)
				MasterDetailPageController.MasterBounds = new Rectangle(0, 0, masterBounds.Width, masterBounds.Height);

			if (!detailsBounds.IsEmpty)
				MasterDetailPageController.DetailBounds = new Rectangle(0, 0, detailsBounds.Width, detailsBounds.Height);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			UpdateBackground();
			_tracker = new VisualElementTracker(this);
			_events = new EventTracker(this);
			_events.LoadEvents(NativeView);
		}

		public override void ViewWillDisappear(bool animated)
		{
			if (_masterVisible)
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
			// On IOS8 the MasterViewController ViewAppear/Disappear weren't being called correctly after rotation 
			// We now close the Master by using the new SplitView API, basicly we set it to hidden and right back to the Normal/AutomaticMode
			if (!MasterDetailPageController.ShouldShowSplitMode && _masterVisible)
			{
				MasterDetailPageController.CanChangeIsPresented = true;
				if (Forms.IsiOS8OrNewer)
				{
					PreferredDisplayMode = UISplitViewControllerDisplayMode.PrimaryHidden;
					PreferredDisplayMode = UISplitViewControllerDisplayMode.Automatic;
				}
			}

			MasterDetailPageController.UpdateMasterBehavior();
			MessagingCenter.Send<IVisualElementRenderer>(this, NavigationRenderer.UpdateToolbarButtons);
			base.WillRotate(toInterfaceOrientation, duration);
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
			if (e.PropertyName == Page.IconProperty.PropertyName || e.PropertyName == Page.TitleProperty.PropertyName)
				MessagingCenter.Send<IVisualElementRenderer>(this, NavigationRenderer.UpdateToolbarButtons);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_tracker == null)
				return;

			if (e.PropertyName == "Master" || e.PropertyName == "Detail")
				UpdateControllers();
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
				ToggleMaster();
			else if (e.PropertyName == MasterDetailPage.IsGestureEnabledProperty.PropertyName)
				PresentsWithGesture = MasterDetailPage.IsGestureEnabled;
			MessagingCenter.Send<IVisualElementRenderer>(this, NavigationRenderer.UpdateToolbarButtons);
		}

		void MasterControllerWillAppear(object sender, EventArgs e)
		{
			_masterVisible = true;
			if (MasterDetailPageController.CanChangeIsPresented)
				ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, true);
		}

		void MasterControllerWillDisappear(object sender, EventArgs e)
		{
			_masterVisible = false;
			if (MasterDetailPageController.CanChangeIsPresented)
				ElementController.SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, false);
		}

		void PerformButtonSelector()
		{
			if (Forms.IsiOS8OrNewer)
				DisplayModeButtonItem.Target.PerformSelector(DisplayModeButtonItem.Action, DisplayModeButtonItem, 0);
			else
				PresentButton.Target.PerformSelector(PresentButton.Action, PresentButton, 0);
		}

		void ToggleMaster()
		{
			if (_masterVisible == MasterDetailPage.IsPresented || MasterDetailPageController.ShouldShowSplitMode)
				return;

			PerformButtonSelector();
		}

		void UpdateBackground()
		{
			if (!string.IsNullOrEmpty(((Page)Element).BackgroundImage))
				View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle(((Page)Element).BackgroundImage));
			else if (Element.BackgroundColor == Color.Default)
				View.BackgroundColor = UIColor.White;
			else
				View.BackgroundColor = Element.BackgroundColor.ToUIColor();
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

		class InnerDelegate : UISplitViewControllerDelegate
		{
			readonly MasterBehavior _masterPresentedDefaultState;

			public InnerDelegate(MasterBehavior masterPresentedDefaultState)
			{
				_masterPresentedDefaultState = masterPresentedDefaultState;
			}

			public UIBarButtonItem PresentButton { get; set; }

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

			public override void WillHideViewController(UISplitViewController svc, UIViewController aViewController, UIBarButtonItem barButtonItem, UIPopoverController pc)
			{
				PresentButton = barButtonItem;
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				platformEffect.Container = View;
		}
	}
}