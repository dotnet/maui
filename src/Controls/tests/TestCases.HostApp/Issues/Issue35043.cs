namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35043, "Landscape orientation with full screen no longer rotates", PlatformAffected.iOS)]
public class Issue35043 : ContentPage
{
	readonly Label _orientationResult;

	public Issue35043()
	{
		_orientationResult = new Label
		{
			Text = "Not evaluated",
			AutomationId = "OrientationResult",
			HorizontalTextAlignment = TextAlignment.Center
		};

		Content = new Grid
		{
			Padding = 24,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			},
			Children =
			{
				new Border
				{
					AutomationId = "LandscapeContent",
					BackgroundColor = Colors.Blue,
					Content = new Label
					{
						Text = "Landscape-only full-screen content",
						TextColor = Colors.White,
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center
					}
				}.Row(0),
				new Button
				{
					Text = "Evaluate orientation",
					AutomationId = "EvaluateOrientation",
					Command = new Command(EvaluateOrientation)
				}.Row(1),
				_orientationResult.Row(2)
			}
		};
	}

	void EvaluateOrientation()
	{
		_orientationResult.Text = Width > Height ? "Landscape" : "Portrait";
	}
}
