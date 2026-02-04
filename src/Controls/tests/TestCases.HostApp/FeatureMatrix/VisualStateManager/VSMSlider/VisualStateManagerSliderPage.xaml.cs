namespace Maui.Controls.Sample;

public partial class VisualStateManagerSliderPage : ContentPage
{
	public VisualStateManagerSliderPage()
	{
		InitializeComponent();
	}

	void OnFocusSlider(object sender, EventArgs e)
	{
		if (!DemoSlider.IsEnabled)
			return;

		DemoSlider.Focus();
	}

	void OnToggleSliderDisabled(object sender, EventArgs e)
	{
		DemoSlider.IsEnabled = !DemoSlider.IsEnabled;
		if (!DemoSlider.IsEnabled)
		{
			SliderState.Text = $"State: Disabled | Value: {DemoSlider.Value:0}";
			return;
		}

		SliderState.Text = DemoSlider.IsFocused
			? $"State: Focused | Value: {DemoSlider.Value:0}"
			: $"State: Unfocused | Value: {DemoSlider.Value:0}";
	}

	void OnResetSlider(object sender, EventArgs e)
	{
		DemoSlider.IsEnabled = true;
		DemoSlider.Unfocus();
		VisualStateManager.GoToState(DemoSlider, "Unfocused");
		SliderState.Text = $"State: Unfocused | Value: {DemoSlider.Value:0}";
	}

	void OnSliderFocused(object sender, FocusEventArgs e)
	{
		SliderState.Text = $"State: Focused | Value: {DemoSlider.Value:0}";
	}

	void OnSliderUnfocused(object sender, FocusEventArgs e)
	{
		SliderState.Text = $"State: Unfocused | Value: {DemoSlider.Value:0}";
	}

	void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
	{
		if (!DemoSlider.IsEnabled)
		{
			SliderState.Text = $"State: Disabled | Value: {e.NewValue:0}";
			return;
		}

		SliderState.Text = DemoSlider.IsFocused
			? $"State: Focused | Value: {e.NewValue:0}"
			: $"State: Unfocused | Value: {e.NewValue:0}";
	}
}