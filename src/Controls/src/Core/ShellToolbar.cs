using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	internal class ShellToolbar : Toolbar
	{
		Shell _shell;
		Page _currentPage;
		BackButtonBehavior _backButtonBehavior;
		ToolbarTracker _toolbarTracker = new ToolbarTracker();
#if WINDOWS
		MenuBarTracker _menuBarTracker = new MenuBarTracker();
#endif
		bool _drawerToggleVisible;

		public override bool DrawerToggleVisible { get => _drawerToggleVisible; set => SetProperty(ref _drawerToggleVisible, value); }

		public ShellToolbar(Shell shell) : base(shell)
		{
			_drawerToggleVisible = true;
			BackButtonVisible = false;
			_shell = shell;
			shell.Navigated += (_, __) => ApplyChanges();
			shell.PropertyChanged += (_, p) =>
			{
				if (p.IsOneOf(
					Shell.CurrentItemProperty,
					Shell.FlyoutBehaviorProperty,
					Shell.BackButtonBehaviorProperty,
					Shell.NavBarIsVisibleProperty,
					Shell.TitleViewProperty))
				{
					ApplyChanges();
				}
				else if (p.Is(Shell.TitleProperty))
					UpdateTitle();
			};

			shell.HandlerChanged += (_, __) => ApplyChanges();

			ApplyChanges();
			_toolbarTracker.CollectionChanged += (_, __) => ToolbarItems = _toolbarTracker.ToolbarItems;
#if WINDOWS
			_menuBarTracker.CollectionChanged += (_, __) => ApplyChanges();
#endif
		}

		internal void ApplyChanges()
		{
			var currentPage = _shell.CurrentPage;

			if (_currentPage != _shell.CurrentPage)
			{
				if (_currentPage != null)
					_currentPage.PropertyChanged -= OnCurrentPagePropertyChanged;

				_currentPage = currentPage;

				if (_currentPage != null)
					_currentPage.PropertyChanged += OnCurrentPagePropertyChanged;
			}

			if (currentPage == null)
				return;

			var stack = _shell.Navigation.NavigationStack;
			if (stack.Count == 0)
				return;

			_toolbarTracker.Target = _shell;
#if WINDOWS
			_menuBarTracker.Target = _shell;
#endif

			Page previousPage = null;
			if (stack.Count > 1)
				previousPage = stack[stack.Count - 1];

			ToolbarItems = _toolbarTracker.ToolbarItems;

			UpdateBackbuttonBehavior();
			bool backButtonVisible = true;

			if (_backButtonBehavior != null)
			{
				backButtonVisible = _backButtonBehavior.IsVisible;
			}

			_drawerToggleVisible = stack.Count <= 1;
			BackButtonVisible = backButtonVisible && stack.Count > 1;
			BackButtonEnabled = _backButtonBehavior?.IsEnabled ?? true;

			if (_backButtonBehavior?.Command != null)
			{
				BackButtonEnabled =
					BackButtonEnabled &&
					_backButtonBehavior.Command.CanExecute(_backButtonBehavior.CommandParameter);
			}

			UpdateTitle();

			if (_currentPage != null &&
				_currentPage.IsSet(Shell.NavBarIsVisibleProperty))
			{
				IsVisible = Shell.GetNavBarIsVisible(_currentPage);
			}
			else
			{
				var flyoutBehavior = (_shell as IFlyoutView).FlyoutBehavior;
#if WINDOWS
				IsVisible = (!String.IsNullOrEmpty(Title) ||
					TitleView != null ||
					_toolbarTracker.ToolbarItems.Count > 0 ||
					_menuBarTracker.ToolbarItems.Count > 0 ||
					flyoutBehavior == FlyoutBehavior.Flyout);
#else
				IsVisible = (BackButtonVisible ||
					!String.IsNullOrEmpty(Title) ||
					TitleView != null ||
					_toolbarTracker.ToolbarItems.Count > 0 ||
					flyoutBehavior == FlyoutBehavior.Flyout);
#endif
			}

			if (currentPage != null)
				DynamicOverflowEnabled = PlatformConfiguration.WindowsSpecific.Page.GetToolbarDynamicOverflowEnabled(currentPage);
		}

		ICommand _backButtonCommand;
		void UpdateBackbuttonBehavior()
		{
			var bbb = Shell.GetBackButtonBehavior(_currentPage);

			if (bbb == _backButtonBehavior)
				return;

			if (_backButtonBehavior != null)
				_backButtonBehavior.PropertyChanged -= OnBackButtonPropertyChanged;

			_backButtonBehavior = bbb;

			if (_backButtonBehavior != null)
				_backButtonBehavior.PropertyChanged += OnBackButtonPropertyChanged;

			void OnBackButtonPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				ApplyChanges();

				if (_backButtonBehavior.Command == _backButtonCommand)
					return;

				if (_backButtonCommand != null)
					_backButtonCommand.CanExecuteChanged -= OnBackButtonCanExecuteChanged;

				_backButtonCommand = _backButtonBehavior.Command;

				if (_backButtonCommand != null)
					_backButtonCommand.CanExecuteChanged += OnBackButtonCanExecuteChanged;
			}

			void OnBackButtonCanExecuteChanged(object sender, EventArgs e)
			{
				BackButtonEnabled =
					_backButtonCommand.CanExecute(_backButtonBehavior.CommandParameter);
			}
		}

		void OnCurrentPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.Is(Page.TitleProperty))
				UpdateTitle();
			else if (e.IsOneOf(
				Shell.BackButtonBehaviorProperty,
				Shell.NavBarIsVisibleProperty,
				Shell.TitleViewProperty))
			{
				ApplyChanges();
			}
		}

		internal void UpdateTitle()
		{
			TitleView = _shell.GetEffectiveValue<VisualElement>(
				Shell.TitleViewProperty,
				Shell.GetTitleView(_shell));

			if (TitleView != null)
			{
				Title = String.Empty;
				return;
			}

			Page currentPage = _shell.GetCurrentShellPage() as Page;
			if (currentPage?.IsSet(Page.TitleProperty) == true)
			{
				Title = currentPage.Title ?? String.Empty;
			}
			// We only want to use the ShellContent as a title if no pages have been
			// Pushed onto the stack
			else if (_shell.Navigation?.NavigationStack?.Count <= 1)
			{
				Title = _shell.CurrentContent?.Title ?? String.Empty;
			}
			else
			{
				Title = String.Empty;
			}
		}
	}
}
