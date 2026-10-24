namespace Maui.Controls.Sample;

public class SearchBarControlPage : NavigationPage
{
	private SearchBarViewModel _viewModel;
	public SearchBarControlPage()
	{
		_viewModel = new SearchBarViewModel();
		PushAsync(new SearchBarControlMainPage(_viewModel));
	}
}
public partial class SearchBarControlMainPage : ContentPage
{
	private SearchBarViewModel _viewModel;

	public SearchBarControlMainPage(SearchBarViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		_viewModel.Reset();

		NewTextChangedLabel.Text = string.Empty;
		OldTextChangedLabel.Text = string.Empty;
		SearchButtonPressedLabel.Text = "No";

		await Navigation.PushAsync(new SearchBarOptionsPage(_viewModel));
	}

	private void OnSearchButtonPressed(object sender, EventArgs e)
	{
		var searchBar = sender as SearchBar;
		if (searchBar != null && !string.IsNullOrEmpty(searchBar.Text))
		{
			SearchButtonPressedLabel.Text = "Yes";
		}
	}

	private void OnTextChanged(object sender, TextChangedEventArgs e)
	{
		if (!string.IsNullOrEmpty(e.NewTextValue))
		{
			NewTextChangedLabel.Text = e.NewTextValue;
			OldTextChangedLabel.Text = e.OldTextValue;
		}
	}
}