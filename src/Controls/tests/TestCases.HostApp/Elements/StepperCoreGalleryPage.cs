namespace Maui.Controls.Sample;

internal class StepperCoreGalleryPage : ContentPage
{
	public StepperCoreGalleryPage()
	{
		var layout = new StackLayout
		{
			Padding = new Thickness(12)
		};

		// Default
		var defaultLabel = new Label { AutomationId = "DefaultLabel", Text = "Default" };
		var defaultStepper = new Stepper { AutomationId = "DefaultStepper" };
		var defaultLabelValue = new Label
		{
			AutomationId = "DefaultLabelValue",
			Text = defaultStepper.Value.ToString()
		};
		layout.Add(defaultLabel);
		layout.Add(defaultStepper);
		layout.Add(defaultLabelValue);

		defaultStepper.ValueChanged += (s, e) =>
		{
			defaultLabelValue.Text = e.NewValue.ToString();
		};

		Content = layout;
	}
}