namespace Maui.Controls.Sample;

public class SampleSearchHandler : SearchHandler
{
	internal static readonly string[] Fruits =
	[
		"Apple", "Apricot", "Avocado", "Banana", "Blackberry",
		"Blueberry", "Cherry", "Coconut", "Cranberry", "Date",
		"Fig", "Grape", "Grapefruit", "Guava", "Kiwi",
		"Lemon", "Lime", "Mango", "Melon", "Nectarine",
		"Orange", "Papaya", "Peach", "Pear", "Pineapple",
		"Plum", "Pomegranate", "Raspberry", "Strawberry", "Watermelon"
	];

	internal static readonly string[] Birds =
	[
		"Blue Jay", "Cardinal", "Crow", "Eagle", "Falcon",
		"Finch", "Flamingo", "Hawk", "Heron", "Hummingbird",
		"Kingfisher", "Magpie", "Nightingale", "Oriole", "Owl",
		"Parrot", "Peacock", "Pelican", "Penguin", "Pigeon",
		"Raven", "Robin", "Sparrow", "Starling", "Swan",
		"Toucan", "Turkey", "Vulture", "Woodpecker", "Wren"
	];

	ShellViewModel _viewModel = null!;

	public void SetViewModel(ShellViewModel vm)
	{
		_viewModel?.PropertyChanged -= OnViewModelPropertyChanged;

		_viewModel = vm;
		vm.PropertyChanged += OnViewModelPropertyChanged;
	}

	internal void Cleanup()
	{
		_viewModel?.PropertyChanged -= OnViewModelPropertyChanged;
	}

	void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ShellViewModel.ItemsSourceMode))
		{
			// Re-run the current query against the new data source
			OnQueryChanged(Query ?? string.Empty, Query ?? string.Empty);
		}
	}

	protected override void OnQueryChanged(string oldValue, string newValue)
	{
		base.OnQueryChanged(oldValue, newValue);

		if (_viewModel is null)
		{
			return;
		}

		_viewModel.QueryChangedLog = $"{oldValue}\u2192{newValue}";

		if (string.IsNullOrEmpty(newValue))
		{
			ItemsSource = null;
			return;
		}

		var mode = _viewModel.ItemsSourceMode;
		IEnumerable<string> source = mode switch
		{
			"Fruits" => Fruits,
			"Birds" => Birds,
			"Null" => null,
			_ => Fruits.Concat(Birds) // "Query" mode searches both
		};

		ItemsSource = source?
			.Where(s => s.Contains(newValue, StringComparison.OrdinalIgnoreCase))
			.ToList();
	}

	protected override async void OnItemSelected(object item)
	{
		base.OnItemSelected(item);
		if (_viewModel is null)
		{
			return;
		}

		var itemName = item?.ToString();
		_viewModel.SelectedItem = itemName;

		if (!string.IsNullOrEmpty(itemName))
		{
			try
			{
				await Shell.Current.GoToAsync(
					ShellSearchControlPage.DetailRoute,
					new Dictionary<string, object>
					{
						[nameof(ShellSearchControlPage.SearchDetailPage.ItemName)] = itemName
					});
			} 
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
			}
		}
	}
}

public partial class ShellSearchControlPage : Shell
{
	internal const string DetailRoute = "searchdetail";
	private readonly ShellViewModel _viewModel;

	public ShellSearchControlPage()
	{
		_viewModel = new ShellViewModel();
		BindingContext = _viewModel;
		InitializeComponent();

		Routing.RegisterRoute(DetailRoute, typeof(SearchDetailPage));

		SearchHandlerInstance.SetViewModel(_viewModel);
		SearchHandlerInstance.Focused += OnSearchHandlerFocused;
		SearchHandlerInstance.Unfocused += OnSearchHandlerUnfocused;
	}

	void OnSearchHandlerFocused(object sender, EventArgs e)
	{
		_viewModel.FocusStatus = "Focused";
		_viewModel.IsFocused = true;
	}

	void OnSearchHandlerUnfocused(object sender, EventArgs e)
	{
		_viewModel.FocusStatus = "Unfocused";
		_viewModel.IsFocused = false;
	}

	async void OnOptionsClicked(object sender, EventArgs e)
	{
		SearchHandlerInstance.Unfocus();
		SearchHandlerInstance.HideSoftInputAsync();
		_viewModel.Reset();
		await Navigation.PushAsync(new ShellSearchOptionsPage(_viewModel));
	}

	// ── Action buttons ────────────────────────────────────────────────────────────

	void OnShowSoftInputClicked(object sender, EventArgs e)
		=> SearchHandlerInstance.ShowSoftInputAsync();

	void OnHideSoftInputClicked(object sender, EventArgs e)
		=> SearchHandlerInstance.HideSoftInputAsync();

	void OnSetQueryClicked(object sender, EventArgs e)
		=> _viewModel.Query = "Robin";

	void OnTriggerClearPlaceholderClicked(object sender, EventArgs e)
		=> ((ISearchHandlerController)SearchHandlerInstance).ClearPlaceholderClicked();

	void OnToggleQueryIconClicked(object sender, EventArgs e)
	{
		var btn = (Button)sender;
		bool isCustom = SearchHandlerInstance.QueryIcon != null;
		SearchHandlerInstance.QueryIcon = isCustom ? null : ImageSource.FromFile("coffee.png");
		btn.Text = isCustom ? "QueryIcon:Default" : "QueryIcon:Custom";
	}

	void OnToggleClearIconClicked(object sender, EventArgs e)
	{
		var btn = (Button)sender;
		bool isCustom = SearchHandlerInstance.ClearIcon != null;
		SearchHandlerInstance.ClearIcon = isCustom ? null : ImageSource.FromFile("bank.png");
		btn.Text = isCustom ? "ClearIcon:Default" : "ClearIcon:Custom";
	}

	void OnToggleClearPlaceholderIconClicked(object sender, EventArgs e)
	{
		var btn = (Button)sender;
		bool isCustom = SearchHandlerInstance.ClearPlaceholderIcon != null;
		SearchHandlerInstance.ClearPlaceholderIcon = isCustom ? null : ImageSource.FromFile("avatar.png");
		btn.Text = isCustom ? "ClearPHIcon:Default" : "ClearPHIcon:Custom";
	}

	void OnToggleShowsResultsClicked(object sender, EventArgs e)
	{
		var btn = (Button)sender;
		_viewModel.ShowsResults = !_viewModel.ShowsResults;
		btn.Text = _viewModel.ShowsResults ? "ShowsResults:False" : "ShowsResults:True";
	}

	// ── Search Detail Page (navigated to when a search result is tapped) ─────────

	[QueryProperty(nameof(ItemName), nameof(ItemName))]
	internal sealed class SearchDetailPage : ContentPage
	{
		readonly Label _nameLabel;
		readonly Label _categoryLabel;

		string _itemName;
		public string ItemName
		{
			get => _itemName;
			set
			{
				_itemName = value;
				UpdateContent();
			}
		}

		public SearchDetailPage()
		{
			AutomationId = "SearchDetailPage";

			_nameLabel = new Label
			{
				FontSize = 24,
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center,
				AutomationId = "DetailName"
			};

			_categoryLabel = new Label
			{
				FontSize = 16,
				HorizontalTextAlignment = TextAlignment.Center,
				AutomationId = "DetailCategory"
			};

			var backButton = new Button
			{
				Text = "Back to Search",
				AutomationId = "BackToSearchButton",
			};
			backButton.Clicked += async (_, _) => await Shell.Current.GoToAsync("..");

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 16,
				Children = { _nameLabel, _categoryLabel, backButton }
			};
		}

		void UpdateContent()
		{
			if (string.IsNullOrEmpty(_itemName))
			{
				return;
			}

			bool isFruit = SampleSearchHandler.Fruits.Contains(_itemName, StringComparer.OrdinalIgnoreCase);
			Title = _itemName;
			_nameLabel.Text = _itemName;
			_categoryLabel.Text = isFruit ? "Fruit" : "Bird";
		}
	}
}
