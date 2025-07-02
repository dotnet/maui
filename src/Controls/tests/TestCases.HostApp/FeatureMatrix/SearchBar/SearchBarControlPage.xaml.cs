using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public class SearchBarControlPage : NavigationPage
{
	public SearchBarControlPage()
	{
		PushAsync(new SearchBarControlMainPage());
	}
}
public partial class SearchBarControlMainPage : ContentPage
{
	private SearchBarViewModel _viewModel;

	// List of all pages
	private ObservableCollection<PageEntry> _allPages;
	// List bound to the ListView
	private ObservableCollection<PageEntry> _filteredPages;
	private ListView _resultsListView;

	public SearchBarControlMainPage()
	{
		InitializeComponent();
		_viewModel = new SearchBarViewModel();
		BindingContext = _viewModel;

		// Initialize pages
		_allPages = new ObservableCollection<PageEntry>
			{
				new PageEntry("Page One", typeof(PageOne)),
				new PageEntry("Page Two", typeof(PageTwo)),
				new PageEntry("Page Three", typeof(PageThree)),
				new PageEntry("Page Four", typeof(PageFour)),
				new PageEntry("Page Five", typeof(PageFive)),
				new PageEntry("Page Six", typeof(PageSix)),
				new PageEntry("Page Seven", typeof(PageSeven)),
			};
		_filteredPages = new ObservableCollection<PageEntry>(_allPages);

		// Create ListView for results
		_resultsListView = new ListView
		{
			ItemsSource = _filteredPages,
			ItemTemplate = new DataTemplate(() =>
			{
				var cell = new TextCell();
				cell.SetBinding(TextCell.TextProperty, nameof(PageEntry.Title));
				return cell;
			}),
			SelectionMode = ListViewSelectionMode.Single,
			VerticalOptions = LayoutOptions.FillAndExpand,
		};
		_resultsListView.ItemTapped += ResultsListView_ItemTapped;

		// Add ListView to MainGrid (after SearchBar)
		MainGrid.Children.Add(_resultsListView);
		Grid.SetRow(_resultsListView, 1);
		Grid.SetColumnSpan(_resultsListView, 2);
	}

	private async void ResultsListView_ItemTapped(object sender, ItemTappedEventArgs e)
	{
		if (e.Item is PageEntry entry)
		{
			_resultsListView.SelectedItem = null;
			await Navigation.PushAsync((Page)Activator.CreateInstance(entry.PageType));
			// Restore full list when returning
			_filteredPages.Clear();
			foreach (var page in _allPages)
				_filteredPages.Add(page);
		}
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = new SearchBarViewModel();
		ReInitializePicker();
		await Navigation.PushAsync(new SearchBarOptionsPage(_viewModel));
	}


	private void ReInitializePicker()
	{
		PickerGrid.Children.Clear();
		var searchBar = new SearchBar
		{
			AutomationId = "SearchBar",
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

		// Event handlers
		searchBar.SearchButtonPressed += OnSearchButtonPressed;
		searchBar.TextChanged += OnTextChanged;

		NewTextChangedLabel.Text = string.Empty;
		OldTextChangedLabel.Text = string.Empty;

		PickerGrid.Children.Add(searchBar);
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
		if (_allPages == null || _filteredPages == null)
			return;

		if (!string.IsNullOrEmpty(e.NewTextValue))
		{
			NewTextChangedLabel.Text = e.NewTextValue;
			OldTextChangedLabel.Text = e.OldTextValue;
			var keyword = e.NewTextValue.ToLowerInvariant();
			_filteredPages.Clear();
			foreach (var page in _allPages.Where(p => p.Title.ToLowerInvariant().Contains(keyword, System.StringComparison.Ordinal)))
				_filteredPages.Add(page);
		}
		else
		{
			_filteredPages.Clear();
			foreach (var page in _allPages)
				_filteredPages.Add(page);
		}
	}
}

public class PageEntry
{
	public string Title { get; }
	public Type PageType { get; }
	public PageEntry(string title, Type pageType)
	{
		Title = title;
		PageType = pageType;
	}
}