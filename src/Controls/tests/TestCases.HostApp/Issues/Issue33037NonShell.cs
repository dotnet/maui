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
				CreateButton("Issue33037ListViewButton", "ListView", () => new Issue33037NonShellListViewPage()),
				CreateButton("Issue33037CollectionViewButton", "CollectionView", () => new Issue33037NonShellCollectionViewPage())
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

	protected static IList<string> CreateItems()
	{
		var items = new List<string>();
		for (int i = 0; i < 60; i++)
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
