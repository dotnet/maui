namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29740, "[Windows] Stepper control fails to reach maximum value when increment exceeds remaining threshold",
	PlatformAffected.UWP)]
public class Issue29740 : ContentPage
{
	Label stepperValueLabel;
	Stepper myStepper;

	public Issue29740()
	{
		// Initialize the label
		stepperValueLabel = new Label
		{
			AutomationId = "29740StepperValueLabel",
			Text = "Stepper Value: 0",
			FontSize = 18,
			HorizontalOptions = LayoutOptions.Center
		};

		// Initialize the stepper
		myStepper = new Stepper
		{
			AutomationId = "29740Stepper",
			Minimum = 0,
			Maximum = 10,
			Increment = 3,
			HorizontalOptions = LayoutOptions.Center
		};
		myStepper.ValueChanged += OnStepperValueChanged;

		// Create the layout
		var verticalStack = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			Children =
				{
					stepperValueLabel,
					myStepper
				}
		};

		// Embed in a ScrollView
		Content = new ScrollView
		{
			Content = verticalStack
		};

		// Optionally update initial value display
		UpdateLabel(myStepper.Value);
	}

	private void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
	{
		UpdateLabel(e.NewValue);
	}

	private void UpdateLabel(double value)
	{
		stepperValueLabel.Text = $"Stepper Value: {value}";
	}
}

