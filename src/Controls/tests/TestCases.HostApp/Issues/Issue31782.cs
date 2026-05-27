namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31782, "Unexpected Line Breaks in Android, Label with WordWrap Mode Due to Trailing Space", PlatformAffected.Android)]
public class Issue31782 : ContentPage
{
	public Issue31782()
	{
		var layout = new VerticalStackLayout { Spacing = 20, Padding = 20 };

		var container = new Grid
		{
			AutomationId = "Container",
			WidthRequest = 300,
			HorizontalOptions = LayoutOptions.Center,
			BackgroundColor = Colors.LightBlue
		};

		var label = new Label
		{
			AutomationId = "TestLabel",
			Text = "How does .NET MAUI handle mauinavigaton?",
			LineBreakMode = LineBreakMode.WordWrap,
			TextColor = Color.FromArgb("#212121"),
			FontSize = 20,
			HorizontalOptions = LayoutOptions.End,
			BackgroundColor = Colors.LightGreen
		};

		container.Add(label);
		layout.Add(container);

		Content = layout;
	}
}
