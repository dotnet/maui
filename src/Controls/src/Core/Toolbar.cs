using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	internal class Toolbar : Element
	{
		ToolbarTracker _toolbarTracker = new ToolbarTracker();
		public IEnumerable<ToolbarItem> ToolbarItems { get; set; }
		public bool Visible { get; set; } = true;
		public bool HasBackStack { get; set; }
		public bool BackButtonVisible { get; set; }
		public double? BarHeight { get; set; }
		public string BackButtonTitle { get; set; }
		public ImageSource TitleIcon { get; set; }
		public Color BarBackgroundColor { get; set; }
		public Brush BarBackground { get; set; }
		public Color BarTextColor { get; set; }
		public Color IconColor { get; set; }
		public string Title { get; set; }
		public VisualElement TitleView { get; set; }
		
		NavigationPage _currentNavigationPage;

		Page _currentPage;

		internal void ApplyNavigationPage(NavigationPage navigationPage)
		{
			if (_currentNavigationPage == navigationPage)
				return;

			if (_currentNavigationPage != null)
				_currentNavigationPage.PropertyChanged -= OnPropertyChanged;

			_currentNavigationPage = navigationPage;
			_currentNavigationPage.PropertyChanged += OnPropertyChanged;
			UpdateCurrentPage();
			ApplyChanges(_currentNavigationPage);
		}

		private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
				NavigationPage.BarTextColorProperty))
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
			Visible = NavigationPage.GetHasNavigationBar(currentPage);
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
			BarTextColor = navigationPage.BarTextColor;
			IconColor = NavigationPage.GetIconColor(currentPage);
			Title = currentPage.Title;
			TitleView = NavigationPage.GetTitleView(navigationPage);


			// TODO TOOLBAR MAUI: This will trigger a full update
			Handler?.UpdateValue(nameof(Title));
		}
	}
}
