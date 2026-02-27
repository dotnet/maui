namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27169, "Grid inside ScrollView should measure with infinite constraints", PlatformAffected.iOS)]
public class Issue27169 : ContentPage
{
	public Issue27169()
	{
		var grid = new Grid { VerticalOptions = LayoutOptions.Start };
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
		var firstContent = new Label { MinimumHeightRequest = 200, AutomationId = "StubLabel", BackgroundColor = Colors.LightBlue };
		firstContent.SetBinding(Label.TextProperty, new Binding(nameof(Label.Height), source: firstContent));
		var secondContent = new Label { MinimumHeightRequest = 40, Text = "Second Content", BackgroundColor = Colors.SlateBlue };

		grid.Add(firstContent);
		grid.Add(secondContent, 0, 1);

		var scrollView = new ScrollView { Content = grid };
		Content = scrollView;
	}
}