using System;
using System.ComponentModel;
using System.Linq;
using UIKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using PointF = CoreGraphics.CGPoint;

namespace Xamarin.Forms.Platform.iOS
{
	public class PhoneMasterDetailRenderer : UIViewController, IVisualElementRenderer, IEffectControlProvider
	{
		UIView _clickOffView;
		UIViewController _detailController;

		bool _disposed;
		EventTracker _events;

		UIViewController _masterController;

		UIPanGestureRecognizer _panGesture;

		bool _presented;
		UIGestureRecognizer _tapGesture;

		VisualElementTracker _tracker;

		Page Page => Element as Page;
		

		public PhoneMasterDetailRenderer()
		{
		}

		MasterDetailPage MasterDetailPage => Element as MasterDetailPage;

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

				((IElementController)Element).SetValueFromRenderer(Xamarin.Forms.MasterDetailPage.IsPresentedProperty, value);
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

			_masterController = new ChildViewController();
			_detailController = new ChildViewController();

			_clickOffView = new UIView();
			_clickOffView.BackgroundColor = new Color(0, 0, 0, 0).ToUIColor();

			Presented = ((MasterDetailPage)Element).IsPresented;

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (element != null)
				element.SendViewInitialized(NativeView);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
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

			LayoutChildren(false);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_tracker = new VisualElementTracker(this);
			_events = new EventTracker(this);
			_events.LoadEvents(View);

			((MasterDetailPage)Element).PropertyChanged += HandlePropertyChanged;

			_tapGesture = new UITapGestureRecognizer(() =>
			{
				if (Presented)
					Presented = false;
			});
			_clickOffView.AddGestureRecognizer(_tapGesture);

			PackContainers();
			UpdateMasterDetailContainers();

			UpdateBackground();

			UpdatePanGesture();
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			if (!MasterDetailPage.ShouldShowSplitMode && _presented)
				Presented = false;

			base.WillRotate(toInterfaceOrientation, duration);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				Element.SizeChanged -= PageOnSizeChanged;
				Element.PropertyChanged -= HandlePropertyChanged;

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
			foreach (var child in _detailController.View.Subviews.Concat(_masterController.View.Subviews))
				child.RemoveFromSuperview();

			foreach (var vc in _detailController.ChildViewControllers.Concat(_masterController.ChildViewControllers))
				vc.RemoveFromParentViewController();
		}

		void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.IconProperty.PropertyName || e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateLeftBarButton();
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Master" || e.PropertyName == "Detail")
				UpdateMasterDetailContainers();
			else if (e.PropertyName == Xamarin.Forms.MasterDetailPage.IsPresentedProperty.PropertyName)
				Presented = ((MasterDetailPage)Element).IsPresented;
			else if (e.PropertyName == Xamarin.Forms.MasterDetailPage.IsGestureEnabledProperty.PropertyName)
				UpdatePanGesture();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackground();
		}

		void LayoutChildren(bool animated)
		{
			var frame = Element.Bounds.ToRectangleF();
			var masterFrame = frame;
			masterFrame.Width = (int)(Math.Min(masterFrame.Width, masterFrame.Height) * 0.8);

			var isRTL = (Element as IVisualElementController)?.EffectiveFlowDirection.IsRightToLeft() == true;
			if (isRTL)
			{
				masterFrame.X = (int)(masterFrame.Width * .25);
			}

			_masterController.View.Frame = masterFrame;

			var target = frame;
			if (Presented)
				target.X += masterFrame.Width;

			if (isRTL)
			{
				target.X = target.X * -1;
			}

			if (animated)
			{
				UIView.BeginAnimations("Flyout");
				var view = _detailController.View;
				view.Frame = target;
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseOut);
				UIView.SetAnimationDuration(250);
				UIView.CommitAnimations();
			}
			else
				_detailController.View.Frame = target;

			MasterDetailPage.MasterBounds = new Rectangle(masterFrame.X, 0, masterFrame.Width, masterFrame.Height);
			MasterDetailPage.DetailBounds = new Rectangle(0, 0, frame.Width, frame.Height);

			if (Presented)
				_clickOffView.Frame = _detailController.View.Frame;
		}

		void PackContainers()
		{
			_detailController.View.BackgroundColor = new UIColor(1, 1, 1, 1);
			View.AddSubview(_masterController.View);
			View.AddSubview(_detailController.View);

			AddChildViewController(_masterController);
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
			if (!string.IsNullOrEmpty(((Page)Element).BackgroundImage))
				View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle(((Page)Element).BackgroundImage));
			else if (Element.BackgroundColor == Color.Default)
				View.BackgroundColor = UIColor.White;
			else
				View.BackgroundColor = Element.BackgroundColor.ToUIColor();
		}

		void UpdateMasterDetailContainers()
		{
			((MasterDetailPage)Element).Master.PropertyChanged -= HandleMasterPropertyChanged;

			EmptyContainers();

			if (Platform.GetRenderer(((MasterDetailPage)Element).Master) == null)
				Platform.SetRenderer(((MasterDetailPage)Element).Master, Platform.CreateRenderer(((MasterDetailPage)Element).Master));
			if (Platform.GetRenderer(((MasterDetailPage)Element).Detail) == null)
				Platform.SetRenderer(((MasterDetailPage)Element).Detail, Platform.CreateRenderer(((MasterDetailPage)Element).Detail));

			var masterRenderer = Platform.GetRenderer(((MasterDetailPage)Element).Master);
			var detailRenderer = Platform.GetRenderer(((MasterDetailPage)Element).Detail);

			((MasterDetailPage)Element).Master.PropertyChanged += HandleMasterPropertyChanged;

			_masterController.View.AddSubview(masterRenderer.NativeView);
			_masterController.AddChildViewController(masterRenderer.ViewController);

			_detailController.View.AddSubview(detailRenderer.NativeView);
			_detailController.AddChildViewController(detailRenderer.ViewController);

			SetNeedsStatusBarAppearanceUpdate();
		}

		void UpdateLeftBarButton()
		{
			var masterDetailPage = Element as MasterDetailPage;
			if (!(masterDetailPage?.Detail is NavigationPage))
				return;

			var detailRenderer = Platform.GetRenderer(masterDetailPage.Detail) as UINavigationController;

			UIViewController firstPage = detailRenderer?.ViewControllers.FirstOrDefault();
			if (firstPage != null)
				NavigationRenderer.SetMasterLeftBarButton(firstPage, masterDetailPage);
		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			if (((MasterDetailPage)Element).Detail != null)
				return (UIViewController)Platform.GetRenderer(((MasterDetailPage)Element).Detail);
			else
				return base.ChildViewControllerForStatusBarHidden();
		}

		void UpdatePanGesture()
		{
			var model = (MasterDetailPage)Element;
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

			UITouchEventArgs shouldReceive = (g, t) => !(t.View is UISlider);
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
							targetFrame.X = (nfloat)Math.Max(0, _masterController.View.Frame.Width + Math.Min(0, motion));
						else
							targetFrame.X = (nfloat)Math.Min(_masterController.View.Frame.Width, Math.Max(0, motion));

						targetFrame.X = targetFrame.X * directionModifier;

						detailView.Frame = targetFrame;
						break;
					case UIGestureRecognizerState.Ended:
						var detailFrame = _detailController.View.Frame;
						var masterFrame = _masterController.View.Frame;
						if (Presented)
						{
							if (detailFrame.X * directionModifier < masterFrame.Width * .75)
								Presented = false;
							else
								LayoutChildren(true);
						}
						else
						{
							if (detailFrame.X * directionModifier > masterFrame.Width * .25)
								Presented = true;
							else
								LayoutChildren(true);
						}
						break;
				}
			});
			_panGesture.ShouldReceiveTouch = shouldReceive;
			_panGesture.MaximumNumberOfTouches = 2;
			View.AddGestureRecognizer(_panGesture);
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
	}
}