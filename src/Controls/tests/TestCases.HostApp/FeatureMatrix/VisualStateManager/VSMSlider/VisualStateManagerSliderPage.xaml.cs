namespace Maui.Controls.Sample;

public partial class VisualStateManagerSliderPage : ContentPage
{
	public VisualStateManagerSliderPage()
	{
		InitializeComponent();
	}

	void OnFocusSlider(object sender, EventArgs e)
	{
		if (!VSMSlider.IsEnabled)
			return;

		VSMSlider.Focus();
	}

	void OnToggleSliderDisabled(object sender, EventArgs e)
	{
		VSMSlider.IsEnabled = !VSMSlider.IsEnabled;
		SliderDisableButton.Text = VSMSlider.IsEnabled ? "Disable" : "Enable";
		if (!VSMSlider.IsEnabled)
		{
			SliderState.Text = $"State: Disabled | Value: {VSMSlider.Value:0}";
			return;
		}

		SliderState.Text = VSMSlider.IsFocused
			? $"State: Focused | Value: {VSMSlider.Value:0}"
			: $"State: Unfocused | Value: {VSMSlider.Value:0}";
	}

	void OnResetSlider(object sender, EventArgs e)
	{
		VSMSlider.IsEnabled = true;
		VSMSlider.Unfocus();
		SliderDisableButton.Text = "Disable";
		VisualStateManager.GoToState(VSMSlider, "Unfocused");
		SliderState.Text = $"State: Unfocused | Value: {VSMSlider.Value:0}";
	}

	void OnSliderFocused(object sender, FocusEventArgs e)
	{
		SliderState.Text = $"State: Focused | Value: {VSMSlider.Value:0}";
	}

	void OnSliderUnfocused(object sender, FocusEventArgs e)
	{
		SliderState.Text = $"State: Unfocused | Value: {VSMSlider.Value:0}";
	}

	void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
	{
		if (!VSMSlider.IsEnabled)
		{
			SliderState.Text = $"State: Disabled | Value: {e.NewValue:0}";
			return;
		}

		SliderState.Text = VSMSlider.IsFocused
			? $"State: Focused | Value: {e.NewValue:0}"
			: $"State: Unfocused | Value: {e.NewValue:0}";
	}
}