namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34472, "Top SafeAreaEdge not respected when page pushed to NavigationPage", PlatformAffected.Android)]
public class Issue34472 : NavigationPage
{
	public Issue34472() : base(new Issue34472_Launcher())
	{
	}
}

public class Issue34472_Launcher : ContentPage
{
	public Issue34472_Launcher()
	{
		Title = "Issue 34472";

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Spacing = 20,
			Padding = 30,
			Children =
			{
				new Label
				{
					Text = "Tap a button to navigate to test page with SafeAreaEdges=None",
					HorizontalTextAlignment = TextAlignment.Center,
					AutomationId = "InstructionLabel"
				},
				new Button
				{
					Text = "Push to Stack",
					AutomationId = "PushToStackButton",
					Command = new Command(async () =>
					{
						await Navigation.PushAsync(new Issue34472_TestPage());
					})
				},
				new Button
				{
					Text = "Set as Main Page",
					AutomationId = "SetMainPageButton",
					Command = new Command(() =>
					{
						Application.Current.Windows[0].Page = new Issue34472_TestPage();
					})
				}
			}
		};
	}
}

public class Issue34472_TestPage : ContentPage
{
	public Issue34472_TestPage()
	{
		SafeAreaEdges = SafeAreaEdges.None;
		NavigationPage.SetHasNavigationBar(this, false);
		NavigationPage.SetHasBackButton(this, false);

		Content = new Border
		{
			Stroke = Colors.Red,
			StrokeThickness = 5,
			Margin = new Thickness(2),
			AutomationId = "ContentBorder",
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = new Thickness(10),
				Children =
				{
					new Label
					{
						Text = "SafeAreaEdges: None",
						FontSize = 16,
						AutomationId = "SafeAreaEdgesLabel"
					},
					new Label
					{
						Text = "NavigationPage.HasNavigationBar: False",
						FontSize = 16,
						AutomationId = "NavBarLabel"
					},
					new Label
					{
						Text = "Content should start at same Y regardless of navigation method",
						FontSize = 14,
						TextColor = Colors.Gray,
						AutomationId = "DescriptionLabel"
					},
					new Button
					{
						Text = "Back",
						AutomationId = "BackButton",
						Command = new Command(() =>
						{
							Application.Current.Windows[0].Page = new Issue34472();
						})
					}
				}
			}
		};
	}
}
