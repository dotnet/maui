namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33769, "Stepper control fails to reach maximum value when increment exceeds remaining threshold", PlatformAffected.iOS)]
public class Issue33769 : ContentPage
{
	Label descriptionLabel;
	Label stepperStatusLabel;
	Stepper stepper;
	public Issue33769()
	{
		descriptionLabel = new Label
		{
			Text = "The test passes if the stepper is able to reach the maximum and minimum value when increment exceeds remaining threshold",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center
		};

		stepper = new Stepper
		{
			AutomationId = "Issue33769_Stepper",
			Minimum = 0,
			Increment = 3,
			Maximum = 2,
			Value = 1,
			HorizontalOptions = LayoutOptions.Center
		};

		stepperStatusLabel = new Label
		{
			AutomationId = "Issue33769_StepperStatusLabel",
			Text = "Failure",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center
		};

		stepper.ValueChanged += (sender, e) =>
		{
			stepperStatusLabel.Text = (stepper.Value == stepper.Maximum || stepper.Value == stepper.Minimum)
			? "Success" : "Failure";
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children =
			{
				descriptionLabel,
				stepper,
				stepperStatusLabel
			}
		};
	}
}
