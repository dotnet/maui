using System.Collections.ObjectModel;
using System.Text;

namespace Maui.Controls.Sample;

public partial class SettingsPage : ContentPage
{
	private readonly StringBuilder _logBuilder = new();
	private readonly Random _random = new();

	// Predefined color themes for SearchHandler
	private static readonly (string Name, Color Background, Color Text, Color Placeholder, Color CancelButton)[] ColorThemes =
	[
		("Blue", Color.FromArgb("#E3F2FD"), Color.FromArgb("#1565C0"), Color.FromArgb("#64B5F6"), Color.FromArgb("#1976D2")),
		("Green", Color.FromArgb("#E8F5E9"), Color.FromArgb("#2E7D32"), Color.FromArgb("#81C784"), Color.FromArgb("#388E3C")),
		("Purple", Color.FromArgb("#F3E5F5"), Color.FromArgb("#7B1FA2"), Color.FromArgb("#BA68C8"), Color.FromArgb("#8E24AA")),
		("Orange", Color.FromArgb("#FFF3E0"), Color.FromArgb("#E65100"), Color.FromArgb("#FFB74D"), Color.FromArgb("#F57C00")),
		("Red", Color.FromArgb("#FFEBEE"), Color.FromArgb("#C62828"), Color.FromArgb("#EF9A9A"), Color.FromArgb("#D32F2F")),
		("Teal", Color.FromArgb("#E0F2F1"), Color.FromArgb("#00695C"), Color.FromArgb("#80CBC4"), Color.FromArgb("#00897B")),
		("Pink", Color.FromArgb("#FCE4EC"), Color.FromArgb("#AD1457"), Color.FromArgb("#F48FB1"), Color.FromArgb("#C2185B")),
		("Amber", Color.FromArgb("#FFF8E1"), Color.FromArgb("#FF6F00"), Color.FromArgb("#FFD54F"), Color.FromArgb("#FFB300")),
	];

	public SettingsPage()
	{
		InitializeComponent();

		// Set initial picker value to Expanded (index 1)
		visibilityPicker.SelectedIndex = 1; // Expanded

		// Subscribe to search handler events
		searchHandler.QueryChangedEvent += OnSearchQueryChanged;
		searchHandler.ItemSelectedEvent += OnSearchItemSelected;
	}

	async void Button_Clicked(object sender, EventArgs e)
	{
		Log("PopToRootAsync called");
		await Navigation.PopToRootAsync();
	}

	async void OnPushDetailsClicked(object sender, EventArgs e)
	{
		Log("Pushing DetailsPage...");
		await Navigation.PushAsync(new DetailsPage());
	}

	void OnSearchQueryChanged(object? sender, string query)
	{
		currentQueryLabel.Text = string.IsNullOrEmpty(query) ? "(empty)" : query;
		Log($"QueryChanged: '{query}'");
	}

	void OnSearchItemSelected(object? sender, object? item)
	{
		if (item is SearchItem searchItem)
		{
			Log($"ItemSelected: {searchItem.Name} ({searchItem.Category})");
			DisplayAlertAsync("Selected", $"You selected: {searchItem.Name}\nCategory: {searchItem.Category}", "OK");
		}
	}

	void OnVisibilityChanged(object? sender, EventArgs e)
	{
		if (visibilityPicker.SelectedItem is string visibility)
		{
			var searchBoxVisibility = visibility switch
			{
				"Collapsible" => SearchBoxVisibility.Collapsible,
				"Expanded" => SearchBoxVisibility.Expanded,
				"Hidden" => SearchBoxVisibility.Hidden,
				_ => SearchBoxVisibility.Collapsible
			};

			searchHandler.SearchBoxVisibility = searchBoxVisibility;
			Log($"SearchBoxVisibility changed to: {visibility}");
		}
	}

	void OnShowsResultsToggled(object? sender, ToggledEventArgs e)
	{
		searchHandler.ShowsResults = e.Value;
		Log($"ShowsResults changed to: {e.Value}");
	}

	void OnIsEnabledToggled(object? sender, ToggledEventArgs e)
	{
		searchHandler.IsSearchEnabled = e.Value;
		Log($"IsSearchEnabled changed to: {e.Value}");
	}

	void OnClearLogClicked(object? sender, EventArgs e)
	{
		_logBuilder.Clear();
		logLabel.Text = "Log cleared.";
	}

	void OnRandomizeColorsClicked(object? sender, EventArgs e)
	{
		// Pick a random color theme
		var theme = ColorThemes[_random.Next(ColorThemes.Length)];

		// Apply to SearchHandler
		searchHandler.BackgroundColor = theme.Background;
		searchHandler.TextColor = theme.Text;
		searchHandler.PlaceholderColor = theme.Placeholder;
		searchHandler.CancelButtonColor = theme.CancelButton;

		// Update UI
		currentColorsLabel.Text = $"{theme.Name} theme";
		currentColorsLabel.TextColor = theme.Text;

		Log($"Colors changed to: {theme.Name} (BG: {theme.Background.ToHex()}, Text: {theme.Text.ToHex()})");
	}

	void Log(string message)
	{
		var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
		_logBuilder.Insert(0, $"[{timestamp}] {message}\n");
		logLabel.Text = _logBuilder.ToString();
		Console.WriteLine($"SANDBOX SearchHandler: {message}");
	}
}

/// <summary>
/// Custom SearchHandler that exposes events for query and item selection changes.
/// SearchHandler uses virtual methods, so we override them and raise events.
/// </summary>
public class SandboxSearchHandler : SearchHandler
{
	private static readonly ObservableCollection<SearchItem> _allItems = new()
	{
		new SearchItem { Name = "Apple", Category = "Fruit" },
		new SearchItem { Name = "Banana", Category = "Fruit" },
		new SearchItem { Name = "Cherry", Category = "Fruit" },
		new SearchItem { Name = "Date", Category = "Fruit" },
		new SearchItem { Name = "Elderberry", Category = "Fruit" },
		new SearchItem { Name = "Carrot", Category = "Vegetable" },
		new SearchItem { Name = "Broccoli", Category = "Vegetable" },
		new SearchItem { Name = "Spinach", Category = "Vegetable" },
		new SearchItem { Name = "Settings", Category = "App" },
		new SearchItem { Name = "Search", Category = "App" },
		new SearchItem { Name = "Shell", Category = "MAUI" },
		new SearchItem { Name = "Handler", Category = "MAUI" },
		new SearchItem { Name = "Navigation", Category = "MAUI" },
	};

	public event EventHandler<string>? QueryChangedEvent;
	public event EventHandler<object?>? ItemSelectedEvent;

	protected override void OnQueryChanged(string oldValue, string newValue)
	{
		base.OnQueryChanged(oldValue, newValue);

		Console.WriteLine($"SANDBOX SearchHandler.OnQueryChanged: '{oldValue}' -> '{newValue}'");

		// Raise event for UI updates
		QueryChangedEvent?.Invoke(this, newValue ?? string.Empty);

		// Filter and update ItemsSource
		if (string.IsNullOrWhiteSpace(newValue))
		{
			ItemsSource = null;
		}
		else
		{
			var results = _allItems
				.Where(item => item.Name.Contains(newValue, StringComparison.OrdinalIgnoreCase))
				.ToList();

			ItemsSource = results;
			Console.WriteLine($"SANDBOX SearchHandler: Found {results.Count} results");
		}
	}

	protected override void OnItemSelected(object item)
	{
		base.OnItemSelected(item);

		Console.WriteLine($"SANDBOX SearchHandler.OnItemSelected: {item}");

		// Raise event for UI updates
		ItemSelectedEvent?.Invoke(this, item);

		// Clear the search
		Query = string.Empty;
	}
}

/// <summary>
/// Simple item class for search results
/// </summary>
public class SearchItem
{
	public string Name { get; set; } = string.Empty;
	public string Category { get; set; } = string.Empty;
}

