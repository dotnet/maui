namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2597, "Stepper control .IsEnabled doesn't work", PlatformAffected.Android)]
public class Issue2597 : TestContentPage
{
	Label _label;

	protected override void Init()
	{
		Label header = new Label
		{
			Text = "Stepper",
			HorizontalOptions = LayoutOptions.Center
		};

#pragma warning disable CS0618 // Type or member is obsolete
		Stepper stepper = new Stepper
		{
			Minimum = 0,
			Maximum = 10,
			Increment = 0.1,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.CenterAndExpand,
			IsEnabled = false,
			AutomationId = "Stepper"
		};
#pragma warning restore CS0618 // Type or member is obsolete
		stepper.ValueChanged += OnStepperValueChanged;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		_label = new Label
		{
			AutomationId = "StepperValue",
			Text = "Stepper value is 0",
			FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.CenterAndExpand
		};
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		// Accomodate iPhone status bar.
		Padding = DeviceInfo.Platform == DevicePlatform.iOS ? new Thickness(10, 20, 10, 5) : new Thickness(10, 0, 10, 5);

		// Build the page.
		Content = new StackLayout
		{
			Children =
			{
				header,
				stepper,
				_label
			}
		};
	}

	void OnStepperValueChanged(object sender, ValueChangedEventArgs e)
	{
		_label.Text = string.Format("Stepper value is {0:F1}", e.NewValue);
	}
}
