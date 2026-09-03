namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33034, "SafeAreaEdges works correctly only on the first tab in Shell. Other tabs have content colliding with the display cutout in the landscape mode.", PlatformAffected.Android)]
public class Issue33034 : TestShell
{
	protected override void Init()
	{
		var tabBar = new TabBar();
		var tab = new Tab { Title = "Tabs" };

		tab.Items.Add(new ShellContent
		{
			Title = "First Tab",
			AutomationId = "FirstTab",
			ContentTemplate = new DataTemplate(typeof(Issue33034TabContent)),
			Route = "tab1"
		});

		tab.Items.Add(new ShellContent
		{
			Title = "Second Tab",
			AutomationId = "SecondTab",
			ContentTemplate = new DataTemplate(typeof(Issue33034TabContent)),
			Route = "tab2"
		});

		tabBar.Items.Add(tab);
		Items.Add(tabBar);
	}
}

public class Issue33034TabContent : ContentPage
{
	public Issue33034TabContent()
	{
		// Full-width label to detect safe area padding on either side
		var edgeLabel = new Label
		{
			Text = "EDGE LABEL",
			AutomationId = "EdgeLabel",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			BackgroundColor = Colors.Red,
			TextColor = Colors.White,
			HorizontalOptions = LayoutOptions.Fill,
			HorizontalTextAlignment = TextAlignment.Center
		};

		Content = new VerticalStackLayout
		{
			Children = { edgeLabel }
		};
	}
}

