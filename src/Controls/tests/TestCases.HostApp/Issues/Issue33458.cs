namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33458, "SafeArea incorrectly applied when using ControlTemplate with ContentPresenter", PlatformAffected.iOS)]
public class Issue33458 : TestContentPage
{
	protected override void Init()
	{
		AutomationId = "TestPage";
		SafeAreaEdges = SafeAreaEdges.None;
		ControlTemplate = new ControlTemplate(() => new ContentPresenter());

		var stackLayout = new VerticalStackLayout
		{
			AutomationId = "TestStackLayout",
			Padding = new Thickness(30, 0),
			Spacing = 25,
			BackgroundColor = Colors.Green,
			SafeAreaEdges = SafeAreaEdges.None,
			Children =
			{
				new Image
				{
					AutomationId = "BotImage",
					Source = "dotnet_bot.png",
					HeightRequest = 185,
					Aspect = Aspect.AspectFit
				},
				new Label { AutomationId = "HelloLabel", Text = "Hello, World!" },
				new Label { AutomationId = "WelcomeLabel", Text = "Welcome to\n.NET Multi-platform App UI" },
				new Button { AutomationId = "CounterBtn", Text = "Click me", HorizontalOptions = LayoutOptions.Fill }
			}
		};

		Content = new ScrollView
		{
			AutomationId = "TestScrollView",
			BackgroundColor = Colors.Red,
			SafeAreaEdges = SafeAreaEdges.None,
			Content = stackLayout
		};
	}
}
