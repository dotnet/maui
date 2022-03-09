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
		Page _currentPage;
		BackButtonBehavior _backButtonBehavior;
		ToolbarTracker _toolbarTracker = new ToolbarTracker();

		public ShellToolbar(Shell shell) : base(shell)
		{
			_shell = shell;
			shell.Navigated += (_, __) => ApplyChanges();
			shell.PropertyChanged += (_, p) =>
			{
				if (p.Is(Shell.BackButtonBehaviorProperty))
				{
					if (_backButtonBehavior != null)
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
				else if (p.Is(Shell.TitleProperty))
					UpdateTitle();
			};

			shell.HandlerChanged += (_, __) => ApplyChanges();
			_backButtonBehavior =
				_shell.GetEffectiveValue<BackButtonBehavior>(Shell.BackButtonBehaviorProperty, null);

			if (_backButtonBehavior != null)
				_backButtonBehavior.PropertyChanged += OnBackButtonPropertyChanged;

			ApplyChanges();
			_toolbarTracker.CollectionChanged += (_, __) => ToolbarItems = _toolbarTracker.ToolbarItems;
			_toolbarTracker.AdditionalTargets = new List<Page> { shell };
		}

		void OnBackButtonPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			ApplyChanges();
		}

		void ApplyChanges()
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

			_toolbarTracker.Target = currentPage;

			Page previousPage = null;
			if (stack.Count > 1)
				previousPage = stack[stack.Count - 1];

			ToolbarItems = _toolbarTracker.ToolbarItems;
			IsVisible = _shell.GetEffectiveValue<bool>(Shell.NavBarIsVisibleProperty, true);
			_backButtonBehavior = _shell.GetEffectiveValue<BackButtonBehavior>(Shell.BackButtonBehaviorProperty, null);
			bool backButtonVisible = true;

			if (_backButtonBehavior != null)
			{
				backButtonVisible = _backButtonBehavior.IsVisible;
			}

			BackButtonVisible = backButtonVisible && stack.Count > 1;

			UpdateTitle();

			TitleView = _shell.GetEffectiveValue<VisualElement>(Shell.TitleViewProperty, null);

			if (currentPage != null)
				DynamicOverflowEnabled = PlatformConfiguration.WindowsSpecific.Page.GetToolbarDynamicOverflowEnabled(currentPage);
		}

		void OnCurrentPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.Is(Page.TitleProperty))
				UpdateTitle();
		}

		internal void UpdateTitle()
		{
			Page currentPage = _shell.GetCurrentShellPage() as Page;

			if (currentPage?.IsSet(Page.TitleProperty) == true)
				Title = currentPage.Title ?? String.Empty;
			else
				Title = _shell.GetEffectiveValue<string>(
					BaseShellItem.TitleProperty,
					_shell.Title,
					ignoreImplicit: true) ?? String.Empty;
		}
	}
}
