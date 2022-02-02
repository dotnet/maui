using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	internal class ShellToolbar : Toolbar
	{
		Shell _shell;
		BackButtonBehavior _backButtonBehavior;

		public ShellToolbar(Shell shell) : base(shell)
		{
			_shell = shell;
			shell.Navigated += (_, __) => ApplyChanges();
			shell.PropertyChanged += (_, p) =>
			{
				if(p.Is(Shell.BackButtonBehaviorProperty))
				{
					if(_backButtonBehavior != null)
						_backButtonBehavior.PropertyChanged -= OnBackButtonPropertyChanged;

					_backButtonBehavior = 
						_shell.GetEffectiveValue<BackButtonBehavior>(Shell.BackButtonBehaviorProperty, null);

					if (_backButtonBehavior != null)
						_backButtonBehavior.PropertyChanged += OnBackButtonPropertyChanged;
				}
				else if (p.IsOneOf(
					Shell.CurrentItemProperty,
					Shell.FlyoutBehaviorProperty,
					Shell.BackButtonBehaviorProperty))
				{
					ApplyChanges();
				}


			};

			shell.HandlerChanged += (_, __) => ApplyChanges();
			_backButtonBehavior =
				_shell.GetEffectiveValue<BackButtonBehavior>(Shell.BackButtonBehaviorProperty, null);

			if (_backButtonBehavior != null)
				_backButtonBehavior.PropertyChanged += OnBackButtonPropertyChanged;

			ApplyChanges();
		}

		void OnBackButtonPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			ApplyChanges();
		}

		void ApplyChanges()
		{
			if (_shell.CurrentPage == null)
				return;

			var stack = _shell.Navigation.NavigationStack;
			if (stack.Count == 0)
				return;

			//UpdateCurrentPage();
			var currentPage = _shell.CurrentPage;

			Page previousPage = null;
			if (stack.Count > 1)
				previousPage = stack[stack.Count - 1];

			//ToolbarItems = _toolbarTracker.ToolbarItems;
			IsVisible = _shell.GetEffectiveValue<bool>(Shell.NavBarIsVisibleProperty, true);
			_backButtonBehavior = _shell.GetEffectiveValue<BackButtonBehavior>(Shell.BackButtonBehaviorProperty, null);
			bool backButtonVisible = true;

			if (_backButtonBehavior != null)
			{
				backButtonVisible = _backButtonBehavior.IsVisible;
			}

			BackButtonVisible = backButtonVisible && stack.Count > 1;

			//if (navigationPage.IsSet(PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.BarHeightProperty))
			//	BarHeight = PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.GetBarHeight(navigationPage);
			//else
			//	BarHeight = null;

			//			if (previousPage != null)
			//				BackButtonTitle = NavigationPage.GetBackButtonTitle(previousPage);
			//			else
			//				BackButtonTitle = null;

			//			TitleIcon = NavigationPage.GetTitleIconImageSource(currentPage);

			//			BarBackground = navigationPage.BarBackground;
			//			if (!Brush.IsNullOrEmpty(navigationPage.BarBackground))
			//				BarBackgroundColor = null;
			//			else
			//				BarBackgroundColor = navigationPage.BarBackgroundColor;

			//#if WINDOWS
			//			if (Brush.IsNullOrEmpty(BarBackground) && BarBackgroundColor == null)
			//			{
			//				BarBackgroundColor = navigationPage.CurrentPage.BackgroundColor ??
			//					navigationPage.BackgroundColor;

			//				BarBackground = navigationPage.CurrentPage.Background ??
			//					navigationPage.Background;
			//			}
			//#endif
			//BarTextColor = GetBarTextColor();
			//IconColor = GetIconColor();
			Title = (!String.IsNullOrWhiteSpace(currentPage.Title)) ? currentPage.Title : _shell.Title;

			//TitleView = GetTitleView();

			if (currentPage != null)
				DynamicOverflowEnabled = PlatformConfiguration.WindowsSpecific.Page.GetToolbarDynamicOverflowEnabled(currentPage);
		}
	}
}
