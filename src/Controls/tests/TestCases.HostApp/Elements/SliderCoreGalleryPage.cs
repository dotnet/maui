namespace Maui.Controls.Sample;

internal class SliderCoreGalleryPage : ContentPage
{
	public SliderCoreGalleryPage()
	{
		var layout = new StackLayout
		{
			Padding = new Microsoft.Maui.Thickness(12)
		};

		// Default
		var defaultLabel = new Label { AutomationId = "DefaultSlider", Text = "Default" };
		var defaultSlider = new Slider();
		layout.Add(defaultLabel);
		layout.Add(defaultSlider);

		// BackgroundColor
		var backgroundColorLabel = new Label { Text = "BackgroundColor" };
		var backgroundColorSlider = new Slider { AutomationId = "BackgroundColorSlider", BackgroundColor = Colors.Red };
		layout.Add(backgroundColorLabel);
		layout.Add(backgroundColorSlider);

		// MaximumTrackColor
		var maximumTrackColorLabel = new Label { Text = "MaximumTrackColor" };
		var maximumTrackColorSlider = new Slider { AutomationId = "MaximumTrackColorSlider", MinimumTrackColor = Colors.Orange };
		layout.Add(maximumTrackColorLabel);
		layout.Add(maximumTrackColorSlider);

		// MinimumTrackColor
		var minimumTrackColorLabel = new Label { Text = "MinimumTrackColor" };
		var minimumTrackColorSlider = new Slider { AutomationId = "MinimumTrackColorSlider", MaximumTrackColor = Colors.Blue };
		layout.Add(minimumTrackColorLabel);
		layout.Add(minimumTrackColorSlider);

		// ThumbColor
		var thumbColorLabel = new Label { Text = "ThumbColor" };
		var thumbColorSlider = new Slider { AutomationId = "ThumbColorSlider", ThumbColor = Colors.Green };
		layout.Add(thumbColorLabel);
		layout.Add(thumbColorSlider);

		// Custom Slider
		var customLabel = new Label { Text = "Custom" };
		var customSlider = new Slider { AutomationId = "CustomSlider", Minimum = 0, Maximum = 100, Value = 30 };
		layout.Add(customLabel);
		layout.Add(customSlider);

		Content = layout;
	}
}