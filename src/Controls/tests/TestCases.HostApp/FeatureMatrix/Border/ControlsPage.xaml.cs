namespace Maui.Controls.Sample
{

public class BorderControlPage : NavigationPage
{
	private BorderViewModel _viewModel;

	public BorderControlPage()
	{
		_viewModel = new BorderViewModel();
		PushAsync(new BorderControlMainPage(_viewModel));
	}
}

public partial class BorderControlMainPage : ContentPage
{
	private BorderViewModel _viewModel;

	public BorderControlMainPage(BorderViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new BorderViewModel();
		await Navigation.PushAsync(new OptionsPage(_viewModel));
	}

	private void OnSetGridContentClicked(object sender, EventArgs e)
	{
		var grid = new Grid
		{
			RowDefinitions =
				{
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Star)
				},
			ColumnDefinitions =
				{
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(GridLength.Star)
				},
			BackgroundColor = Colors.LightGoldenrodYellow
		};

		grid.Add(new Label { Text = "Top Left", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }, 0, 0);
		grid.Add(new Label { Text = "Top Right", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }, 1, 0);
		grid.Add(new Label { Text = "Bottom Left", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }, 0, 1);
		grid.Add(new Label { Text = "Bottom Right", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }, 1, 1);

		// You may want to set this grid as the content of a Border or the page itself
		// Border.Content = grid;
	}

	private void OnSetStackContentClicked(object sender, EventArgs e)
	{
		var stack = new VerticalStackLayout
		{
			Spacing = 10,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		stack.Children.Add(new Label { Text = "First Line", FontSize = 18, TextColor = Colors.Black });
		stack.Children.Add(new Label { Text = "Second Line", FontSize = 18, TextColor = Colors.DarkBlue });

		// You may want to set this stack as the content of a Border or the page itself
		// Border.Content = stack;
	}
}

}