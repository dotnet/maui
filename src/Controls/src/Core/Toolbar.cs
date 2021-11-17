using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	internal interface IToolbarElement
	{
		Toolbar Toolbar { get;}
	}

	public class Toolbar : Microsoft.Maui.IElement
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
		bool _backButtonVisible;
		bool _hasBackStack;
		bool _isVisible = false;
		IEnumerable<ToolbarItem> _toolbarItems;
		ToolbarTracker _toolbarTracker = new ToolbarTracker();
		bool _dynamicOverflowEnabled;

		public Toolbar()
		{
			_toolbarTracker.CollectionChanged += (_, __) => ToolbarItems = _toolbarTracker.ToolbarItems;
		}

		public IEnumerable<ToolbarItem> ToolbarItems { get => _toolbarItems; set => SetProperty(ref _toolbarItems, value); }
		public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
		public bool HasBackStack { get => _hasBackStack; set => SetProperty(ref _hasBackStack, value); }
		public bool BackButtonVisible { get => _backButtonVisible; set => SetProperty(ref _backButtonVisible, value); }
		public double? BarHeight { get => _barHeight; set => SetProperty(ref _barHeight, value); }
		public string BackButtonTitle { get => _backButtonTitle; set => SetProperty(ref _backButtonTitle, value); }
		public ImageSource TitleIcon { get => _titleIcon; set => SetProperty(ref _titleIcon, value); }
		public Color BarBackgroundColor { get => _barBackgroundColor; set => SetProperty(ref _barBackgroundColor, value); }
		public Brush BarBackground { get => _barBackground; set => SetProperty(ref _barBackground, value); }
		public Color BarTextColor { get => _barTextColor; set => SetProperty(ref _barTextColor, value); }
		public Color IconColor { get => _iconColor; set => SetProperty(ref _iconColor, value); }
		public string Title { get => _title; set => SetProperty(ref _title, value); }
		public VisualElement TitleView { get => _titleView; set => SetProperty(ref _titleView, value); }
		public bool DynamicOverflowEnabled { get => _dynamicOverflowEnabled; set => SetProperty(ref _dynamicOverflowEnabled, value); }

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

		internal void ApplyNavigationPage(NavigationPage navigationPage)
		{
			_parent = _parent ?? navigationPage.FindParentOfType<Window>();
			if (_currentNavigationPage == navigationPage)
				return;

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

			HasBackStack = _currentNavigationPage.Navigation.NavigationStack.Count > 1;
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
			IsVisible = NavigationPage.GetHasNavigationBar(currentPage);
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
			BarBackgroundColor = navigationPage.BarBackgroundColor;
			BarBackground = navigationPage.BarBackground;

#if WINDOWS
			if (Brush.IsNullOrEmpty(BarBackground) && BarBackgroundColor == null)
			{
				BarBackgroundColor = navigationPage.CurrentPage.BackgroundColor ??
					navigationPage.BackgroundColor;

				BarBackground = navigationPage.CurrentPage.Background ??
					navigationPage.Background;
			}
#endif
			BarTextColor = navigationPage.BarTextColor;
			IconColor = NavigationPage.GetIconColor(currentPage);
			Title = currentPage.Title;
			TitleView = NavigationPage.GetTitleView(navigationPage);
			DynamicOverflowEnabled = PlatformConfiguration.WindowsSpecific.Page.GetToolbarDynamicOverflowEnabled(_currentPage);
		}
	}
}
