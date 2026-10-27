namespace Maui.Controls.Sample;

internal class StepperCoreGalleryPage : ContentPage
{
	public StepperCoreGalleryPage()
	{
		var layout = new StackLayout
		{
			FlowDirection = FlowDirection.LeftToRight,
			Padding = new Thickness(12)
		};

		// Default
		var defaultLabel = new Label { AutomationId = "DefaultLabel", Text = "Default" };
		var defaultStepper = new Stepper { AutomationId = "DefaultStepper" };
		if (OperatingSystem.IsIOSVersionAtLeast(26))
		{
			defaultStepper.HorizontalOptions = LayoutOptions.Center;
		}
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

		var useInheritedRtl = new Button
		{
			AutomationId = "UseInheritedRTL",
			Text = "Use inherited right-to-left flow"
		};
		useInheritedRtl.Clicked += (s, e) => layout.FlowDirection = FlowDirection.RightToLeft;
		layout.Add(useInheritedRtl);

		Content = layout;
	}
}