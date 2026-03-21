using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Versioning;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using PageSpecific = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using PageUIStatusBarAnimation = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class PageRenderer : UIViewController, IVisualElementRenderer, IEffectControlProvider, IShellContentInsetObserver, Controls.Platform.Compatibility.IDisconnectable
	{
		bool _appeared;
		bool _disposed;
		VisualElementPackager _packager;
		VisualElementTracker _tracker;

		// storing this into a local variable causes it to not get collected. Do not delete this please		
		PageContainer _pageContainer;
		internal PageContainer Container => NativeView as PageContainer;

		Page Page => Element as Page;
		Thickness SafeAreaInsets => Page.On<PlatformConfiguration.iOS>().SafeAreaInsets();
		bool IsPartOfShell => (Element?.Parent is BaseShellItem);
		ShellSection _shellSection;
		bool _safeAreasSet = false;
		Thickness _userPadding = default(Thickness);
		bool _userOverriddenSafeArea = false;

		[Preserve(Conditional = true)]
		public PageRenderer()
		{
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, NativeView);
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

			if (element != null)
			{
				if (!string.IsNullOrEmpty(element.AutomationId))
					SetAutomationId(element.AutomationId);

				element.SendViewInitialized(NativeView);

				var parent = Element.Parent;

				while (!Application.IsApplicationOrNull(parent))
				{
					if (parent is ShellContent)
						_isInItems = true;

					if (parent is ShellSection shellSection)
					{
						_shellSection = shellSection;
						((IShellSectionController)_shellSection).AddContentInsetObserver(this);

						break;
					}

					parent = parent.Parent;
				}
			}

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public void SetElementSize(Size size)
		{
			// In Split Mode the Frame will occasionally get set to the wrong value
			var rect = new CoreGraphics.CGRect(Element.X, Element.Y, size.Width, size.Height);
			if (rect != _pageContainer.Frame)
				_pageContainer.Frame = rect;

			Element.Layout(new Rect(Element.X, Element.Y, size.Width, size.Height));
		}

		public override void LoadView()
		{

			//by default use the MainScreen Bounds so Effects can access the Container size
			if (_pageContainer == null)
			{
				var bounds = UIApplication.SharedApplication?.GetKeyWindow()?.Bounds ??
					UIScreen.MainScreen.Bounds;

				_pageContainer = new PageContainer() { Frame = bounds };
			}

			View = _pageContainer;
		}
		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (_disposed || Element == null)
				return;

			if (Element.Parent is BaseShellItem)
				Element.Layout(View.Bounds.ToRectangle());

			if (_safeAreasSet || !Forms.IsiOS11OrNewer)
				UpdateUseSafeArea();

			if (Element.Background != null && !Element.Background.IsEmpty)
				NativeView?.UpdateBackgroundLayer();
		}

		[SupportedOSPlatform("ios11.0")]
		[SupportedOSPlatform("tvos11.0")]
		public override void ViewSafeAreaInsetsDidChange()
		{
			_safeAreasSet = true;
			UpdateUseSafeArea();
			base.ViewSafeAreaInsetsDidChange();
		}

		public UIViewController ViewController => _disposed ? null : this;

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (_appeared || _disposed || Element == null)
				return;

			_appeared = true;
			UpdateStatusBarPrefersHidden();
			if (OperatingSystem.IsIOSVersionAtLeast(11))
				SetNeedsUpdateOfHomeIndicatorAutoHidden();

			Page.SendAppearing();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);

			if (!_appeared || _disposed || Element == null)
				return;

			_appeared = false;

			Page.SendDisappearing();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (NativeView == null)
				return;

			var uiTapGestureRecognizer = new UITapGestureRecognizer(a => NativeView?.EndEditing(true));

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

			Element.SendViewInitialized(NativeView);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			NativeView?.Window?.EndEditing(true);
		}

		void Controls.Platform.Compatibility.IDisconnectable.Disconnect()
		{
			if (_shellSection != null)
			{
				((IShellSectionController)_shellSection).RemoveContentInsetObserver(this);
				_shellSection = null;
			}

			if (Element != null)
			{
				Element.PropertyChanged -= OnHandlePropertyChanged;
				Platform.SetRenderer(Element, null);

				if (_appeared)
					Page.SendDisappearing();

				Element = null;
			}

			_packager?.Disconnect();
			_tracker?.Disconnect();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				(this as Controls.Platform.Compatibility.IDisconnectable).Disconnect();

				_packager?.Dispose();
				_tracker?.Dispose();
				_packager = null;
				_tracker = null;

				Element = null;
				Container?.Dispose();
				_pageContainer = null;
			}

			_disposed = true;

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
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle();
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName)
				UpdateStatusBarPrefersHidden();
			else if (Forms.IsiOS11OrNewer && e.PropertyName == PlatformConfiguration.iOSSpecific.Page.UseSafeAreaProperty.PropertyName)
			{
				_userOverriddenSafeArea = false;
				UpdateUseSafeArea();
			}
			else if (Forms.IsiOS11OrNewer && e.PropertyName == PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty.PropertyName)
				UpdateUseSafeArea();
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName)
				UpdateHomeIndicatorAutoHidden();
			else if (e.PropertyName == Page.PaddingProperty.PropertyName)
			{
				if (ShouldUseSafeArea() && Page.Padding != SafeAreaInsets)
					_userOverriddenSafeArea = true;
			}
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

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
#pragma warning disable CA1422 // Validate platform compatibility
			base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility

			if (Forms.IsiOS13OrNewer &&
				previousTraitCollection.UserInterfaceStyle != TraitCollection.UserInterfaceStyle &&
				UIApplication.SharedApplication.ApplicationState != UIApplicationState.Background)
				((IApplication)Application.Current)?.ThemeChanged();
		}

		bool ShouldUseSafeArea()
		{
			bool usingSafeArea = Page.On<PlatformConfiguration.iOS>().UsingSafeArea();
			bool isSafeAreaSet = Element.IsSet(PageSpecific.UseSafeAreaProperty);

			if (IsPartOfShell && !isSafeAreaSet)
				usingSafeArea = true;

			return usingSafeArea;
		}

		void UpdateUseSafeArea()
		{
			if (Element == null)
				return;

			if (_userOverriddenSafeArea)
				return;

			if (!IsPartOfShell && !Forms.IsiOS11OrNewer)
				return;

			var tabThickness = _tabThickness;
			if (!_isInItems)
				tabThickness = 0;

			Thickness safeareaPadding = default(Thickness);

			if (Page.Padding != SafeAreaInsets)
				_userPadding = Page.Padding;

			if (Forms.IsiOS11OrNewer)
			{
				var insets = NativeView.SafeAreaInsets;
				if (Page.Parent is TabbedPage)
				{
					insets.Bottom = 0;
				}

				safeareaPadding = new Thickness(insets.Left, insets.Top + tabThickness, insets.Right, insets.Bottom);
				Page.On<PlatformConfiguration.iOS>().SetSafeAreaInsets(safeareaPadding);
			}
			else if (IsPartOfShell)
			{
				safeareaPadding = new Thickness(0, TopLayoutGuide.Length + tabThickness, 0, BottomLayoutGuide.Length);
				Page.On<PlatformConfiguration.iOS>().SetSafeAreaInsets(safeareaPadding);
			}

			bool usingSafeArea = Page.On<PlatformConfiguration.iOS>().UsingSafeArea();
			bool isSafeAreaSet = Element.IsSet(PageSpecific.UseSafeAreaProperty);

			if (IsPartOfShell && !isSafeAreaSet)
			{
				if (Shell.GetNavBarIsVisible(Element) || _tabThickness != default(Thickness))
					usingSafeArea = true;
			}

			if (!usingSafeArea && isSafeAreaSet && Page.Padding == safeareaPadding)
			{
				Page.SetValueFromRenderer(Page.PaddingProperty, _userPadding);
			}

			if (!usingSafeArea)
				return;

			if (SafeAreaInsets == Page.Padding)
				return;

			// this is determining if there is a UIScrollView control occupying the whole screen
			if (IsPartOfShell && !isSafeAreaSet)
			{
				var subViewSearch = View;
				for (int i = 0; i < 2 && subViewSearch != null; i++)
				{
					if (subViewSearch?.Subviews.Length > 0)
					{
						if (subViewSearch.Subviews[0] is UIScrollView)
							return;

						subViewSearch = subViewSearch.Subviews[0];
					}
					else
					{
						subViewSearch = null;
					}
				}
			}

			Page.SetValueFromRenderer(Page.PaddingProperty, SafeAreaInsets);
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

		[PortHandler]
		bool OnShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			foreach (UIView v in ViewAndSuperviewsOfView(touch.View))
			{
				if (v != null && (v is UITableView || v is UITableViewCell || v.CanBecomeFirstResponder))
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
				else
				{
					Brush background = Element.Background;

					if (!Brush.IsNullOrEmpty(background))
						NativeView.UpdateBackground(Element.Background);
					else
					{
						NativeView.BackgroundColor = Element.BackgroundColor?.ToPlatform() ?? Maui.Platform.ColorExtensions.BackgroundColor;
					}
				}
			});
		}

		void UpdateTitle()
		{
			if (!string.IsNullOrWhiteSpace(Page.Title))
				NavigationItem.Title = Page.Title;
		}

		[PortHandler]
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
			if (Element == null || !OperatingSystem.IsIOSVersionAtLeast(11))
				return;

			SetNeedsUpdateOfHomeIndicatorAutoHidden();
		}

		double _tabThickness;
		bool _isInItems;

		void IShellContentInsetObserver.OnInsetChanged(Thickness inset, double tabThickness)
		{
			if (_tabThickness != tabThickness)
			{
				_safeAreasSet = true;
				_tabThickness = tabThickness;
				UpdateUseSafeArea();
			}
		}

		public override bool PrefersHomeIndicatorAutoHidden => Page.OnThisPlatform().PrefersHomeIndicatorAutoHidden();
	}
}
