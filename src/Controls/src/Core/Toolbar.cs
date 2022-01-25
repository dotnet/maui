using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar : Maui.IToolbar
	{
		NavigationPage _currentNavigationPage;
		Page _currentPage;
		VisualElement _titleView;
		string _title;
		Color _iconColor;
		Color _barTextColor;
		Brush _barBackground;
		Color _barBackgroundColor;
		ImageSource _titleIcon;
		string _backButtonTitle;
		double? _barHeight;
		IEnumerable<ToolbarItem> _toolbarItems;
		ToolbarTracker _toolbarTracker = new ToolbarTracker();
		bool _dynamicOverflowEnabled;
		bool _hasAppeared;
		bool _isVisible = false;
		bool _backButtonVisible;
		bool _drawerToggleVisible;

		public Toolbar(Maui.IElement parent)
		{
			_parent = parent;
			_toolbarTracker.CollectionChanged += (_, __) => ToolbarItems = _toolbarTracker.ToolbarItems;
		}

		public IEnumerable<ToolbarItem> ToolbarItems { get => _toolbarItems; set => SetProperty(ref _toolbarItems, value); }
		public double? BarHeight { get => _barHeight; set => SetProperty(ref _barHeight, value); }
		public string BackButtonTitle { get => _backButtonTitle; set => SetProperty(ref _backButtonTitle, value); }
		public ImageSource TitleIcon { get => _titleIcon; set => SetProperty(ref _titleIcon, value); }
		public Color BarBackgroundColor { get => _barBackgroundColor; set => SetProperty(ref _barBackgroundColor, value); }
		public Brush BarBackground { get => _barBackground; set => SetProperty(ref _barBackground, value); }
		public Color BarTextColor { get => GetBarTextColor(); set => SetProperty(ref _barTextColor, value); }
		public Color IconColor { get => GetIconColor(); set => SetProperty(ref _iconColor, value); }
		public string Title { get => GetTitle(); set => SetProperty(ref _title, value); }
		public VisualElement TitleView { get => GetTitleView(); set => SetProperty(ref _titleView, value); }
		public bool DynamicOverflowEnabled { get => _dynamicOverflowEnabled; set => SetProperty(ref _dynamicOverflowEnabled, value); }
		public bool BackButtonVisible { get => _backButtonVisible; set => SetProperty(ref _backButtonVisible, value); }
		public bool DrawerToggleVisible { get => _drawerToggleVisible; set => SetProperty(ref _drawerToggleVisible, value); }
		public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
		public IElementHandler Handler { get; set; }

		Maui.IElement _parent;
		Maui.IElement Maui.IElement.Parent => _parent;

		void SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName] string propertyName = "")
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return;

			backingStore = value;
			Handler?.UpdateValue(propertyName);
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
			if (_parent is FlyoutPage && stack.Count == 1)
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

		Color GetBarTextColor() => _currentNavigationPage?.BarTextColor;
		Color GetIconColor() => (_currentPage != null) ? NavigationPage.GetIconColor(_currentPage) : null;
		string GetTitle() => _currentPage?.Title;
		VisualElement GetTitleView() => (_currentNavigationPage != null) ? NavigationPage.GetTitleView(_currentNavigationPage) : null;
	}
}
