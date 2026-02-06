namespace Maui.Controls.Sample;

public partial class VisualStateManagerSwitchPage : ContentPage
{
	public VisualStateManagerSwitchPage()
	{
		InitializeComponent();
	}

	void OnSwitchToggled(object sender, ToggledEventArgs e)
	{
		if (!VSMSwitch.IsEnabled)
		{
			SwitchState.Text = "State: Disabled";
			return;
		}

		var state = e.Value ? "On" : "Off";
		VisualStateManager.GoToState(VSMSwitch, state);
		SwitchState.Text = $"State: {state}";
	}

	void OnToggleSwitchDisabled(object sender, EventArgs e)
	{
		VSMSwitch.IsEnabled = !VSMSwitch.IsEnabled;
		SwitchDisableButton.Text = VSMSwitch.IsEnabled ? "Disable" : "Enable";
		if (!VSMSwitch.IsEnabled)
		{
			VisualStateManager.GoToState(VSMSwitch, "Disabled");
			SwitchState.Text = "State: Disabled";
		}
		else
		{
			var state = VSMSwitch.IsToggled ? "On" : "Off";
			VisualStateManager.GoToState(VSMSwitch, state);
			SwitchState.Text = $"State: {state}";
		}
	}

	void OnResetSwitch(object sender, EventArgs e)
	{
		VSMSwitch.IsEnabled = true;
		VSMSwitch.IsToggled = false;
		SwitchDisableButton.Text = "Disable";
		VisualStateManager.GoToState(VSMSwitch, "Off");
		SwitchState.Text = "State: Off";
	}
}