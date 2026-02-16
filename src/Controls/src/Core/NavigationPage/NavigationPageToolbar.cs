#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	internal class NavigationPageToolbar : Toolbar, IToolbar
	{
		ToolbarTracker _toolbarTracker = new ToolbarTracker();
		NavigationPage _currentNavigationPage;
		Page _currentPage;
		bool _hasAppeared;
		Color _barTextColor;
		Color _iconColor;
		string _title;
		VisualElement _titleView;
		bool _drawerToggleVisible;
		bool _backButtonVisible;
		bool _userChanged;
		internal Page RootPage { get; private set; }
		List<NavigationPage> _navigationPagesStack = new List<NavigationPage>();
		internal NavigationPage CurrentNavigationPage => _currentNavigationPage;
		internal ToolbarTracker ToolbarTracker => _toolbarTracker;
		public override Color BarTextColor { get => GetBarTextColor(); set => SetProperty(ref _barTextColor, value); }
		public override Color IconColor { get => GetIconColor(); set => SetProperty(ref _iconColor, value); }
		public override string Title { get => GetTitle(); set => SetProperty(ref _title, value); }
		public override VisualElement TitleView { get => GetTitleView(); set => SetProperty(ref _titleView, value); }
		public override bool DrawerToggleVisible { get => _drawerToggleVisible; set => SetProperty(ref _drawerToggleVisible, value); }

		public NavigationPageToolbar(Maui.IElement parent, Page rootPage) : base(parent)
		{
			_toolbarTracker.CollectionChanged += OnToolbarItemsChanged;
			RootPage = rootPage;
			_toolbarTracker.PageAppearing += OnPageAppearing;
			_toolbarTracker.Target = RootPage;
		}

		void OnToolbarItemsChanged(object sender, EventArgs e)
		{
			ToolbarItems = _toolbarTracker.ToolbarItems;
		}

		void OnPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(NavigationPage.HasNavigationBarProperty,
				NavigationPage.HasBackButtonProperty,
				NavigationPage.TitleIconImageSourceProperty,
				NavigationPage.TitleViewProperty,
				NavigationPage.IconColorProperty) ||
				e.IsOneOf(Page.TitleProperty,
				PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.BarHeightProperty,
				NavigationPage.BarBackgroundColorProperty,
				NavigationPage.BarBackgroundProperty,
				NavigationPage.BarTextColorProperty) ||
				e.IsOneOf(
					PlatformConfiguration.WindowsSpecific.Page.ToolbarDynamicOverflowEnabledProperty,
					PlatformConfiguration.WindowsSpecific.Page.ToolbarPlacementProperty))
			{
				ApplyChanges(_currentNavigationPage);
			}
			else if (_currentPage != sender && sender == _currentNavigationPage && e.Is(NavigationPage.CurrentPageProperty))
			{
				ApplyChanges(_currentNavigationPage);
			}
		}

		void OnPageAppearing(object sender, EventArgs e)
		{
			if (sender is not ContentPage cp)
				return;

			var pages = cp.GetParentPages();
			Page previous = null;
			foreach (var page in pages)
			{
				if (page is FlyoutPage fp)
				{
					if (fp.Flyout == cp || previous == fp.Flyout)
						return;
				}
				previous = page;
			}

			_toolbarTracker.PagePropertyChanged -= OnPagePropertyChanged;
			_currentPage = cp;
			_currentNavigationPage = _currentPage.FindParentOfType<NavigationPage>();

			foreach (var navPage in _navigationPagesStack)
			{
				navPage.ChildAdded -= NavigationPageChildrenChanged;
				navPage.ChildRemoved -= NavigationPageChildrenChanged;
			}

			_navigationPagesStack.Clear();
			if (_currentNavigationPage == null)
			{
				IsVisible = false;
				return;
			}

			_navigationPagesStack.Add(_currentNavigationPage);

			// we collect all the parent navigation pages because we need to know what
			// all the nav stacks look like for things like BackButton Visibility
			var parentNavigationPage = _currentNavigationPage.FindParentOfType<NavigationPage>();
			if (parentNavigationPage != null)
				_navigationPagesStack.Insert(0, parentNavigationPage);

			while (parentNavigationPage != null)
			{
				parentNavigationPage = parentNavigationPage.FindParentOfType<NavigationPage>();

				if (parentNavigationPage != null)
					_navigationPagesStack.Insert(0, parentNavigationPage);
			}

			foreach (var navPage in _navigationPagesStack)
			{
				navPage.ChildAdded += NavigationPageChildrenChanged;
				navPage.ChildRemoved += NavigationPageChildrenChanged;
			}

			_hasAppeared = true;

			ApplyChanges(_currentNavigationPage);
			_toolbarTracker.PagePropertyChanged += OnPagePropertyChanged;
		}

		// This is to catch scenarios where the user
		// inserts or removes the root page.
		// Which will cause the back button visibility to change.
		void NavigationPageChildrenChanged(object s, ElementEventArgs a)
		{
			ApplyChanges(_currentNavigationPage);
		}

		bool GetBackButtonVisible()
		{
			if (_currentPage == null)
				return false;

			return NavigationPage.GetHasBackButton(_currentPage) && GetBackButtonVisibleCalculated(false).Value;
		}

		public override bool BackButtonVisible
		{
			get => GetBackButtonVisible();
			set => _backButtonVisible = value;
		}

		bool? GetBackButtonVisibleCalculated(bool? defaultValue = null)
		{
			if (_currentPage == null || _currentNavigationPage == null)
				return defaultValue;

			foreach (var navPage in _navigationPagesStack)
			{
				if (navPage.Navigation.NavigationStack.Count == 0)
					return defaultValue;

				if (navPage.Navigation.NavigationStack.Count > 1)
				{
					return true;
				}
			}

			return false;
		}

		void UpdateBackButton()
		{
			if (_currentPage == null || _currentNavigationPage == null)
				return;

			var anyPagesPushed = GetBackButtonVisibleCalculated();

			if (anyPagesPushed == null)
				return;

			// Set this before BackButtonVisible triggers an update to the handler
			// This way all useful information is present
			// Show drawer toggle if it's a FlyoutPage, toolbar button should be shown, and either no pages are pushed or back button is not visible
			if (Parent is FlyoutPage flyout && flyout.ShouldShowToolbarButton()
#if !WINDOWS // TODO NET 10 : Move this logic to ShouldShowToolbarButton
				&& (!anyPagesPushed.Value || !BackButtonVisible)
#endif
				)
				_drawerToggleVisible = true;
			else
				_drawerToggleVisible = false;

			// Once we have better logic inside core to handle backbutton visiblity this
			// code should all go away.
			// Windows currently doesn't have logic in core to handle back button visibility
			// Android just handles it as part of core which means you get cool animations
			// that we don't want to interrupt here.
			// Once it's all built into core we can remove this code and simplify visibility logic
			if (_currentPage.IsSet(NavigationPage.HasBackButtonProperty))
			{
				SetProperty(ref _backButtonVisible, GetBackButtonVisible(), nameof(BackButtonVisible));
				_userChanged = true;
			}
			else
			{
				if (_userChanged)
				{
					SetProperty(ref _backButtonVisible, GetBackButtonVisible(), nameof(BackButtonVisible));
				}
				else
				{
#if ANDROID
					_backButtonVisible = GetBackButtonVisible();
#else
					SetProperty(ref _backButtonVisible, GetBackButtonVisible(), nameof(BackButtonVisible));
#endif
				}

				_userChanged = false;
			}
		}

		void ApplyChanges(NavigationPage navigationPage)
		{
			if (_currentPage == null)
				return;

			var stack = navigationPage.Navigation.NavigationStack;
			if (stack.Count == 0)
				return;

			var currentPage = _currentPage;

			Page previousPage = null;
			if (stack.Count > 1)
				previousPage = stack[stack.Count - 1];

			ToolbarItems = _toolbarTracker.ToolbarItems;
			IsVisible = NavigationPage.GetHasNavigationBar(currentPage) && _hasAppeared;

			UpdateBackButton();

			if (navigationPage.IsSet(PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.BarHeightProperty))
				BarHeight = PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.GetBarHeight(navigationPage);
			else
				BarHeight = null;

			if (previousPage != null)
				BackButtonTitle = NavigationPage.GetBackButtonTitle(previousPage);
			else
				BackButtonTitle = null;

			TitleIcon = NavigationPage.GetTitleIconImageSource(currentPage);

			BarBackground = navigationPage.BarBackground;
			if (Brush.IsNullOrEmpty(BarBackground) &&
				navigationPage.BarBackgroundColor != null)
			{
				BarBackground = new SolidColorBrush(navigationPage.BarBackgroundColor);
			}

#if WINDOWS
			if (Brush.IsNullOrEmpty(BarBackground))
			{
				var backgroundColor = navigationPage.CurrentPage.BackgroundColor ??
					navigationPage.BackgroundColor;

				BarBackground = navigationPage.CurrentPage.Background ??
					navigationPage.Background;

				if (Brush.IsNullOrEmpty(BarBackground) && backgroundColor != null)
				{
					BarBackground = new SolidColorBrush(backgroundColor);
				}
			}
#endif
			BarTextColor = GetBarTextColor();
			IconColor = GetIconColor();
			Title = GetTitle();
			TitleView = GetTitleView();
			DynamicOverflowEnabled = PlatformConfiguration.WindowsSpecific.Page.GetToolbarDynamicOverflowEnabled(_currentPage);
		}

		Color GetBarTextColor() => _currentNavigationPage?.BarTextColor;
		Color GetIconColor() => NavigationPage.GetIconColor(_currentPage) ?? NavigationPage.GetIconColor(_currentNavigationPage);

		string GetTitle()
		{
			if (GetTitleView() != null)
			{
				return string.Empty;
			}

			return _currentNavigationPage?.CurrentPage?.Title ?? _currentNavigationPage?.Title;
		}

		VisualElement GetTitleView()
		{
			if (_currentNavigationPage == null)
			{
				return null;
			}

			Page target = _currentNavigationPage;

			if (_currentNavigationPage.CurrentPage is Page currentPage)
			{
				target = currentPage;
			}

			return NavigationPage.GetTitleView(target);
		}

		internal void Disconnect()
		{
			if (_toolbarTracker is not null)
			{
				_toolbarTracker.PageAppearing -= OnPageAppearing;
				_toolbarTracker.PagePropertyChanged -= OnPagePropertyChanged;
				_toolbarTracker.CollectionChanged -= OnToolbarItemsChanged;
				_toolbarTracker.Target = null;
			}

			if (_navigationPagesStack is not null)
			{
				foreach (var navPage in _navigationPagesStack)
				{
					navPage.ChildAdded -= NavigationPageChildrenChanged;
					navPage.ChildRemoved -= NavigationPageChildrenChanged;
				}
			}
		}
	}
}
