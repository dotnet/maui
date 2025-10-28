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
		BindingContext = _viewModel = new SearchBarViewModel();
		ReInitializeSearchBar();
		await Navigation.PushAsync(new SearchBarOptionsPage(_viewModel));
	}

	private void ReInitializeSearchBar()
	{
		SearchBarGrid.Children.Clear();

		var searchBar = new UITestSearchBar
		{
			AutomationId = "SearchBar",
			IsCursorVisible = false
		};
		searchBar.SetBinding(SearchBar.CancelButtonColorProperty, nameof(SearchBarViewModel.CancelButtonColor));
		searchBar.SetBinding(SearchBar.CharacterSpacingProperty, nameof(SearchBarViewModel.CharacterSpacing));
		searchBar.SetBinding(SearchBar.CursorPositionProperty, nameof(SearchBarViewModel.CursorPosition));
		searchBar.SetBinding(SearchBar.FlowDirectionProperty, nameof(SearchBarViewModel.FlowDirection));
		searchBar.SetBinding(SearchBar.FontAttributesProperty, nameof(SearchBarViewModel.FontAttributes));
		searchBar.SetBinding(SearchBar.FontAutoScalingEnabledProperty, nameof(SearchBarViewModel.FontAutoScalingEnabled));
		searchBar.SetBinding(SearchBar.FontFamilyProperty, nameof(SearchBarViewModel.FontFamily));
		searchBar.SetBinding(SearchBar.FontSizeProperty, nameof(SearchBarViewModel.FontSize));
		searchBar.SetBinding(SearchBar.HorizontalTextAlignmentProperty, nameof(SearchBarViewModel.HorizontalTextAlignment));
		searchBar.SetBinding(SearchBar.IsEnabledProperty, nameof(SearchBarViewModel.IsEnabled));
		searchBar.SetBinding(SearchBar.IsVisibleProperty, nameof(SearchBarViewModel.IsVisible));
		searchBar.SetBinding(SearchBar.ShadowProperty, nameof(SearchBarViewModel.Shadow));
		searchBar.SetBinding(SearchBar.IsReadOnlyProperty, nameof(SearchBarViewModel.IsReadOnly));
		searchBar.SetBinding(SearchBar.IsSpellCheckEnabledProperty, nameof(SearchBarViewModel.IsSpellCheckEnabled));
		searchBar.SetBinding(SearchBar.IsTextPredictionEnabledProperty, nameof(SearchBarViewModel.IsTextPredictionEnabled));
		searchBar.SetBinding(SearchBar.KeyboardProperty, nameof(SearchBarViewModel.Keyboard));
		searchBar.SetBinding(SearchBar.MaxLengthProperty, nameof(SearchBarViewModel.MaxLength));
		searchBar.SetBinding(SearchBar.PlaceholderProperty, nameof(SearchBarViewModel.Placeholder));
		searchBar.SetBinding(SearchBar.PlaceholderColorProperty, nameof(SearchBarViewModel.PlaceholderColor));
		searchBar.SetBinding(SearchBar.SelectionLengthProperty, nameof(SearchBarViewModel.SelectionLength));
		searchBar.SetBinding(SearchBar.TextProperty, nameof(SearchBarViewModel.Text));
		searchBar.SetBinding(SearchBar.TextColorProperty, nameof(SearchBarViewModel.TextColor));
		searchBar.SetBinding(SearchBar.TextTransformProperty, nameof(SearchBarViewModel.TextTransform));
		searchBar.SetBinding(SearchBar.VerticalTextAlignmentProperty, nameof(SearchBarViewModel.VerticalTextAlignment));
		searchBar.SetBinding(SearchBar.SearchCommandProperty, nameof(SearchBarViewModel.SearchCommand));
		searchBar.SetBinding(SearchBar.SearchCommandParameterProperty, nameof(SearchBarViewModel.Text));

		searchBar.SearchButtonPressed += OnSearchButtonPressed;
		searchBar.TextChanged += OnTextChanged;

		NewTextChangedLabel.Text = string.Empty;
		OldTextChangedLabel.Text = string.Empty;
		SearchButtonPressedLabel.Text = "No";

		SearchBarGrid.Children.Add(searchBar);
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