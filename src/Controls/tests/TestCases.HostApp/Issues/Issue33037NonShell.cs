using ControlsPage = Microsoft.Maui.Controls.Page;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33037, "iOS Large Title display disappears when scrolling in non-Shell NavigationPage", PlatformAffected.iOS, issueTestNumber: 1)]
public class Issue33037NonShell : NavigationPage
{
	public Issue33037NonShell() : base(new Issue33037NonShellRootPage())
	{
		BarBackgroundColor = Colors.Transparent;
		BackgroundColor = Colors.Brown;
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.NavigationPage.SetPrefersLargeTitles(this, true);
	}
}

public class Issue33037NonShellRootPage : ContentPage
{
	public Issue33037NonShellRootPage()
	{
		Title = "Issue 33037 Non-Shell";

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 12,
			Children =
			{
				new Label
				{
					Text = "Select a non-Shell NavigationPage large-title scenario.",
					FontAttributes = FontAttributes.Bold
				},
				CreateButton("Issue33037ScrollViewButton", "Direct ScrollView", () => new Issue33037NonShellScrollViewPage()),
				CreateButton("Issue33037GridScrollViewButton", "Grid wrapping ScrollView", () => new Issue33037NonShellGridScrollViewPage()),
				CreateButton("Issue33037ContentViewGridScrollViewButton", "ContentView wrapping Grid/ScrollView", () => new Issue33037NonShellContentViewGridScrollViewPage()),
				CreateButton("Issue33037DynamicContentViewGridScrollViewButton", "Late ContentView wrapping Grid/ScrollView", () => new Issue33037NonShellDynamicContentViewGridScrollViewPage()),
				CreateButton("Issue33037ListViewButton", "ListView", () => new Issue33037NonShellListViewPage()),
				CreateButton("Issue33037CollectionViewButton", "CollectionView", () => new Issue33037NonShellCollectionViewPage()),
				CreateButton("Issue33037FixedHeaderCollectionViewButton", "Fixed header with CollectionView", () => new Issue33037NonShellFixedHeaderCollectionViewPage()),
				CreateButton("Issue33037ShortFixedHeaderCollectionViewButton", "Short fixed header with CollectionView", () => new Issue33037NonShellShortFixedHeaderCollectionViewPage()),
				CreateButton("Issue33037ProgrammaticCollectionViewButton", "Programmatic CollectionView scroll", () => new Issue33037NonShellProgrammaticCollectionViewPage()),
				CreateButton("Issue33037AppearingCollectionViewButton", "OnAppearing CollectionView scroll", () => new Issue33037NonShellAppearingCollectionViewPage()),
				CreateButton("Issue33037HiddenNavigationBarButton", "Hidden navigation bar", () => new Issue33037NonShellHiddenNavigationBarPage())
			}
		};
	}

	Button CreateButton(string automationId, string text, Func<ControlsPage> createPage)
	{
		var button = new Button
		{
			AutomationId = automationId,
			Text = text
		};

		button.Clicked += async (_, _) => await Navigation.PushAsync(createPage());
		return button;
	}
}

abstract class Issue33037NonShellScenarioPage : ContentPage
{
	protected Issue33037NonShellScenarioPage(string title)
	{
		Title = title;
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(
			this,
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode.Automatic);
	}

	protected static View CreateStackContent(string automationIdPrefix)
	{
		var stack = new VerticalStackLayout
		{
			Padding = 16,
			Spacing = 4
		};

		stack.Children.Add(new Label
		{
			AutomationId = $"{automationIdPrefix}Instructions",
			Text = "Scroll down to collapse the large navigation title.",
			FontAttributes = FontAttributes.Bold
		});

		for (int i = 0; i < 60; i++)
		{
			stack.Children.Add(new Label
			{
				AutomationId = $"{automationIdPrefix}Item{i}",
				Text = $"Item {i}"
			});
		}

		return stack;
	}

	protected static IList<string> CreateItems(int count = 60)
	{
		var items = new List<string>();
		for (int i = 0; i < count; i++)
		{
			items.Add($"Item {i}");
		}

		return items;
	}
}

class Issue33037NonShellScrollViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellScrollViewPage() : base("Issue33037 Direct")
	{
		Content = new ScrollView
		{
			AutomationId = "Issue33037ScrollViewScroller",
			Content = CreateStackContent("Issue33037Direct")
		};
	}
}

class Issue33037NonShellGridScrollViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellGridScrollViewPage() : base("Issue33037 Grid")
	{
		var grid = new Grid
		{
			Children =
			{
				new ScrollView
				{
					AutomationId = "Issue33037GridScrollViewScroller",
					Content = CreateStackContent("Issue33037Grid")
				},
				new ActivityIndicator
				{
					AutomationId = "Issue33037GridActivityIndicator",
					IsVisible = false
				}
			}
		};

		Content = grid;
	}
}

class Issue33037NonShellContentViewGridScrollViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellContentViewGridScrollViewPage() : base("Issue33037 Wrapped")
	{
		Content = new ContentView
		{
			Content = new Grid
			{
				Children =
				{
					new ScrollView
					{
						AutomationId = "Issue33037ContentViewGridScrollViewScroller",
						Content = CreateStackContent("Issue33037Wrapped")
					}
				}
			}
		};
	}
}

class Issue33037NonShellDynamicContentViewGridScrollViewPage : Issue33037NonShellScenarioPage
{
	readonly ContentView _contentView = new();
	bool _contentCreated;

	public Issue33037NonShellDynamicContentViewGridScrollViewPage() : base("Issue33037 Dynamic")
	{
		Content = _contentView;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (_contentCreated)
			return;

		_contentCreated = true;
		_contentView.Content = new Grid
		{
			Children =
			{
				new ScrollView
				{
					AutomationId = "Issue33037DynamicContentViewGridScrollViewScroller",
					Content = CreateStackContent("Issue33037Dynamic")
				}
			}
		};
	}
}

class Issue33037NonShellListViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellListViewPage() : base("Issue33037 List")
	{
#pragma warning disable CS0618 // ListView/ViewCell obsolete - intentionally covering issue #33037 legacy ListView behavior
		Content = new ListView
		{
			AutomationId = "Issue33037ListViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return new ViewCell { View = label };
			})
		};
#pragma warning restore CS0618
	}
}

class Issue33037NonShellCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellCollectionViewPage() : base("Issue33037 Collection")
	{
		Content = new CollectionView
		{
			AutomationId = "Issue33037CollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};
	}
}

class Issue33037NonShellFixedHeaderCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellFixedHeaderCollectionViewPage() : base("Issue33037 Fixed Header")
	{
		var header = new Label
		{
			AutomationId = "Issue33037FixedHeader",
			Text = "Fixed header must remain below the navigation bar",
			BackgroundColor = Colors.LightBlue,
			FontAttributes = FontAttributes.Bold,
			Padding = 12
		};

		var collectionView = new CollectionView
		{
			AutomationId = "Issue33037FixedHeaderCollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				header,
				collectionView
			}
		};
	}
}

class Issue33037NonShellShortFixedHeaderCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellShortFixedHeaderCollectionViewPage() : base("Issue33037 Short Header")
	{
		var header = new Label
		{
			AutomationId = "Issue33037ShortFixedHeader",
			Text = "Short content must still collapse the title",
			BackgroundColor = Colors.LightBlue,
			FontAttributes = FontAttributes.Bold,
			Padding = 12
		};

		var collectionView = new CollectionView
		{
			AutomationId = "Issue33037ShortFixedHeaderCollectionViewScroller",
			ItemsSource = CreateItems(17),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HeightRequest = 50,
					Padding = new Thickness(16, 0),
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				header,
				collectionView
			}
		};
	}
}

class Issue33037NonShellProgrammaticCollectionViewPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellProgrammaticCollectionViewPage() : base("Issue33037 Programmatic")
	{
		var collectionView = new CollectionView
		{
			AutomationId = "Issue33037ProgrammaticCollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		var scrollButton = new Button
		{
			AutomationId = "Issue33037ProgrammaticScrollButton",
			Text = "Scroll to item 50"
		};
		scrollButton.Clicked += (_, _) => collectionView.ScrollTo(50, position: ScrollToPosition.Start, animate: false);

		Grid.SetRow(collectionView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				scrollButton,
				collectionView
			}
		};
	}
}

class Issue33037NonShellAppearingCollectionViewPage : Issue33037NonShellScenarioPage
{
	readonly CollectionView _collectionView;
	bool _scrolled;

	public Issue33037NonShellAppearingCollectionViewPage() : base("Issue33037 Appearing")
	{
		_collectionView = new CollectionView
		{
			AutomationId = "Issue33037AppearingCollectionViewScroller",
			ItemsSource = CreateItems(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Padding = new Thickness(16, 12)
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		Content = new Grid
		{
			Children =
			{
				_collectionView
			}
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (_scrolled)
			return;

		_scrolled = true;
		await Task.Yield();
		_collectionView.ScrollTo(50, position: ScrollToPosition.Start, animate: false);
	}
}

class Issue33037NonShellHiddenNavigationBarPage : Issue33037NonShellScenarioPage
{
	public Issue33037NonShellHiddenNavigationBarPage() : base("Issue33037 Hidden Navigation")
	{
		NavigationPage.SetHasNavigationBar(this, false);

		var topMarker = new Label
		{
			AutomationId = "Issue33037HiddenNavigationBarTopMarker",
			Text = "This content must remain below the status bar.",
			BackgroundColor = Colors.LightGreen,
			FontAttributes = FontAttributes.Bold,
			Padding = 12
		};

		var backButton = new Button
		{
			AutomationId = "Issue33037HiddenNavigationBarBackButton",
			Text = "Back"
		};
		backButton.Clicked += async (_, _) => await Navigation.PopAsync();

		var scrollView = new ScrollView
		{
			AutomationId = "Issue33037HiddenNavigationBarScroller",
			Content = CreateStackContent("Issue33037HiddenNavigationBar")
		};

		Grid.SetRow(backButton, 1);
		Grid.SetRow(scrollView, 2);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				topMarker,
				backButton,
				scrollView
			}
		};
	}
}
