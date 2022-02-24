using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using UIKit;
using static Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using PageUIStatusBarAnimation = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;
using TabbedPageConfiguration = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.TabbedPage;
using TranslucencyMode = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.TranslucencyMode;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class TabbedRenderer : UITabBarController, IPlatformViewHandler
	{
		bool _barBackgroundColorWasSet;
		bool _barTextColorWasSet;
		UIColor _defaultBarTextColor;
		bool _defaultBarTextColorSet;
		UIColor _defaultBarColor;
		bool _defaultBarColorSet;
		bool? _defaultBarTranslucent;
		IMauiContext _mauiContext;
		IMauiContext MauiContext => _mauiContext;
		public static IPropertyMapper<TabbedPage, TabbedRenderer> Mapper = new PropertyMapper<TabbedPage, TabbedRenderer>(TabbedViewHandler.ViewMapper);
		public static CommandMapper<TabbedPage, TabbedRenderer> CommandMapper = new CommandMapper<TabbedPage, TabbedRenderer>(TabbedViewHandler.ViewCommandMapper);

		ViewHandlerDelegator<TabbedPage> _viewHandlerWrapper;
		Page Page => Element as Page;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public TabbedRenderer()
		{
			_viewHandlerWrapper = new ViewHandlerDelegator<TabbedPage>(Mapper, CommandMapper, this);
		}

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

		public VisualElement Element => _viewHandlerWrapper.Element;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		public UIView NativeView
		{
			get { return View; }
		}

		public void SetElement(VisualElement element)
		{
			_viewHandlerWrapper.SetVirtualView(element, OnElementChanged, false);

			FinishedCustomizingViewControllers += HandleFinishedCustomizingViewControllers;
			Tabbed.PropertyChanged += OnPropertyChanged;
			Tabbed.PagesChanged += OnPagesChanged;

			OnPagesChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			//disable edit/reorder of tabs
			CustomizableViewControllers = null;

			UpdateBarBackgroundColor();
			UpdateBarBackground();
			UpdateBarTextColor();
			UpdateSelectedTabColors();
			UpdateBarTranslucent();
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
			Page.SendAppearing();
			base.ViewDidAppear(animated);
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			Page.SendDisappearing();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (Element is IView view)
				view.Arrange(View.Bounds.ToRectangle());
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Page.SendDisappearing();
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
			if (page.Handler is not IPlatformViewHandler nvh)
				return null;

			return nvh.ViewController;
		}

		void HandleFinishedCustomizingViewControllers(object sender, UITabBarCustomizeChangeEventArgs e)
		{
			if (e.Changed)
				UpdateChildrenOrderIndex(e.ViewControllers);
		}

		void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// Setting TabBarItem.Title in iOS 10 causes rendering bugs
			// Work around this by creating a new UITabBarItem on each change
			if (e.PropertyName == Page.TitleProperty.PropertyName && !PlatformVersion.IsAtLeast(10))
			{
				var page = (Page)sender;
				var renderer = page.ToHandler(_mauiContext);
				if (renderer == null)
					return;

				if (renderer.ViewController.TabBarItem != null)
					renderer.ViewController.TabBarItem.Title = page.Title;
			}
			else if (e.PropertyName == Page.IconImageSourceProperty.PropertyName || e.PropertyName == Page.TitleProperty.PropertyName && PlatformVersion.IsAtLeast(10))
			{
				var page = (Page)sender;

				IPlatformViewHandler renderer = page.ToHandler(_mauiContext);

				if (renderer?.ViewController.TabBarItem == null)
					return;

				SetTabBarItem(renderer);
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

			UpdateBarBackgroundColor();
			UpdateBarTextColor();
			UpdateSelectedTabColors();
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
			else if (e.PropertyName == TabbedPage.BarBackgroundProperty.PropertyName)
				UpdateBarBackground();
			else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
				UpdateBarTextColor();
			else if (e.PropertyName == PrefersStatusBarHiddenProperty.PropertyName)
				UpdatePrefersStatusBarHiddenOnPages();
			else if (e.PropertyName == PreferredStatusBarUpdateAnimationProperty.PropertyName)
				UpdateCurrentPagePreferredStatusBarUpdateAnimation();
			else if (e.PropertyName == TabbedPage.SelectedTabColorProperty.PropertyName || e.PropertyName == TabbedPage.UnselectedTabColorProperty.PropertyName)
				UpdateSelectedTabColors();
			else if (e.PropertyName == PrefersHomeIndicatorAutoHiddenProperty.PropertyName)
				UpdatePrefersHomeIndicatorAutoHiddenOnPages();
			else if (e.PropertyName == TabbedPageConfiguration.TranslucencyModeProperty.PropertyName)
				UpdateBarTranslucent();

		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			var current = Tabbed?.CurrentPage;
			if (current == null)
				return null;

			return GetViewController(current);
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

		public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden
		{
			get
			{
				var current = Tabbed.CurrentPage;
				if (current == null)
					return null;

				return GetViewController(current);
			}
		}

		void UpdatePrefersHomeIndicatorAutoHiddenOnPages()
		{
			bool isHomeIndicatorHidden = Tabbed.OnThisPlatform().PrefersHomeIndicatorAutoHidden();
			for (var i = 0; i < ViewControllers.Length; i++)
			{
				Tabbed.GetPageByIndex(i).OnThisPlatform().SetPrefersHomeIndicatorAutoHidden(isHomeIndicatorHidden);
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
			var logicalChildren = ((IElementController)Element).LogicalChildren;
			for (var i = 0; i < logicalChildren.Count; i++)
			{
				var child = logicalChildren[i];
				var v = child as Page;
				if (v == null)
					continue;
				if (GetViewController(v) != null)
					list.Add(GetViewController(v));
			}
			ViewControllers = list.ToArray();
		}

		void SetupPage(Page page, int index)
		{
			var renderer = (IPlatformViewHandler)page.ToHandler(_mauiContext);

			page.PropertyChanged += OnPagePropertyChanged;

			SetTabBarItem(renderer);
		}

		void TeardownPage(Page page, int index)
		{
			page.PropertyChanged -= OnPagePropertyChanged;

			page.Handler?.DisconnectHandler();
		}

		void UpdateBarBackgroundColor()
		{
			if (Tabbed == null || TabBar == null)
				return;

			var barBackgroundColor = Tabbed.BarBackgroundColor;
			var isDefaultColor = barBackgroundColor == null;

			if (isDefaultColor && !_barBackgroundColorWasSet)
				return;

			if (!_defaultBarColorSet)
			{
				_defaultBarColor = TabBar.BarTintColor;

				_defaultBarColorSet = true;
			}

			if (!isDefaultColor)
				_barBackgroundColorWasSet = true;

			TabBar.BarTintColor = isDefaultColor ? _defaultBarColor : barBackgroundColor.ToPlatform();
		}

		void UpdateBarBackground()
		{
			if (Tabbed == null || TabBar == null)
				return;

			var barBackground = Tabbed.BarBackground;

			TabBar.UpdateBackground(barBackground);
		}

		void UpdateBarTextColor()
		{
			if (Tabbed == null || TabBar == null || TabBar.Items == null)
				return;

			var barTextColor = Tabbed.BarTextColor;
			var isDefaultColor = barTextColor == null;

			if (isDefaultColor && !_barTextColorWasSet)
				return;

			if (!_defaultBarTextColorSet)
			{
				_defaultBarTextColor = TabBar.TintColor;
				_defaultBarTextColorSet = true;
			}

			if (!isDefaultColor)
				_barTextColorWasSet = true;

			UIColor tabBarTextColor;
			if (isDefaultColor)
				tabBarTextColor = _defaultBarTextColor;
			else
				tabBarTextColor = barTextColor.ToPlatform();

			var attributes = new UIStringAttributes();
			attributes.ForegroundColor = tabBarTextColor;

			foreach (UITabBarItem item in TabBar.Items)
			{
				item.SetTitleTextAttributes(attributes, UIControlState.Normal);
			}

			// set TintColor for selected icon
			// setting the unselected icon tint is not supported by iOS
			TabBar.TintColor = isDefaultColor ? _defaultBarTextColor : barTextColor.ToPlatform();
		}

		void UpdateBarTranslucent()
		{
			if (Tabbed == null || TabBar == null || Element == null)
				return;

			_defaultBarTranslucent = _defaultBarTranslucent ?? TabBar.Translucent;
			switch (TabbedPageConfiguration.GetTranslucencyMode(Element))
			{
				case TranslucencyMode.Translucent:
					TabBar.Translucent = true;
					return;
				case TranslucencyMode.Opaque:
					TabBar.Translucent = false;
					return;
				default:
					TabBar.Translucent = _defaultBarTranslucent.GetValueOrDefault();
					return;
			}
		}

		void UpdateChildrenOrderIndex(UIViewController[] viewControllers)
		{
			for (var i = 0; i < viewControllers.Length; i++)
			{
				var originalIndex = -1;
				if (int.TryParse(viewControllers[i].TabBarItem.Tag.ToString(), out originalIndex))
				{
					var page = (Page)Tabbed.InternalChildren[originalIndex];
					TabbedPage.SetIndex(page, i);
				}
			}
		}

		void UpdateCurrentPage()
		{
			var count = Tabbed.InternalChildren.Count;
			var index = (int)SelectedIndex;
			((TabbedPage)Element).CurrentPage = index >= 0 && index < count ? Tabbed.GetPageByIndex(index) : null;
		}

		async void SetTabBarItem(IPlatformViewHandler renderer)
		{
			var page = renderer.VirtualView as Page;
			if (page == null)
				throw new InvalidCastException($"{nameof(renderer)} must be a {nameof(Page)} renderer.");

			var icons = await GetIcon(page);
			renderer.ViewController.TabBarItem = new UITabBarItem(page.Title, icons?.Item1, icons?.Item2)
			{
				Tag = Tabbed.Children.IndexOf(page),
				AccessibilityIdentifier = page.AutomationId
			};
			icons?.Item1?.Dispose();
			icons?.Item2?.Dispose();
		}

		void UpdateSelectedTabColors()
		{
			if (Tabbed == null || TabBar == null || TabBar.Items == null)
				return;

			if (Tabbed.IsSet(TabbedPage.SelectedTabColorProperty) && Tabbed.SelectedTabColor != null)
			{
				if (PlatformVersion.IsAtLeast(10))
					TabBar.TintColor = Tabbed.SelectedTabColor.ToPlatform();
				else
					TabBar.SelectedImageTintColor = Tabbed.SelectedTabColor.ToPlatform();

			}
			else
			{
				if (PlatformVersion.IsAtLeast(10))
					TabBar.TintColor = UITabBar.Appearance.TintColor;
				else
					TabBar.SelectedImageTintColor = UITabBar.Appearance.SelectedImageTintColor;
			}

			if (!PlatformVersion.IsAtLeast(10))
				return;

			if (Tabbed.IsSet(TabbedPage.UnselectedTabColorProperty) && Tabbed.UnselectedTabColor != null)
				TabBar.UnselectedItemTintColor = Tabbed.UnselectedTabColor.ToPlatform();
			else
				TabBar.UnselectedItemTintColor = UITabBar.Appearance.TintColor;
		}

		/// <summary>
		/// Get the icon for the tab bar item of this page
		/// </summary>
		/// <returns>
		/// A tuple containing as item1: the unselected version of the icon, item2: the selected version of the icon (item2 can be null),
		/// or null if no icon should be set.
		/// </returns>
		protected virtual Task<Tuple<UIImage, UIImage>> GetIcon(Page page)
		{
			TaskCompletionSource<Tuple<UIImage, UIImage>> source =
				new TaskCompletionSource<Tuple<UIImage, UIImage>>();

			page.IconImageSource.LoadImage(MauiContext, result =>
			{
				if (result?.Value == null)
					source.SetResult(null);
				else
					source.SetResult(Tuple.Create(result.Value, (UIImage)null));
			});

			return source.Task;
		}

		#region IPlatformViewHandler
		bool IViewHandler.HasContainer { get => false; set { } }

		object IViewHandler.ContainerView => null;

		IView IViewHandler.VirtualView => Element;

		object IElementHandler.PlatformView => NativeView;

		Maui.IElement IElementHandler.VirtualView => Element;

		IMauiContext IElementHandler.MauiContext => _mauiContext;

		UIView IPlatformViewHandler.PlatformView => NativeView;

		UIView IPlatformViewHandler.ContainerView => null;

		UIViewController IPlatformViewHandler.ViewController => this;

		void IViewHandler.PlatformArrange(Rectangle rect) =>
			_viewHandlerWrapper.PlatformArrange(rect);

		void IElementHandler.SetMauiContext(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		void IElementHandler.SetVirtualView(Maui.IElement view)
		{
			SetElement((VisualElement)view);
		}

		void IElementHandler.UpdateValue(string property)
		{
			_viewHandlerWrapper.UpdateProperty(property);
		}

		void IElementHandler.Invoke(string command, object args)
		{
			_viewHandlerWrapper.Invoke(command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			_viewHandlerWrapper.DisconnectHandler();
		}
		#endregion
	}
}
