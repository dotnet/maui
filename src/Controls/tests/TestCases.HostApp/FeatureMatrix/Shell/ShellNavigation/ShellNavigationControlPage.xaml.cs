using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class ShellNavigationControlPage : Shell
	{
		readonly ShellViewModel _viewModel;

		public ShellNavigationControlPage()
		{
			_viewModel = new ShellViewModel();
			BindingContext = _viewModel;
			InitializeComponent();

			Routing.RegisterRoute("detail1", typeof(DetailPage1));
			Routing.RegisterRoute("detail2", typeof(DetailPage2));

			this.Navigating += OnShellNavigating;
			this.Navigated += OnShellNavigated;

			MainContentPage.Appearing += (s, e) => UpdateCurrentState();
			Page2ContentPage.Appearing += (s, e) => UpdatePage2State();
			Page3ContentPage.Appearing += (s, e) => UpdatePage3State();
		}

		public ShellViewModel ViewModel => _viewModel;

		static string CleanRoute(string route)
		{
			return route.TrimStart('/').Replace("/", " > ", StringComparison.Ordinal);
		}

		void OnShellNavigating(object sender, ShellNavigatingEventArgs e)
		{
			string from = CleanRoute(e.Current?.Location?.ToString() ?? "none");
			string to = CleanRoute(e.Target?.Location?.ToString() ?? "none");
			_viewModel.Navigating = $"{from} → {to}";
		}

		void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
		{
			string from = CleanRoute(e.Previous?.Location?.ToString() ?? "none");
			string to = CleanRoute(e.Current?.Location?.ToString() ?? "none");
			_viewModel.Navigated = $"{from} → {to}";
			UpdateCurrentState();
		}

		void UpdateCurrentState()
		{
			var shell = Shell.Current;
			if (shell != null)
			{
				_viewModel.CurrentState = CleanRoute(shell.CurrentState?.Location?.ToString() ?? "Not Set");
				_viewModel.CurrentPage = shell.CurrentPage?.Title ?? "Not Set";
				_viewModel.CurrentItem = shell.CurrentItem?.Title ?? "Not Set";
				_viewModel.ShellCurrent = shell.GetType().Name;
			}
		}

		void UpdatePage2State()
		{
			var shell = Shell.Current;
			if (shell != null)
			{
				Page2CurrentStateLabel.Text = CleanRoute(shell.CurrentState?.Location?.ToString() ?? "Not Set");
				Page2CurrentPageLabel.Text = shell.CurrentPage?.Title ?? "Not Set";
				Page2CurrentItemLabel.Text = shell.CurrentItem?.Title ?? "Not Set";
				Page2ShellCurrentLabel.Text = shell.GetType().Name;
				Page2NavigatingLabel.Text = _viewModel.Navigating;
				Page2NavigatedLabel.Text = _viewModel.Navigated;
			}
		}

		void UpdatePage3State()
		{
			var shell = Shell.Current;
			if (shell != null)
			{
				Page3CurrentStateLabel.Text = CleanRoute(shell.CurrentState?.Location?.ToString() ?? "Not Set");
				Page3CurrentPageLabel.Text = shell.CurrentPage?.Title ?? "Not Set";
				Page3CurrentItemLabel.Text = shell.CurrentItem?.Title ?? "Not Set";
				Page3ShellCurrentLabel.Text = shell.GetType().Name;
				Page3NavigatingLabel.Text = _viewModel.Navigating;
				Page3NavigatedLabel.Text = _viewModel.Navigated;
			}
		}

		public void OnIconOverrideClicked(object sender, EventArgs e)
		{
			if (sender is Button btn)
			{
				if (btn.Text == "None")
					_viewModel.IconOverride = string.Empty;
				else
					_viewModel.IconOverride = btn.Text;
			}
		}

		void OnToggleIsEnabled(object sender, EventArgs e)
		{
			_viewModel.IsEnabled = !_viewModel.IsEnabled;
		}

		void OnToggleIsVisible(object sender, EventArgs e)
		{
			_viewModel.IsVisible = !_viewModel.IsVisible;
		}

		async void OnNavigateToDetail1Clicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync("detail1");
		}

		async void OnNavigateToDetail2Clicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync("detail2");
		}

		async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new ShellNavigationOptionsPage(_viewModel));
		}

		async void OnGoBackClicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync("..");
		}

		void OnResetClicked(object sender, EventArgs e)
		{
			_viewModel.TextOverride = string.Empty;
			_viewModel.IconOverride = string.Empty;
			_viewModel.IsEnabled = true;
			_viewModel.IsVisible = true;
			_viewModel.CommandParameter = string.Empty;
			_viewModel.CommandExecuted = string.Empty;
			_viewModel.Navigating = string.Empty;
			_viewModel.Navigated = string.Empty;
			_viewModel.CurrentState = "Not Set";
			_viewModel.CurrentPage = "Not Set";
			_viewModel.CurrentItem = "Not Set";
			_viewModel.ShellCurrent = "Not Set";
		}
	}

	public class ShellDetailBasePage : ContentPage
	{
		readonly string _prefix;
		Label _currentStateLabel;
		Label _currentPageLabel;
		Label _currentItemLabel;
		Label _shellCurrentLabel;
		Label _commandExecutedLabel;
		Label _navigatingLabel;
		Label _navigatedLabel;

		public ShellDetailBasePage(string title, string prefix)
		{
			Title = title;
			AutomationId = $"{prefix}Page";
			_prefix = prefix;

			var behavior = new BackButtonBehavior();
			behavior.SetBinding(BackButtonBehavior.TextOverrideProperty, "TextOverride");
			behavior.SetBinding(BackButtonBehavior.IconOverrideProperty, "IconOverride");
			behavior.SetBinding(BackButtonBehavior.IsEnabledProperty, "IsEnabled");
			behavior.SetBinding(BackButtonBehavior.IsVisibleProperty, "IsVisible");
			behavior.SetBinding(BackButtonBehavior.CommandProperty, "Command");
			behavior.SetBinding(BackButtonBehavior.CommandParameterProperty, "CommandParameter");
			Shell.SetBackButtonBehavior(this, behavior);

			BuildUI();
			this.Appearing += OnPageAppearing;
		}

		void BuildUI()
		{
			_currentStateLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CurrentStateLabel" };
			_currentPageLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CurrentPageLabel" };
			_currentItemLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CurrentItemLabel" };
			_shellCurrentLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}ShellCurrentLabel" };
			_commandExecutedLabel = new Label { FontSize = 12, AutomationId = $"{_prefix}CommandExecutedLabel" };
			_navigatingLabel = new Label { FontSize = 11, AutomationId = $"{_prefix}NavigatingLabel" };
			_navigatedLabel = new Label { FontSize = 11, AutomationId = $"{_prefix}NavigatedLabel" };

			var goBackButton = new Button
			{
				Text = "Go Back",
				FontSize = 12,
				HeightRequest = 35,
				Padding = new Thickness(5, 0),
				AutomationId = $"{_prefix}GoBackButton"
			};
			goBackButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");

			var grid = new Grid
			{
				Padding = 10,
				RowSpacing = 4,
				ColumnSpacing = 10,
				RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
				ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }
			};

			AddRow(grid, 0, "CurrentState:", _currentStateLabel);
			AddRow(grid, 1, "CurrentPage:", _currentPageLabel);
			AddRow(grid, 2, "CurrentItem:", _currentItemLabel);
			AddRow(grid, 3, "Shell.Current:", _shellCurrentLabel);
			AddRow(grid, 4, "CommandExecuted:", _commandExecutedLabel);
			AddRow(grid, 5, "Navigating:", _navigatingLabel);
			AddRow(grid, 6, "Navigated:", _navigatedLabel);

			Grid.SetRow(goBackButton, 7);
			Grid.SetColumn(goBackButton, 0);
			Grid.SetColumnSpan(goBackButton, 2);
			grid.Children.Add(goBackButton);

			Content = grid;
		}

		static void AddRow(Grid grid, int row, string labelText, Label valueLabel)
		{
			var label = new Label { Text = labelText, FontSize = 12 };
			Grid.SetRow(label, row);
			Grid.SetColumn(label, 0);
			Grid.SetRow(valueLabel, row);
			Grid.SetColumn(valueLabel, 1);
			grid.Children.Add(label);
			grid.Children.Add(valueLabel);
		}

		void OnPageAppearing(object sender, EventArgs e)
		{
			var shell = Shell.Current;
			if (shell != null)
			{
				_currentStateLabel.Text = (shell.CurrentState?.Location?.ToString() ?? "Not Set").TrimStart('/');
				_currentPageLabel.Text = shell.CurrentPage?.Title ?? "Not Set";
				_currentItemLabel.Text = shell.CurrentItem?.Title ?? "Not Set";
				_shellCurrentLabel.Text = shell.GetType().Name;

				if (shell is ShellNavigationControlPage controlPage)
				{
					var vm = controlPage.ViewModel;
					_commandExecutedLabel.Text = vm.CommandExecuted;
					_navigatingLabel.Text = vm.Navigating;
					_navigatedLabel.Text = vm.Navigated;
					BindingContext = vm;
				}
			}
		}
	}

	public class DetailPage1 : ShellDetailBasePage
	{
		public DetailPage1() : base("Detail Page 1", "Detail1") { }
	}

	public class DetailPage2 : ShellDetailBasePage
	{
		public DetailPage2() : base("Detail Page 2", "Detail2") { }
	}
}
