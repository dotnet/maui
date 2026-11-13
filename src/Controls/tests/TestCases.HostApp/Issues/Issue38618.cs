namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38618, "[MAUI] L2 TabBar Switching Test - Crashes when opening L2", PlatformAffected.UWP)]
public class Issue38618 : TestShell
{
	protected override void Init()
	{
		var collectionView = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			ItemsSource = new[]
			{
				"L2_TabBarSwitching"
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 18,
					Padding = new Thickness(10)
				};

				label.SetBinding(Label.TextProperty, ".");

				return label;
			}),
			AutomationId = "IssueCollectionView"
		};

		collectionView.SelectionChanged += (sender, args) =>
		{
			if (args.CurrentSelection.FirstOrDefault() is string selectedItem &&
			selectedItem == "L2_TabBarSwitching")
			{
				var page = CreateL2TabBarSwitchingPage();

				if (page is Shell)
				{
					this.Window.Page = page;
				}
			}

			if (sender is CollectionView cv)
			{
				cv.SelectedItem = null;
			}
		};

		var categoryPage = new ContentPage
		{
			Title = "BugFixes",
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						Text = "BugFixes",
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center
					},
					collectionView
				}
			}
		};

		Items.Add(new ShellContent
		{
			Title = "BugFixes",
			Content = categoryPage
		});
	}

	Shell CreateL2TabBarSwitchingPage()
	{
		var shell = new Shell
		{
			Title = "L2_TabBarSwitching"
		};

		var tabBar = new TabBar();

		tabBar.Items.Add(new Tab
		{
			Title = "Tab1",
			Items =
			{
				new ShellContent
				{
					Title = "Tab1",
					Content = CreateTabPage("Tab1")
				}
			}
		});

		tabBar.Items.Add(new Tab
		{
			Title = "Tab2",
			Items =
			{
				new ShellContent
				{
					Title = "Tab2",
					Content = CreateTabPage("Tab2")
				}
			}
		});

		shell.Items.Add(tabBar);

		return shell;
	}

	ContentPage CreateTabPage(string tabName)
	{
		return new ContentPage
		{
			Title = tabName,
			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = "1. This test is to check if TabBar works well."
					},
					new Label
					{
						Text = "2. Click 'Tab2' to switch to Tab2 and then switch back to Tab1."
					},
					new Label
					{
					Text = "3. Click button 'Back to BugFixes Category' to go back to BugFixes Category page."
					}
				}
			}
		};
	}
}
