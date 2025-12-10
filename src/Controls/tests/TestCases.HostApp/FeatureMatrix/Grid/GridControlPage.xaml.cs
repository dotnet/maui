using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class GridControlPage : NavigationPage
{
	private GridViewModel _viewModel;
	public GridControlPage()
	{
		_viewModel = new GridViewModel();

		PushAsync(new GridControlMainPage(_viewModel));
	}
}

public partial class GridControlMainPage : ContentPage
{
	private GridViewModel _viewModel;

	public GridControlMainPage(GridViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new GridViewModel();
		await Navigation.PushAsync(new GridOptionsPage(_viewModel));
	}
	protected override void OnAppearing()
	{
		base.OnAppearing();
		MainGrid.Children.Clear();

		var mainBox = new BoxView
		{
			Color = Colors.LightBlue,
			AutomationId = "Boxview1"
		};
		MainGrid.Add(mainBox, 0, 0);
		Grid.SetRowSpan(mainBox, _viewModel.MainContentRowSpan);
		Grid.SetColumnSpan(mainBox, _viewModel.MainContentColumnSpan);

		for (int row = 0; row < _viewModel.Row; row++)
		{
			for (int col = 0; col < _viewModel.Column; col++)
			{
				if (row == 0 && col == 0)
					continue;

				var cellColor = ((row + col) % 2 == 0) ? Colors.LightBlue : Colors.LightGreen;

				if (_viewModel.ShowNestedGrid && row == 1 && col == 1)
				{
					var nestedGrid = new Grid
					{
						BackgroundColor = Colors.LightGray,
						RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Star }
				},
						ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star }
				}
					};

					nestedGrid.Add(new BoxView { Color = Colors.Red }, 0, 0);
					nestedGrid.Add(new BoxView { Color = Colors.Blue }, 1, 0);
					nestedGrid.Add(new BoxView { Color = Colors.Green }, 0, 1);
					nestedGrid.Add(new BoxView { Color = Colors.Yellow }, 1, 1);

					MainGrid.Add(nestedGrid, col, row);
				}
				else
				{
					var box = new BoxView
					{
						Color = cellColor,
						AutomationId = $"BoxView_{row}_{col}"
					};
					MainGrid.Add(box, col, row);
				}
			}
		}
	}
}