namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36269, "SafeAreaEdges bottom padding is lost when the Shell TabBar is hidden at runtime on a page that started above the navigation bar", PlatformAffected.Android)]
public class Issue36269 : TestShell
{
	protected override void Init()
	{
		var tabBar = new TabBar();

		tabBar.Items.Add(new ShellContent
		{
			Title = "Page1",
			AutomationId = "FirstTab",
			ContentTemplate = new DataTemplate(typeof(Issue36269FirstTab)),
			Route = "page1"
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Page2",
			AutomationId = "SecondTab",
			ContentTemplate = new DataTemplate(typeof(ContentPage)),
			Route = "page2"
		});

		Items.Add(tabBar);
	}
}

public class Issue36269FirstTab : ContentPage
{
	public Issue36269FirstTab()
	{
		BackgroundColor = Colors.Black;

		var scrollContent = new VerticalStackLayout
		{
			Padding = new Thickness(16, 16, 16, 48),
			Spacing = 16,
			Children =
			{
				new Label { Text = "Page 1", AutomationId = "PageTitleLabel", TextColor = Colors.White, FontSize = 24 },
				new Label { Text = "TabBar starts visible; this page is above the nav bar.", TextColor = Colors.White },
			}
		};

		var scrollView = new ScrollView { Content = scrollContent };

		var hideTabBarButton = new Button
		{
			Text = "Hide TabBar",
			AutomationId = "HideTabBarButton",
			Margin = new Thickness(0, 8, 0, 0)
		};
		hideTabBarButton.Clicked += (s, e) => Shell.SetTabBarIsVisible(this, false);

		var bottomBar = new VerticalStackLayout
		{
			BackgroundColor = Colors.MediumPurple,
			Padding = new Thickness(24, 16, 24, 24),
			Children =
			{
				new BoxView { HeightRequest = 2, Color = Colors.Gray },
				new Label
				{
					Text = "BOTTOM EDGE",
					AutomationId = "BottomEdgeLabel",
					TextColor = Colors.White,
					FontAttributes = FontAttributes.Bold,
					HorizontalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(0, 8, 0, 0)
				},
				hideTabBarButton
			}
		};

		var grid = new Grid
		{
			SafeAreaEdges = SafeAreaEdges.All,
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		grid.Add(scrollView, 0, 0);
		grid.Add(bottomBar, 0, 1);

		Content = grid;
	}
}
