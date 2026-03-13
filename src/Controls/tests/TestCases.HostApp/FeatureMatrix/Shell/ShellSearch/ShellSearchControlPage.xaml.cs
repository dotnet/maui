namespace Maui.Controls.Sample;

public class SampleSearchHandler : SearchHandler
{
	static readonly string[] Fruits =
	[
		"Apple", "Apricot", "Avocado", "Banana", "Blackberry",
		"Blueberry", "Cherry", "Coconut", "Cranberry", "Date",
		"Fig", "Grape", "Grapefruit", "Guava", "Kiwi",
		"Lemon", "Lime", "Mango", "Melon", "Nectarine",
		"Orange", "Papaya", "Peach", "Pear", "Pineapple",
		"Plum", "Pomegranate", "Raspberry", "Strawberry", "Watermelon"
	];

	static readonly string[] Birds =
	[
		"Blue Jay", "Cardinal", "Crow", "Eagle", "Falcon",
		"Finch", "Flamingo", "Hawk", "Heron", "Hummingbird",
		"Kingfisher", "Magpie", "Nightingale", "Oriole", "Owl",
		"Parrot", "Peacock", "Pelican", "Penguin", "Pigeon",
		"Raven", "Robin", "Sparrow", "Starling", "Swan",
		"Toucan", "Turkey", "Vulture", "Woodpecker", "Wren"
	];

#pragma warning disable CS0649 // Field is assigned in SetViewModel
	ShellViewModel _viewModel;
#pragma warning restore CS0649

	public void SetViewModel(ShellViewModel vm)
	{
		_viewModel = vm;
		vm.PropertyChanged += OnViewModelPropertyChanged;
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

		_viewModel.QueryChangedLog = $"{oldValue}→{newValue}";

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

	protected override void OnItemSelected(object item)
	{
		base.OnItemSelected(item);
		if (_viewModel is null)
		{
			return;
		}

		_viewModel.SelectedItem = item?.ToString();
	}
}

public partial class ShellSearchControlPage : Shell
{
	private readonly ShellViewModel _viewModel;

	public ShellSearchControlPage()
	{
		_viewModel = new ShellViewModel();
		BindingContext = _viewModel;
		InitializeComponent();

		SearchHandlerInstance.SetViewModel(_viewModel);
		SearchHandlerInstance.Focused += OnSearchHandlerFocused;
		SearchHandlerInstance.Unfocused += OnSearchHandlerUnfocused;

		// ItemTemplate is not in XAML — apply from ViewModel and keep in sync
		SearchHandlerInstance.ItemTemplate = _viewModel.ItemTemplate;
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
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

	void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		// ItemTemplate is not bound in XAML — sync manually
		if (e.PropertyName == nameof(ShellViewModel.ItemTemplate))
		{
			SearchHandlerInstance.ItemTemplate = _viewModel.ItemTemplate;
		}
	}

	// ── Toolbar ──────────────────────────────────────────────────────────────────

	async void OnOptionsClicked(object sender, EventArgs e)
	{
		_viewModel.Reset();
		await Navigation.PushAsync(new ShellSearchOptionsPage(_viewModel));
	}

	// ── Action buttons ────────────────────────────────────────────────────────────

	void OnFocusClicked(object sender, EventArgs e)
		=> SearchHandlerInstance.Focus();

	void OnUnfocusClicked(object sender, EventArgs e)
		=> SearchHandlerInstance.Unfocus();

	void OnShowSoftInputClicked(object sender, EventArgs e)
		=> SearchHandlerInstance.ShowSoftInputAsync();

	void OnHideSoftInputClicked(object sender, EventArgs e)
		=> SearchHandlerInstance.HideSoftInputAsync();

	void OnSetQueryClicked(object sender, EventArgs e)
		=> _viewModel.Query = "Robin";

	void OnTriggerClearPlaceholderClicked(object sender, EventArgs e)
		=> ((ISearchHandlerController)SearchHandlerInstance).ClearPlaceholderClicked();
}
