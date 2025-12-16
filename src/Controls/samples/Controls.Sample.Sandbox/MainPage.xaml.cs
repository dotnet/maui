using System.Threading.Tasks;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		Console.WriteLine("SANDBOX: MainPage loaded successfully");

		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Info",
			Order = ToolbarItemOrder.Secondary,
			Command = new Command(() =>
			{
				Console.WriteLine("SANDBOX: Info toolbar item clicked");
				DisplayAlertAsync("Info", "This is the Info button from toolbar", "OK");
			})
		});

		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Help",
			Order = ToolbarItemOrder.Secondary,
			Command = new Command(() =>
			{
				Console.WriteLine("SANDBOX: Help toolbar item clicked");
				DisplayAlertAsync("Help", "This is the Help button from toolbar", "OK");
			})
		});
	}

	async void Button_Clicked(object sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Navigation button clicked");
		await Navigation.PushAsync(new DetailsPage());
		Console.WriteLine("SANDBOX: Navigated to DetailsPage");
	}

	async void ToggleFlyoutButton_Clicked(object sender, EventArgs e)
	{
		Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
	}

	async void ChangeFlyoutBackgroundButton_Clicked(object sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Change Flyout Background button clicked");
		var random = new Random();
		var color = Color.FromRgb(random.Next(256), random.Next(256), random.Next(256));
		Shell.Current.FlyoutBackground = new SolidColorBrush(color);
		Console.WriteLine($"SANDBOX: Flyout background changed to {color}");
	}

	void OnInfoClicked(object sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Info toolbar item clicked");
		DisplayAlertAsync("Info", "This is the Info button from toolbar", "OK");
	}

	void OnHelpClicked(object sender, EventArgs e)
	{
		Console.WriteLine("SANDBOX: Help toolbar item clicked");
		DisplayAlertAsync("Help", "This is the Help button from toolbar", "OK");
	}
}

public partial class DetailsPage : ContentPage
{
	public DetailsPage()
	{
		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Details Info",
			Command = new Command(() =>
			{
				Console.WriteLine("SANDBOX: Details Info toolbar item clicked");
			})
		});

		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Details Help",
			Command = new Command(() =>
			{
				Console.WriteLine("SANDBOX: Details Help toolbar item clicked");
			})
		});
		Console.WriteLine("SANDBOX: DetailsPage constructor called");

		var stackDepthLabel = new Label
		{
			Text = $"Navigation Stack Depth: {Shell.Current?.Navigation?.NavigationStack?.Count ?? 0}",
			FontSize = 14,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			TextColor = Colors.Blue
		};

		var label = new Label
		{
			Text = "This is the Details Page",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var instructionLabel = new Label
		{
			Text = "Try using the hardware back button!",
			FontSize = 12,
			HorizontalOptions = LayoutOptions.Center,
			TextColor = Colors.Gray,
			Margin = new Thickness(0, 10, 0, 0)
		};

		var backButton = new Button
		{
			Text = "Go Back",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var popToRootButton = new Button
		{
			Text = "Pop to Root",
			AutomationId = "PopToRootButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			BackgroundColor = Colors.Green,
			TextColor = Colors.White
		};

		var pushAnotherButton = new Button
		{
			Text = "Push Another Page",
			AutomationId = "PushAnotherButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var insertBeforeButton = new Button
		{
			Text = "Insert Page Before This",
			AutomationId = "InsertBeforeButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			BackgroundColor = Colors.Orange,
			TextColor = Colors.White
		};

		var removePageButton = new Button
		{
			Text = "Remove Previous Page",
			AutomationId = "RemovePageButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			BackgroundColor = Colors.Red,
			TextColor = Colors.White
		};

		label.HorizontalOptions = LayoutOptions.Center;
		label.VerticalOptions = LayoutOptions.Center;
		backButton.HorizontalOptions = LayoutOptions.Center;
		backButton.VerticalOptions = LayoutOptions.Center;
		backButton.Clicked += async (s, e) =>
		{
			Console.WriteLine("SANDBOX: Back button clicked");
			await Navigation.PopAsync();
			Console.WriteLine("SANDBOX: Navigated back to MainPage");
		};

		popToRootButton.Clicked += async (s, e) =>
		{
			Console.WriteLine("SANDBOX: PopToRoot button clicked");
			await Navigation.PopToRootAsync();
			Console.WriteLine("SANDBOX: Popped to root");
		};

		pushAnotherButton.Clicked += async (s, e) =>
		{
			Console.WriteLine("SANDBOX: Push another page button clicked");
			await Navigation.PushAsync(new DetailsPage());
			Console.WriteLine("SANDBOX: Pushed another DetailsPage");
		};

		insertBeforeButton.Clicked += (s, e) =>
		{
			Console.WriteLine("SANDBOX: Insert before button clicked");
			try
			{
				// Insert a new page before the current page
				var insertedPage = new ContentPage
				{
					Title = "Inserted Page",
					Content = new Label
					{
						Text = "This page was inserted!",
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center
					}
				};
				Navigation.InsertPageBefore(insertedPage, this);
				Console.WriteLine("SANDBOX: Page inserted successfully");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SANDBOX: Insert failed: {ex.Message}");
			}
		};

		removePageButton.Clicked += (s, e) =>
		{
			Console.WriteLine("SANDBOX: Remove page button clicked");
			try
			{
				var stack = Navigation.NavigationStack;
				if (stack.Count > 2)
				{
					// Remove the previous page (one before current)
					var pageToRemove = stack[stack.Count - 2];
					Navigation.RemovePage(pageToRemove);
					Console.WriteLine($"SANDBOX: Removed page: {pageToRemove.GetType().Name}");
				}
				else
				{
					Console.WriteLine("SANDBOX: Not enough pages to remove");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"SANDBOX: Remove failed: {ex.Message}");
			}
		};

		Content = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30, 0),
			Children = { stackDepthLabel, label, instructionLabel, backButton, popToRootButton, pushAnotherButton, insertBeforeButton, removePageButton }
		};

		// Update stack depth label when page appears
		this.Appearing += (s, e) =>
		{
			stackDepthLabel.Text = $"Navigation Stack Depth: {Shell.Current?.Navigation?.NavigationStack?.Count ?? 0}";
		};

		Console.WriteLine("SANDBOX: DetailsPage loaded successfully");
	}
}

// Phase 4 Test Pages: Multiple ShellContent within a Tab
public partial class Content1Page : ContentPage
{
	public Content1Page()
	{
		Title = "Content 1";
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(30, 0),
			Children =
			{
				new Label
				{
					Text = "This is Content 1",
					FontSize = 24,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				},
				new Label
				{
					Text = "Swipe left/right or use tabs to switch content",
					FontSize = 14,
					HorizontalOptions = LayoutOptions.Center,
					TextColor = Colors.Gray
				},
				new Button
				{
					Text = "Push Navigation Page",
					HorizontalOptions = LayoutOptions.Center,
					Command = new Command(async () => await Navigation.PushAsync(new DetailsPage()))
				}
			}
		};
		Console.WriteLine("SANDBOX: Content1Page loaded");
	}
}

public partial class Content2Page : ContentPage
{
	public Content2Page()
	{
		Title = "Content 2";
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(30, 0),
			Children =
			{
				new Label
				{
					Text = "This is Content 2",
					FontSize = 24,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Blue
				},
				new Label
				{
					Text = "Each content maintains its own state",
					FontSize = 14,
					HorizontalOptions = LayoutOptions.Center,
					TextColor = Colors.Gray
				},
				new Entry
				{
					Placeholder = "Type something to test state preservation",
					HorizontalOptions = LayoutOptions.Center,
					WidthRequest = 300
				}
			}
		};
		Console.WriteLine("SANDBOX: Content2Page loaded");
	}
}

public partial class Content3Page : ContentPage
{
	public Content3Page()
	{
		Title = "Content 3";
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(30, 0),
			Children =
			{
				new Label
				{
					Text = "This is Content 3",
					FontSize = 24,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.Green
				},
				new Label
				{
					Text = "Testing ViewPager2 + TabLayout integration",
					FontSize = 14,
					HorizontalOptions = LayoutOptions.Center,
					TextColor = Colors.Gray
				},
				new BoxView
				{
					Color = Colors.Green,
					HeightRequest = 100,
					WidthRequest = 100,
					HorizontalOptions = LayoutOptions.Center
				}
			}
		};
		Console.WriteLine("SANDBOX: Content3Page loaded");
	}
}

// Phase 5 Test Page: SearchHandler
public partial class SearchTestPage : ContentPage
{
	private Label _resultsLabel;
	private Label _queryLabel;

	public SearchTestPage()
	{
		Title = "Search Test";

		_queryLabel = new Label
		{
			Text = "Query: (none)",
			FontSize = 16,
			FontAttributes = FontAttributes.Bold,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 20, 0, 0)
		};

		_resultsLabel = new Label
		{
			Text = "Selected: (none)",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 10, 0, 0)
		};

		// Create SearchHandler
		var searchHandler = new TestSearchHandler(_queryLabel, _resultsLabel)
		{
			Placeholder = "Search for fruits...",
			ShowsResults = true,
			SearchBoxVisibility = SearchBoxVisibility.Expanded,
			IsSearchEnabled = true,
			BackgroundColor = Colors.LightBlue,
			TextColor = Colors.Black,
			PlaceholderColor = Colors.Gray
		};

		Shell.SetSearchHandler(this, searchHandler);

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(30, 0),
			Children =
			{
				new Label
				{
					Text = "Phase 5: SearchHandler Test",
					FontSize = 24,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(0, 40, 0, 0)
				},
				new Label
				{
					Text = "Search functionality should appear in the toolbar above",
					FontSize = 14,
					HorizontalOptions = LayoutOptions.Center,
					TextColor = Colors.Gray
				},
				_queryLabel,
				_resultsLabel,
				new Label
				{
					Text = "Try searching for: Apple, Banana, Orange, Grape, Mango",
					FontSize = 12,
					HorizontalOptions = LayoutOptions.Center,
					TextColor = Colors.Blue,
					Margin = new Thickness(0, 20, 0, 0)
				}
			}
		};

		Console.WriteLine("SANDBOX: SearchTestPage loaded with SearchHandler");
	}
}

// Custom SearchHandler for testing
public class TestSearchHandler : SearchHandler
{
	private readonly List<string> _fruits = new List<string>
	{
		"Apple", "Apricot", "Avocado",
		"Banana", "Blackberry", "Blueberry",
		"Orange", "Olive",
		"Grape", "Grapefruit", "Guava",
		"Mango", "Melon"
	};

	private readonly Label _queryLabel;
	private readonly Label _resultsLabel;

	public TestSearchHandler(Label queryLabel, Label resultsLabel)
	{
		_queryLabel = queryLabel;
		_resultsLabel = resultsLabel;
		ClearIconHelpText = "Clear search";
		ClearIconName = "clear";
		ClearPlaceholderHelpText = "Clear";
	}

	protected override void OnQueryChanged(string oldValue, string newValue)
	{
		base.OnQueryChanged(oldValue, newValue);

		_queryLabel.Text = $"Query: {newValue ?? "(none)"}";
		Console.WriteLine($"SANDBOX: SearchHandler Query changed: {newValue}");

		if (string.IsNullOrWhiteSpace(newValue))
		{
			ItemsSource = null;
		}
		else
		{
			ItemsSource = _fruits
				.Where(f => f.Contains(newValue, StringComparison.OrdinalIgnoreCase))
				.ToList();
		}
	}

	protected override void OnItemSelected(object item)
	{
		base.OnItemSelected(item);

		if (item is string fruit)
		{
			_resultsLabel.Text = $"Selected: {fruit}";
			Console.WriteLine($"SANDBOX: SearchHandler Item selected: {fruit}");
		}
	}

	protected override void OnQueryConfirmed()
	{
		base.OnQueryConfirmed();
		Console.WriteLine($"SANDBOX: SearchHandler Query confirmed: {Query}");
	}
}