using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UIKit;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page;
using PageUIStatusBarAnimation = Xamarin.Forms.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;

namespace Xamarin.Forms.Platform.iOS
{
	public class TabbedRenderer : UITabBarController, IVisualElementRenderer, IEffectControlProvider
	{
		bool _barBackgroundColorWasSet;
		bool _barTextColorWasSet;
		UIColor _defaultBarTextColor;
		bool _defaultBarTextColorSet;
		UIColor _defaultBarColor;
		bool _defaultBarColorSet;
		bool _loaded;
		Size _queuedSize;

		IPageController PageController => Element as IPageController;
		IElementController ElementController => Element as IElementController;

		public override UIViewController SelectedViewController
		{
			get { return base.SelectedViewController; }
			set
			{
				base.SelectedViewController = value;
				UpdateCurrentPage();
			}
		}

		protected TabbedPage Tabbed
		{
			get { return (TabbedPage)Element; }
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

			FinishedCustomizingViewControllers += HandleFinishedCustomizingViewControllers;
			Tabbed.PropertyChanged += OnPropertyChanged;
			Tabbed.PagesChanged += OnPagesChanged;

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			OnPagesChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			if (element != null)
				element.SendViewInitialized(NativeView);

			//disable edit/reorder of tabs
			CustomizableViewControllers = null;

			UpdateBarBackgroundColor();
			UpdateBarTextColor();

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public void SetElementSize(Size size)
		{
			if (_loaded)
				Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
			else
				_queuedSize = size;
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			View.SetNeedsLayout();
		}

		public override void ViewDidAppear(bool animated)
		{
			PageController.SendAppearing();
			base.ViewDidAppear(animated);
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			PageController.SendDisappearing();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (Element == null)
				return;

			if (!Element.Bounds.IsEmpty)
			{
				View.Frame = new System.Drawing.RectangleF((float)Element.X, (float)Element.Y, (float)Element.Width, (float)Element.Height);
			}

			var frame = View.Frame;
			var tabBarFrame = TabBar.Frame;
			PageController.ContainerArea = new Rectangle(0, 0, frame.Width, frame.Height - tabBarFrame.Height);

			if (!_queuedSize.IsZero)
			{
				Element.Layout(new Rectangle(Element.X, Element.Y, _queuedSize.Width, _queuedSize.Height));
				_queuedSize = Size.Zero;
			}

			_loaded = true;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (!Forms.IsiOS7OrNewer)
				WantsFullScreenLayout = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				PageController.SendDisappearing();
				Tabbed.PropertyChanged -= OnPropertyChanged;
				Tabbed.PagesChanged -= OnPagesChanged;
				FinishedCustomizingViewControllers -= HandleFinishedCustomizingViewControllers;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		UIViewController GetViewController(Page page)
		{
			var renderer = Platform.GetRenderer(page);
			if (renderer == null)
				return null;

			return renderer.ViewController;
		}

		void HandleFinishedCustomizingViewControllers(object sender, UITabBarCustomizeChangeEventArgs e)
		{
			if (e.Changed)
				UpdateChildrenOrderIndex(e.ViewControllers);
		}

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				var page = (Page)sender;
				var renderer = Platform.GetRenderer(page);
				if (renderer == null)
					return;

				if (renderer.ViewController.TabBarItem != null)
					renderer.ViewController.TabBarItem.Title = page.Title;
			}
			else if (e.PropertyName == Page.IconProperty.PropertyName)
			{
				var page = (Page)sender;

				var renderer = Platform.GetRenderer(page);
				if (renderer == null)
					return;

				if (renderer.ViewController.TabBarItem == null)
					return;

				UIImage image = null;
				if (!string.IsNullOrEmpty(page.Icon))
					image = new UIImage(page.Icon);

				// the new UITabBarItem forces redraw, setting the UITabBarItem.Image does not
				renderer.ViewController.TabBarItem = new UITabBarItem(page.Title, image, 0);

				if (image != null)
					image.Dispose();
			}
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			e.Apply((o, i, c) => SetupPage((Page)o, i), (o, i) => TeardownPage((Page)o, i), Reset);

			SetControllers();

			UIViewController controller = null;
			if (Tabbed.CurrentPage != null)
				controller = GetViewController(Tabbed.CurrentPage);
			if (controller != null && controller != base.SelectedViewController)
				base.SelectedViewController = controller;
		}

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TabbedPage.CurrentPage))
			{
				var current = Tabbed.CurrentPage;
				if (current == null)
					return;

				var controller = GetViewController(current);
				if (controller == null)
					return;

				SelectedViewController = controller;
			}
			else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
				UpdateBarBackgroundColor();
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
			else if (e.PropertyName == PrefersStatusBarHiddenProperty.PropertyName)
				UpdatePrefersStatusBarHiddenOnPages();
			else if (e.PropertyName == PreferredStatusBarUpdateAnimationProperty.PropertyName)
				UpdateCurrentPagePreferredStatusBarUpdateAnimation();
		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			return GetViewController(Tabbed.CurrentPage);
		}

		void UpdateCurrentPagePreferredStatusBarUpdateAnimation()
		{
			PageUIStatusBarAnimation animation = ((Page)Element).OnThisPlatform().PreferredStatusBarUpdateAnimation();
			Tabbed.CurrentPage.OnThisPlatform().SetPreferredStatusBarUpdateAnimation(animation);
		}

		void UpdatePrefersStatusBarHiddenOnPages()
		{
			for (var i = 0; i < ViewControllers.Length; i++)
			{
				Tabbed.GetPageByIndex(i).OnThisPlatform().SetPrefersStatusBarHidden(Tabbed.OnThisPlatform().PrefersStatusBarHidden());
			}
		}

		void Reset()
		{
			var i = 0;
			foreach (var page in Tabbed.Children)
				SetupPage(page, i++);
		}

		void SetControllers()
		{
			var list = new List<UIViewController>();
			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				var child = ElementController.LogicalChildren[i];
				var v = child as VisualElement;
				if (v == null)
					continue;
				if (Platform.GetRenderer(v) != null)
					list.Add(Platform.GetRenderer(v).ViewController);
			}
			ViewControllers = list.ToArray();
		}

		void SetupPage(Page page, int index)
		{
			var renderer = Platform.GetRenderer(page);
			if (renderer == null)
			{
				renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);
			}

			page.PropertyChanged += OnPagePropertyChanged;

			UIImage icon = null;
			if (page.Icon != null)
				icon = new UIImage(page.Icon);

			renderer.ViewController.TabBarItem = new UITabBarItem(page.Title, icon, 0);
			if (icon != null)
				icon.Dispose();

			renderer.ViewController.TabBarItem.Tag = index;
		}

		void TeardownPage(Page page, int index)
		{
			page.PropertyChanged -= OnPagePropertyChanged;

			Platform.SetRenderer(page, null);
		}

		void UpdateBarBackgroundColor()
		{
			if (Tabbed == null || TabBar == null)
				return;

			var barBackgroundColor = Tabbed.BarBackgroundColor;
			var isDefaultColor = barBackgroundColor.IsDefault;

			if (isDefaultColor && !_barBackgroundColorWasSet)
				return;

			if (!_defaultBarColorSet)
			{
				if (Forms.IsiOS7OrNewer)
					_defaultBarColor = TabBar.BarTintColor;
				else
					_defaultBarColor = TabBar.TintColor;

				_defaultBarColorSet = true;
			}

			if (!isDefaultColor)
				_barBackgroundColorWasSet = true;

			if (Forms.IsiOS7OrNewer)
			{
				TabBar.BarTintColor = isDefaultColor ? _defaultBarColor : barBackgroundColor.ToUIColor();
			}
			else
			{
				TabBar.TintColor = isDefaultColor ? _defaultBarColor : barBackgroundColor.ToUIColor();
			}
		}

		void UpdateBarTextColor()
		{
			if (Tabbed == null || TabBar == null || TabBar.Items == null)
				return;

			var barTextColor = Tabbed.BarTextColor;
			var isDefaultColor = barTextColor.IsDefault;

			if (isDefaultColor && !_barTextColorWasSet)
				return;

			if (!_defaultBarTextColorSet)
			{
				_defaultBarTextColor = TabBar.TintColor;
				_defaultBarTextColorSet = true;
			}

			if (!isDefaultColor)
				_barTextColorWasSet = true;

			var attributes = new UITextAttributes();

			if (isDefaultColor)
				attributes.TextColor = _defaultBarTextColor;
			else
				attributes.TextColor = barTextColor.ToUIColor();

			foreach (UITabBarItem item in TabBar.Items)
			{
				item.SetTitleTextAttributes(attributes, UIControlState.Normal);
			}

			// set TintColor for selected icon
			// setting the unselected icon tint is not supported by iOS
			if (Forms.IsiOS7OrNewer)
			{
				TabBar.TintColor = isDefaultColor ? _defaultBarTextColor : barTextColor.ToUIColor();
			}
		}

		void UpdateChildrenOrderIndex(UIViewController[] viewControllers)
		{
			for (var i = 0; i < viewControllers.Length; i++)
			{
				var originalIndex = -1;
				if (int.TryParse(viewControllers[i].TabBarItem.Tag.ToString(), out originalIndex))
				{
					var page = (Page)((IPageController)Tabbed).InternalChildren[originalIndex];
					TabbedPage.SetIndex(page, i);
				}
			}
		}

		void UpdateCurrentPage()
		{
			var count = ((IPageController)Tabbed).InternalChildren.Count;
			((TabbedPage)Element).CurrentPage = SelectedIndex >= 0 && SelectedIndex < count ? Tabbed.GetPageByIndex((int)SelectedIndex) : null;
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				platformEffect.Container = View;
		}
	}
}