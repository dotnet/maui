using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
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

		public override Color BarTextColor { get => GetBarTextColor(); set => SetProperty(ref _barTextColor, value); }
		public override Color IconColor { get => GetIconColor(); set => SetProperty(ref _iconColor, value); }
		public override string Title { get => GetTitle(); set => SetProperty(ref _title, value); }
		public override VisualElement TitleView { get => GetTitleView(); set => SetProperty(ref _titleView, value); }
		public override bool DrawerToggleVisible { get => _drawerToggleVisible; set => SetProperty(ref _drawerToggleVisible, value); }

		public NavigationPageToolbar(Maui.IElement parent) : base(parent)
		{
			_toolbarTracker.CollectionChanged += (_, __) => ToolbarItems = _toolbarTracker.ToolbarItems;
		}

		internal void ApplyNavigationPage(NavigationPage navigationPage, bool hasAppeared)
		{
			_hasAppeared = hasAppeared;
			if (_currentNavigationPage == navigationPage)
			{
				IsVisible = hasAppeared;
				return;
			}

			if (_currentNavigationPage != null)
				_currentNavigationPage.PropertyChanged -= OnPropertyChanged;

			_currentNavigationPage = navigationPage;
			_currentNavigationPage.PropertyChanged += OnPropertyChanged;
			UpdateCurrentPage();
			ApplyChanges(_currentNavigationPage);
		}



		void UpdateCurrentPage()
		{
			if (_currentNavigationPage == null)
				return;

			var stack = _currentNavigationPage.Navigation.NavigationStack;
			if (stack.Count == 0)
				return;

			if (_currentPage == _currentNavigationPage.CurrentPage)
				return;

			if (_currentPage != null)
				_currentPage.PropertyChanged -= OnPropertyChanged;

			_currentPage = _currentNavigationPage.CurrentPage;
			_currentNavigationPage.CurrentPage.PropertyChanged += OnPropertyChanged;
		}

		void ApplyChanges(NavigationPage navigationPage)
		{
			var stack = navigationPage.Navigation.NavigationStack;
			if (stack.Count == 0)
				return;

			UpdateCurrentPage();
			var currentPage = _currentPage;

			Page previousPage = null;
			if (stack.Count > 1)
				previousPage = stack[stack.Count - 1];

			_toolbarTracker.Target = navigationPage.CurrentPage;
			_toolbarTracker.AdditionalTargets = navigationPage.GetParentPages();
			ToolbarItems = _toolbarTracker.ToolbarItems;
			IsVisible = NavigationPage.GetHasNavigationBar(currentPage) && _hasAppeared;

			// Set this before BackButtonVisible triggers an update to the handler
			// This way all useful information is present
			if (Parent is FlyoutPage && stack.Count == 1)
				_drawerToggleVisible = true;
			else
				_drawerToggleVisible = false;

			BackButtonVisible = NavigationPage.GetHasBackButton(currentPage) && stack.Count > 1;

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
			if (!Brush.IsNullOrEmpty(navigationPage.BarBackground))
				BarBackgroundColor = null;
			else
				BarBackgroundColor = navigationPage.BarBackgroundColor;

#if WINDOWS
			if (Brush.IsNullOrEmpty(BarBackground) && BarBackgroundColor == null)
			{
				BarBackgroundColor = navigationPage.CurrentPage.BackgroundColor ??
					navigationPage.BackgroundColor;

				BarBackground = navigationPage.CurrentPage.Background ??
					navigationPage.Background;
			}
#endif
			BarTextColor = GetBarTextColor();
			IconColor = GetIconColor();
			Title = GetTitle();
			TitleView = GetTitleView();
			DynamicOverflowEnabled = PlatformConfiguration.WindowsSpecific.Page.GetToolbarDynamicOverflowEnabled(_currentPage);
		}

		void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender == _currentNavigationPage && e.Is(NavigationPage.CurrentPageProperty))
			{
				UpdateCurrentPage();
				ApplyChanges(_currentNavigationPage);
			}
			else if (e.IsOneOf(NavigationPage.HasNavigationBarProperty,
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
		}

		Color GetBarTextColor() => _currentNavigationPage?.BarTextColor;
		Color GetIconColor() => (_currentPage != null) ? NavigationPage.GetIconColor(_currentPage) : null;
		string GetTitle() => _currentPage?.Title;
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
	}
}
