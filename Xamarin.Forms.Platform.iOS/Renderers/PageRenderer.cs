using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using UIKit;
using PageUIStatusBarAnimation = Xamarin.Forms.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Platform.iOS
{
	public class PageRenderer : UIViewController, IVisualElementRenderer, IEffectControlProvider
	{
		bool _appeared;
		bool _disposed;
		EventTracker _events;
		VisualElementPackager _packager;
		VisualElementTracker _tracker;

		Page Page => Element as Page;

		public PageRenderer()
		{
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, View);
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public UIView NativeView
		{
			get { return _disposed ? null : View; }
		}

		public void SetElement(VisualElement element)
		{
			VisualElement oldElement = Element;
			Element = element;
			UpdateTitle();

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				SetAutomationId(Element.AutomationId);

			if (element != null)
				element.SendViewInitialized(NativeView);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public override void ViewSafeAreaInsetsDidChange()
		{

			var page = (Element as Page);
			if (page != null && Forms.IsiOS11OrNewer)
			{
				var insets = NativeView.SafeAreaInsets;
				if(page.Parent is TabbedPage)
				{
					insets.Bottom = 0;
				}
				page.On<PlatformConfiguration.iOS>().SetSafeAreaInsets(new Thickness(insets.Left, insets.Top, insets.Right, insets.Bottom));
			
			}
			base.ViewSafeAreaInsetsDidChange();
		}

		public UIViewController ViewController => _disposed ? null : this;

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (_appeared || _disposed)
				return;

			_appeared = true;
			Page.SendAppearing();
			UpdateStatusBarPrefersHidden();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);

			if (!_appeared || _disposed)
				return;

			_appeared = false;
			Page.SendDisappearing();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var uiTapGestureRecognizer = new UITapGestureRecognizer(a => View.EndEditing(true));

			uiTapGestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true;
			uiTapGestureRecognizer.ShouldReceiveTouch = OnShouldReceiveTouch;
			uiTapGestureRecognizer.DelaysTouchesBegan =
				uiTapGestureRecognizer.DelaysTouchesEnded = uiTapGestureRecognizer.CancelsTouchesInView = false;
			View.AddGestureRecognizer(uiTapGestureRecognizer);

			UpdateBackground();

			_packager = new VisualElementPackager(this);
			_packager.Load();

			Element.PropertyChanged += OnHandlePropertyChanged;
			_tracker = new VisualElementTracker(this);

			_events = new EventTracker(this);
			_events.LoadEvents(View);

			Element.SendViewInitialized(View);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			View.Window?.EndEditing(true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				Element.PropertyChanged -= OnHandlePropertyChanged;
				Platform.SetRenderer(Element, null);
				if (_appeared)
					Page.SendDisappearing();

				_appeared = false;

				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}

				if (_packager != null)
				{
					_packager.Dispose();
					_packager = null;
				}

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				Element = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			ElementChanged?.Invoke(this, e);
		}

		protected virtual void SetAutomationId(string id)
		{
			if (NativeView != null)
				NativeView.AccessibilityIdentifier = id;
		}

		void OnHandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle();
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName)
				UpdateStatusBarPrefersHidden();
		}

		public override UIKit.UIStatusBarAnimation PreferredStatusBarUpdateAnimation
		{
			get
			{
				var animation = ((Page)Element).OnThisPlatform().PreferredStatusBarUpdateAnimation();
				switch (animation)
				{
					case (PageUIStatusBarAnimation.Fade):
						return UIKit.UIStatusBarAnimation.Fade;
					case (PageUIStatusBarAnimation.Slide):
						return UIKit.UIStatusBarAnimation.Slide;
					case (PageUIStatusBarAnimation.None):
					default:
						return UIKit.UIStatusBarAnimation.None;
				}
			}
		}

		void UpdateStatusBarPrefersHidden()
		{
			if (Element == null)
				return;

			var animation = ((Page)Element).OnThisPlatform().PreferredStatusBarUpdateAnimation();
			if (animation == PageUIStatusBarAnimation.Fade || animation == PageUIStatusBarAnimation.Slide)
				UIView.Animate(0.25, () => SetNeedsStatusBarAppearanceUpdate());
			else
				SetNeedsStatusBarAppearanceUpdate();
			View.SetNeedsLayout();
		}

		bool OnShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			foreach (UIView v in ViewAndSuperviewsOfView(touch.View))
			{
				if (v is UITableView || v is UITableViewCell || v.CanBecomeFirstResponder)
					return false;
			}
			return true;
		}

		public override bool PrefersStatusBarHidden()
		{
			var mode = ((Page)Element).OnThisPlatform().PrefersStatusBarHidden();
			switch (mode)
			{
				case (StatusBarHiddenMode.True):
					return true;
				case (StatusBarHiddenMode.False):
					return false;
				case (StatusBarHiddenMode.Default):
				default:
					return base.PrefersStatusBarHidden();
			}
		}

		void UpdateBackground()
		{
			string bgImage = ((Page)Element).BackgroundImage;
			if (!string.IsNullOrEmpty(bgImage))
			{
				View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle(bgImage) ?? throw new Exception($"Image: File '{bgImage}' not found in app bundle"));
				return;
			}
			Color bgColor = Element.BackgroundColor;
			if (bgColor.IsDefault)
				View.BackgroundColor = UIColor.White;
			else
				View.BackgroundColor = bgColor.ToUIColor();
		}

		void UpdateTitle()
		{
			if (!string.IsNullOrWhiteSpace(((Page)Element).Title))
				Title = ((Page)Element).Title;
		}

		IEnumerable<UIView> ViewAndSuperviewsOfView(UIView view)
		{
			while (view != null)
			{
				yield return view;
				view = view.Superview;
			}
		}
	}
}