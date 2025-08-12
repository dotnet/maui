using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public class LayoutControlPage : NavigationPage
{
	private LayoutViewModel _viewModel;
	public LayoutControlPage()
	{
		_viewModel = new LayoutViewModel();

		PushAsync(new LayoutMainPage(_viewModel));
	}
}

public partial class LayoutMainPage : ContentPage
{
	private LayoutViewModel _viewModel;

	public LayoutMainPage(LayoutViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
		InitializeContent();
	}
	protected override void OnAppearing()
	{
		base.OnAppearing();
		InitializeContent();
	}

	private void InitializeContent()
	{
		var defaultLayout = new VerticalStackLayout
		{
			BackgroundColor = Colors.LightGray,
			Children =
			{
				new Label { Text = "Welcome to LayoutPage" },
			}
		};

		MyScrollView.Content = defaultLayout;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new LayoutViewModel();
		await Navigation.PushAsync(new LayoutOptionsPage(_viewModel));
	}

	private void OnScrollViewWithStackLayoutClicked(object sender, EventArgs e)
	{
		Layout layout;

		if (_viewModel.Orientation == ScrollOrientation.Horizontal)
		{
			layout = new HorizontalStackLayout
			{
				HorizontalOptions = _viewModel.HorizontalOptions,
				VerticalOptions = _viewModel.VerticalOptions,
				BackgroundColor = Colors.LightGray,
				Spacing = 10,
				Children =
			{
				new Label { Text = "StackLayout", VerticalOptions = LayoutOptions.Center  },
				new Button { Text = "Button1", VerticalOptions = LayoutOptions.Center },
				new Button { Text = "Button2", VerticalOptions = LayoutOptions.Center }
			}
			};
		}
		else
		{
			layout = new VerticalStackLayout
			{
				HorizontalOptions = _viewModel.HorizontalOptions,
				VerticalOptions = _viewModel.VerticalOptions,
				BackgroundColor = Colors.LightGray,
				Spacing = 10,
				Children =
			{
				new Label { Text = "StackLayout", HorizontalOptions = LayoutOptions.Center },
				new Button { Text = "Button1", HorizontalOptions = LayoutOptions.Center  },
				new Button { Text = "Button2", HorizontalOptions = LayoutOptions.Center }
			}
			};
		}

		MyScrollView.Content = layout;
	}

	private void OnGridWithChildrenClicked(object sender, EventArgs e)
	{
		var grid = new Grid
		{
			Padding = 15,
			BackgroundColor = Colors.LightGray,
			HorizontalOptions = _viewModel.HorizontalOptions,
			VerticalOptions = _viewModel.VerticalOptions,
			RowSpacing = 10,
			ColumnSpacing = 10
		};

		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

		var button1 = new Button { Text = "Button 1", BackgroundColor = Colors.Orange, TextColor = Colors.White };
		var button2 = new Button { Text = "Button 2", BackgroundColor = Colors.Blue, TextColor = Colors.White };
		var button3 = new Button { Text = "Button 3", BackgroundColor = Colors.Green, TextColor = Colors.White };
		var button4 = new Button { Text = "Button 4", BackgroundColor = Colors.Red, TextColor = Colors.White };
		var button5 = new Button { Text = "Button 5", BackgroundColor = Colors.Purple, TextColor = Colors.White };
		var button6 = new Button { Text = "Button 6", BackgroundColor = Colors.Brown, TextColor = Colors.White };
		grid.Add(button1, 0, 0);
		grid.Add(button2, 1, 0);
		grid.Add(button3, 2, 0);
		grid.Add(button4, 0, 1);
		grid.Add(button5, 1, 1);
		grid.Add(button6, 2, 1);
		MyScrollView.Content = grid;
	}
}