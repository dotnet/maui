using System;
using System.Collections.Generic;
using System.ComponentModel;
using Foundation;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using PageUIStatusBarAnimation = Xamarin.Forms.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;

namespace Xamarin.Forms.Platform.iOS
{
	public class PageRenderer : UIViewController, IVisualElementRenderer, IEffectControlProvider, IAccessibilityElementsController
	{
		bool _appeared;
		bool _disposed;
		EventTracker _events;
		VisualElementPackager _packager;
		VisualElementTracker _tracker;

		internal PageContainer Container => NativeView as PageContainer;

		Page Page => Element as Page;
		IAccessibilityElementsController AccessibilityElementsController => this;

		bool UsingSafeArea => (Forms.IsiOS11OrNewer) ? Page.On<PlatformConfiguration.iOS>().UsingSafeArea() : false;
		Thickness SafeAreaInsets => Page.On<PlatformConfiguration.iOS>().SafeAreaInsets();

		public PageRenderer()
		{
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, NativeView);
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public List<NSObject> GetAccessibilityElements()
		{
			if (Container == null || Element == null)
				return null;

			var children = Element.Descendants();
			SortedDictionary<int, List<ITabStopElement>> tabIndexes = null;
			List<NSObject> views = new List<NSObject>();
			foreach (var child in children)
			{
				if (!(child is VisualElement ve))
					continue;

				tabIndexes = ve.GetSortedTabIndexesOnParentPage(out _);
				break;
			}

			if (tabIndexes == null)
				return null;

			foreach (var idx in tabIndexes?.Keys)
			{
				var tabGroup = tabIndexes[idx];
				foreach (var child in tabGroup)
				{
					if (child is Layout ||
						!(
							child is VisualElement ve && ve.IsTabStop
							&& AutomationProperties.GetIsInAccessibleTree(ve) != false // accessible == true
							&& ve.GetRenderer().NativeView is ITabStop tabStop)
						 )
						continue;

					var thisControl = tabStop.TabStop;

					if (thisControl == null)
						continue;

					if (views.Contains(thisControl))
						break; // we've looped to the beginning

					views.Add(thisControl);
				}
			}

			return views;
		}

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

			if (element != null)
			{
				if (!string.IsNullOrEmpty(element.AutomationId))
					SetAutomationId(element.AutomationId);

				element.SendViewInitialized(NativeView);
			}

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public override void LoadView()
		{
			View = new PageContainer(this);
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			AccessibilityElementsController.ResetAccessibilityElements();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (_disposed)
				return;

			if (Element.Parent is BaseShellItem)
				Element.Layout(View.Bounds.ToRectangle());

			UpdateShellInsetPadding();
		}

		public override void ViewSafeAreaInsetsDidChange()
		{
			UpdateShellInsetPadding();
			if (Page != null && Forms.IsiOS11OrNewer)
			{
				var insets = NativeView.SafeAreaInsets;
				if (Page.Parent is TabbedPage)
				{
					insets.Bottom = 0;
				}
				Page.On<PlatformConfiguration.iOS>().SetSafeAreaInsets(new Thickness(insets.Left, insets.Top, insets.Right, insets.Bottom));
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
			UpdateStatusBarPrefersHidden();
			if(Forms.IsiOS11OrNewer)
				SetNeedsUpdateOfHomeIndicatorAutoHidden();

			if (Element.Parent is CarouselPage)
				return;

			Page.SendAppearing();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);

			if (!_appeared || _disposed)
				return;

			_appeared = false;

			if (Element.Parent is CarouselPage)
				return;

			Page.SendDisappearing();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (NativeView == null)
				return;

			var uiTapGestureRecognizer = new UITapGestureRecognizer(a => NativeView.EndEditing(true));

			uiTapGestureRecognizer.ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true;
			uiTapGestureRecognizer.ShouldReceiveTouch = OnShouldReceiveTouch;
			uiTapGestureRecognizer.DelaysTouchesBegan =
				uiTapGestureRecognizer.DelaysTouchesEnded = uiTapGestureRecognizer.CancelsTouchesInView = false;
			NativeView.AddGestureRecognizer(uiTapGestureRecognizer);

			UpdateBackground();

			_packager = new VisualElementPackager(this);
			_packager.Load();

			Element.PropertyChanged += OnHandlePropertyChanged;
			_tracker = new VisualElementTracker(this, !(Element.Parent is BaseShellItem));

			_events = new EventTracker(this);
			_events.LoadEvents(NativeView);

			Element.SendViewInitialized(NativeView);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			NativeView?.Window?.EndEditing(true);
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
				Container?.Dispose();
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
			else if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle();
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName)
				UpdateStatusBarPrefersHidden();
			else if (Forms.IsiOS11OrNewer && e.PropertyName == PlatformConfiguration.iOSSpecific.Page.UseSafeAreaProperty.PropertyName)
				UpdateUseSafeArea();
			else if (Forms.IsiOS11OrNewer && e.PropertyName == PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty.PropertyName)
				UpdateUseSafeArea();
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName)
				UpdateHomeIndicatorAutoHidden();
		}

		public override UIKit.UIStatusBarAnimation PreferredStatusBarUpdateAnimation
		{
			get
			{
				var animation = Page.OnThisPlatform().PreferredStatusBarUpdateAnimation();
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

		void IAccessibilityElementsController.ResetAccessibilityElements()
		{
			Container?.ClearAccessibilityElements();
		}

		void UpdateUseSafeArea()
		{
			if (!Forms.IsiOS11OrNewer)
				return;

			if (!UsingSafeArea)
			{
				var safeAreaInsets = SafeAreaInsets;
				if (safeAreaInsets == Page.Padding)
					Page.Padding = default(Thickness);
			}
			else
			{
				Page.Padding = SafeAreaInsets;
			}
		}

		void UpdateShellInsetPadding()
		{
			if (!(Element?.Parent is ShellContent))
				return;

			nfloat topPadding = 0;
			nfloat bottomPadding = 0;

			if (Forms.IsiOS11OrNewer)
			{
				topPadding = View.SafeAreaInsets.Top;
				bottomPadding = View.SafeAreaInsets.Bottom;
			}
			else
			{
				topPadding = TopLayoutGuide.Length;
				bottomPadding = BottomLayoutGuide.Length;
			}

			Page.Padding = new Thickness(0, topPadding, 0, bottomPadding);
		}

		void UpdateStatusBarPrefersHidden()
		{
			if (Element == null)
				return;

			var animation = Page.OnThisPlatform().PreferredStatusBarUpdateAnimation();
			if (animation == PageUIStatusBarAnimation.Fade || animation == PageUIStatusBarAnimation.Slide)
				UIView.Animate(0.25, () => SetNeedsStatusBarAppearanceUpdate());
			else
				SetNeedsStatusBarAppearanceUpdate();
			NativeView?.SetNeedsLayout();
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
			var mode = Page.OnThisPlatform().PrefersStatusBarHidden();
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
			if (NativeView == null)
				return;

			_ = this.ApplyNativeImageAsync(Page.BackgroundImageSourceProperty, bgImage =>
			{
				if (NativeView == null)
					return;

				if (bgImage != null)
					NativeView.BackgroundColor = UIColor.FromPatternImage(bgImage);
				else if (Element.BackgroundColor.IsDefault)
					NativeView.BackgroundColor = UIColor.White;
				else
					NativeView.BackgroundColor = Element.BackgroundColor.ToUIColor();
			});
		}

		void UpdateTitle()
		{
			if (!string.IsNullOrWhiteSpace(Page.Title))
				NavigationItem.Title = Page.Title;
		}

		IEnumerable<UIView> ViewAndSuperviewsOfView(UIView view)
		{
			while (view != null)
			{
				yield return view;
				view = view.Superview;
			}
		}

		void UpdateHomeIndicatorAutoHidden()
		{
			if (Element == null || !Forms.IsiOS11OrNewer)
				return;

			SetNeedsUpdateOfHomeIndicatorAutoHidden();
		}

		public override bool PrefersHomeIndicatorAutoHidden => Page.OnThisPlatform().PrefersHomeIndicatorAutoHidden();
	}
}