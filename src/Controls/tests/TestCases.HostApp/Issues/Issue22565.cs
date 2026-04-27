namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 22565, "A disabled Picker prevents the parent container's GestureRecognizer from being triggered", PlatformAffected.Android)]
public class Issue22565 : ContentPage
{
	Label descriptionLabel;
	public Issue22565()
	{
		VerticalStackLayout verticalStackLayout = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25
		};

		Grid grid = new Grid
		{
			Padding = 20,
			BackgroundColor = Colors.DarkRed
		};

		Picker picker = new Picker
		{
			AutomationId = "DisabledPicker",
			BackgroundColor = Colors.Gray,
			IsEnabled = false
		};

		TapGestureRecognizer tapGesture = new TapGestureRecognizer();
		tapGesture.Tapped += OnCounterClicked;

		grid.GestureRecognizers.Add(tapGesture);
		grid.Children.Add(picker);

		descriptionLabel = new Label
		{
			AutomationId = "22565DescriptionLabel",
			Text = "The test passes if the disabled Picker does not intercept the parent container's GestureRecognizer; otherwise, it fails.",
		};

		verticalStackLayout.Children.Add(grid);
		verticalStackLayout.Children.Add(descriptionLabel);

		Content = verticalStackLayout;
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		descriptionLabel.Text = "Parent Gesture recognizer triggered";
	}
}