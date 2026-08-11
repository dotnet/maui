namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27627, "[Windows] Border Automation Peer", PlatformAffected.UWP)]
public partial class Issue27627 : ContentPage
{
	public Issue27627()
	{
		var grid = new Grid();

		var border = new Border
		{
			AutomationId = "TestBorder",
			VerticalOptions = LayoutOptions.Center,
			HeightRequest = 100,
			Padding = new Thickness(10),
			Stroke = Colors.Red
		};
		SemanticProperties.SetDescription(border, "Outer test border");
		AutomationProperties.SetIsInAccessibleTree(border, true);

		var label = new Label
		{
			Text = "Welcome to Maui!",
			AutomationId = "TestLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var nestedBorder = new Border
		{
			Stroke = Colors.Blue,
			AutomationId = "NestedBorder",
			StrokeThickness = 2,
			Padding = new Thickness(10),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		SemanticProperties.SetDescription(nestedBorder, "Nested test border");
		AutomationProperties.SetIsInAccessibleTree(nestedBorder, true);

		nestedBorder.Content = label;
		border.Content = nestedBorder;

		grid.Children.Add(border);
		Content = grid;
	}
}
