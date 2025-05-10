namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28949, "On iOS GestureRecognizers don't work on Span in a Label, which doesn't get IsVisible (=true) update from its parent", PlatformAffected.iOS)]
public class Issue28949 : ContentPage
{
	public Issue28949()
	{
		var toggleButton = new Button
		{
			Text = "Toggle Visibility",
			AutomationId = "ToggleButton"
		};
		var formattedString = new FormattedString();
		var label = new Label
		{
			FormattedText = formattedString,
			BackgroundColor = Colors.Transparent,
			AutomationId = "Label"
		};
		var grid = new Grid
		{
			BackgroundColor = Colors.Yellow
		};

		var span = new Span
		{
			Text = "Click me",
			TextColor = Colors.Pink
		};
		var tapGesture = new TapGestureRecognizer();
		tapGesture.Tapped += (s, e) =>
		{
			span.Text = "Success";
			span.TextColor = Colors.Green;
		};
		span.GestureRecognizers.Add(tapGesture);
		formattedString.Spans.Add(span);

		toggleButton.Clicked += (s, e) =>
		{
			grid.IsVisible = !grid.IsVisible;
		};
		grid.IsVisible = false;
		grid.Children.Add(label);
		var stackLayout = new StackLayout
		{
			Children =
			{
				toggleButton,
				grid
			}
		};
		Content = stackLayout;
	}

}