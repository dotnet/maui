namespace Maui.Controls.Sample;

public partial class VisualStateManagerSliderPage : ContentPage
{
	bool _isResetting = false;

	public VisualStateManagerSliderPage()
	{
		InitializeComponent();
		VisualStateManager.GoToState(VSMSlider, "Normal");
	}

	void OnFocusSlider(object sender, EventArgs e)
	{
		if (!VSMSlider.IsEnabled)
			return;
		if (!VSMSlider.IsFocused)
		{
			VSMSlider.Focus();
			VisualStateManager.GoToState(VSMSlider, "Focused");
			VSMSlider.Value = 65;
			SliderState.Text = $"State: Focused | Value: {VSMSlider.Value:0}";
		}
	}

	void OnUnfocusSlider(object sender, EventArgs e)
	{
		if (!VSMSlider.IsEnabled)
			return;

		VSMSlider.Unfocus();
		VisualStateManager.GoToState(VSMSlider, "Normal");
		SliderState.Text = $"State: Normal/Unfocused | Value: {VSMSlider.Value:0}";
	}

	void OnToggleSliderDisabled(object sender, EventArgs e)
	{
		VSMSlider.IsEnabled = !VSMSlider.IsEnabled;
		SliderDisableButton.Text = VSMSlider.IsEnabled ? "Disable" : "Enable";
		if (!VSMSlider.IsEnabled)
		{
			VisualStateManager.GoToState(VSMSlider, "Disabled");
			SliderState.Text = $"State: Disabled | Value: {VSMSlider.Value:0}";
		}
		else
		{
			VisualStateManager.GoToState(VSMSlider, "Normal");
			SliderState.Text = $"State: Normal | Value: {VSMSlider.Value:0}";
		}
	}

	void OnResetSlider(object sender, EventArgs e)
	{
		_isResetting = true;
		VSMSlider.IsEnabled = true;
		SliderDisableButton.Text = "Disable";
		VSMSlider.Value = 50;
		VisualStateManager.GoToState(VSMSlider, "Normal");
		SliderState.Text = $"State: Normal | Value: {VSMSlider.Value:0}";
		_isResetting = false;
	}

	void OnSliderFocused(object sender, FocusEventArgs e)
	{
		VSMSlider.Focus();
		VisualStateManager.GoToState(VSMSlider, "Focused");
		SliderState.Text = $"State: Focused | Value: {VSMSlider.Value:0}";
	}

	void OnSliderUnfocused(object sender, FocusEventArgs e)
	{
		VisualStateManager.GoToState(VSMSlider, "Normal");
		SliderState.Text = $"State: Normal/Unfocused | Value: {VSMSlider.Value:0}";
	}

	void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
	{
		if (_isResetting)
		{
			VisualStateManager.GoToState(VSMSlider, "Normal");
			SliderState.Text = $"State: Normal | Value: {e.NewValue:0}";
			return;
		}
		if (!VSMSlider.IsEnabled)
		{
			VisualStateManager.GoToState(VSMSlider, "Disabled");
			SliderState.Text = $"State: Disabled | Value: {e.NewValue:0}";
			return;
		}
		VisualStateManager.GoToState(VSMSlider, "Focused");
		SliderState.Text = $"State: Focused | Value: {e.NewValue:0}";
	}
}

