#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Linq;
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
		UITabBarAppearance _tabBarAppearance;
		WeakReference<VisualElement> _element;

		IMauiContext MauiContext => _mauiContext;
		public static IPropertyMapper<TabbedPage, TabbedRenderer> Mapper = new PropertyMapper<TabbedPage, TabbedRenderer>(TabbedViewHandler.ViewMapper);
		public static CommandMapper<TabbedPage, TabbedRenderer> CommandMapper = new CommandMapper<TabbedPage, TabbedRenderer>(TabbedViewHandler.ViewCommandMapper);

		ViewHandlerDelegator<TabbedPage> _viewHandlerWrapper;
		Page Page => Element as Page;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public TabbedRenderer()
		{
			this.DisableiOS18ToolbarTabs();
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

		public VisualElement Element => _viewHandlerWrapper.Element ?? _element?.GetTargetOrDefault();

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
			_element = element is null ? null : new(element);

			FinishedCustomizingViewControllers += HandleFinishedCustomizingViewControllers;
			if (element is TabbedPage tabbed)
			{
				tabbed.PropertyChanged += OnPropertyChanged;
				tabbed.PagesChanged += OnPagesChanged;
			}

			OnPagesChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

			//disable edit/reorder of tabs
			CustomizableViewControllers = null;

			UpdateBarBackgroundColor();
			UpdateBarBackground();
			UpdateBarTextColor();
			UpdateSelectedTabColors();
			UpdateBarTranslucent();
			UpdatePageSpecifics();
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		[System.Runtime.Versioning.UnsupportedOSPlatform("ios8.0")]
		[System.Runtime.Versioning.UnsupportedOSPlatform("tvos")]
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);

			View.SetNeedsLayout();
		}

		public override void ViewDidAppear(bool animated)
		{
			Page?.SendAppearing();
			base.ViewDidAppear(animated);
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			Page?.SendDisappearing();
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
				_tabBarAppearance?.Dispose();
				_tabBarAppearance = null;

				Page?.SendDisappearing();

				if (Tabbed is TabbedPage tabbed)
				{
					tabbed.PropertyChanged -= OnPropertyChanged;
					tabbed.PagesChanged -= OnPagesChanged;
				}

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
			if (page?.Handler is not IPlatformViewHandler nvh)
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
			if (e.PropertyName == Page.IconImageSourceProperty.PropertyName || e.PropertyName == Page.TitleProperty.PropertyName)
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
			if (Tabbed?.CurrentPage is Page currentPage)
				controller = GetViewController(currentPage);
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
				var current = Tabbed?.CurrentPage;
				if (current == null)
					return;

				var controller = GetViewController(current);
				if (controller == null)
					return;

				SetNeedsUpdateOfHomeIndicatorAutoHidden();
				SetNeedsStatusBarAppearanceUpdate();
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
			else if (e.PropertyName == PrefersHomeIndicatorAutoHiddenProperty.PropertyName || e.PropertyName == PrefersStatusBarHiddenProperty.PropertyName)
				UpdatePageSpecifics();
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
			if (Page is Page page)
			{
				PageUIStatusBarAnimation animation = page.OnThisPlatform().PreferredStatusBarUpdateAnimation();
				Tabbed?.CurrentPage?.OnThisPlatform().SetPreferredStatusBarUpdateAnimation(animation);
			}
		}

		void UpdatePrefersStatusBarHiddenOnPages()
		{
			if (Tabbed is not TabbedPage tabbed)
				return;
			for (var i = 0; i < ViewControllers.Length; i++)
			{
				tabbed.GetPageByIndex(i).OnThisPlatform().SetPrefersStatusBarHidden(tabbed.OnThisPlatform().PrefersStatusBarHidden());
			}
		}

		public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden
		{
			get
			{
				var current = Tabbed?.CurrentPage;
				if (current == null)
					return null;

				return GetViewController(current);
			}
		}

		void UpdatePageSpecifics()
		{
			ChildViewControllerForHomeIndicatorAutoHidden?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
			ChildViewControllerForStatusBarHidden()?.SetNeedsStatusBarAppearanceUpdate();
		}

		void Reset()
		{
			if (Tabbed is not TabbedPage tabbed)
				return;
			var i = 0;
			foreach (var page in tabbed.Children)
				SetupPage(page, i++);
		}

		void SetControllers()
		{
			if (Tabbed is not TabbedPage tabbed)
				return;
			var list = new List<UIViewController>();
			var pages = tabbed.InternalChildren;
			for (var i = 0; i < pages.Count; i++)
			{
				var child = pages[i];
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
			if (Tabbed is not TabbedPage tabbed || TabBar == null)
				return;

			var barBackgroundColor = tabbed.BarBackgroundColor;
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

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
				UpdateiOS15TabBarAppearance();
			else
				TabBar.BarTintColor = isDefaultColor ? _defaultBarColor : barBackgroundColor.ToPlatform();
		}

		void UpdateBarBackground()
		{
			if (Tabbed is not TabbedPage tabbed || TabBar == null)
				return;

			var barBackground = tabbed.BarBackground;

			TabBar.UpdateBackground(barBackground);
		}

		void UpdateBarTextColor()
		{
			if (Tabbed is not TabbedPage tabbed || TabBar == null || TabBar.Items == null)
				return;

			var barTextColor = tabbed.BarTextColor;
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

			foreach (UITabBarItem item in TabBar.Items)
			{
				item.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = tabBarTextColor }, UIControlState.Normal);
				item.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = tabBarTextColor }, UIControlState.Selected);
				item.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = tabBarTextColor }, UIControlState.Disabled);
			}

			// set TintColor for selected icon
			// setting the unselected icon tint is not supported by iOS
			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
				UpdateiOS15TabBarAppearance();
			else
			{
				TabBar.TintColor = isDefaultColor ? _defaultBarTextColor : barTextColor.ToPlatform();
			}
		}

		void UpdateBarTranslucent()
		{
			if (Tabbed == null || TabBar == null || Element is not VisualElement element)
				return;

			_defaultBarTranslucent = _defaultBarTranslucent ?? TabBar.Translucent;
			switch (TabbedPageConfiguration.GetTranslucencyMode(element))
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
			if (Tabbed is not TabbedPage tabbed)
				return;
			for (var i = 0; i < viewControllers.Length; i++)
			{
				var originalIndex = -1;
				if (int.TryParse(viewControllers[i].TabBarItem.Tag.ToString(), out originalIndex))
				{
					var page = (Page)tabbed.InternalChildren[originalIndex];
					TabbedPage.SetIndex(page, i);
				}
			}
		}

		void UpdateCurrentPage()
		{
			if (Tabbed is TabbedPage tabbed)
			{
				var count = tabbed.InternalChildren.Count;
				var index = (int)SelectedIndex;
				tabbed.CurrentPage = index >= 0 && index < count ? tabbed.GetPageByIndex(index) : null;
			}
		}

		async void SetTabBarItem(IPlatformViewHandler renderer)
		{
			var page = renderer.VirtualView as Page;
			if (page == null)
				throw new InvalidCastException($"{nameof(renderer)} must be a {nameof(Page)} renderer.");

			var icons = await GetIcon(page);
			renderer.ViewController.TabBarItem = new UITabBarItem(page.Title, icons?.Item1, icons?.Item2)
			{
				Tag = Tabbed?.Children.IndexOf(page) ?? -1,
				AccessibilityIdentifier = page.AutomationId
			};
			icons?.Item1?.Dispose();
			icons?.Item2?.Dispose();
		}

		void UpdateSelectedTabColors()
		{
			if (Tabbed is not TabbedPage tabbed || TabBar == null || TabBar.Items == null)
				return;

			if (tabbed.IsSet(TabbedPage.SelectedTabColorProperty) && tabbed.SelectedTabColor != null)
			{
				TabBar.TintColor = tabbed.SelectedTabColor.ToPlatform();
			}
			else
			{
				TabBar.TintColor = UITabBar.Appearance.TintColor;
			}

			if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsTvOSVersionAtLeast(15))
				UpdateiOS15TabBarAppearance();
			else
			{
				if (tabbed.IsSet(TabbedPage.UnselectedTabColorProperty) && tabbed.UnselectedTabColor != null)
					TabBar.UnselectedItemTintColor = tabbed.UnselectedTabColor.ToPlatform();
				else
					TabBar.UnselectedItemTintColor = UITabBar.Appearance.TintColor;
			}
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

		[System.Runtime.Versioning.SupportedOSPlatform("ios15.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos15.0")]
		void UpdateiOS15TabBarAppearance()
		{
			if (Tabbed is not TabbedPage tabbed)
				return;
			TabBar.UpdateiOS15TabBarAppearance(
				ref _tabBarAppearance,
				_defaultBarColor,
				_defaultBarTextColor,
				tabbed.IsSet(TabbedPage.SelectedTabColorProperty) ? tabbed.SelectedTabColor : null,
				tabbed.IsSet(TabbedPage.UnselectedTabColorProperty) ? tabbed.UnselectedTabColor : null,
				tabbed.IsSet(TabbedPage.BarBackgroundColorProperty) ? tabbed.BarBackgroundColor : null,
				tabbed.IsSet(TabbedPage.BarTextColorProperty) ? tabbed.BarTextColor : null,
				null);
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

		void IViewHandler.PlatformArrange(Rect rect) =>
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
