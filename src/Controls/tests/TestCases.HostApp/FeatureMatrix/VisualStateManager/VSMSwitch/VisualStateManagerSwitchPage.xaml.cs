namespace Maui.Controls.Sample;

public partial class VisualStateManagerSwitchPage : ContentPage
{
	public VisualStateManagerSwitchPage()
	{
		InitializeComponent();
	}

	void OnSwitchToggled(object sender, ToggledEventArgs e)
	{
		if (!DemoSwitch.IsEnabled)
		{
			SwitchState.Text = "State: Disabled";
			return;
		}

		var state = e.Value ? "On" : "Off";
		VisualStateManager.GoToState(DemoSwitch, state);
		SwitchState.Text = $"State: {state}";
	}

	void OnToggleSwitchDisabled(object sender, EventArgs e)
	{
		DemoSwitch.IsEnabled = !DemoSwitch.IsEnabled;
		if (!DemoSwitch.IsEnabled)
		{
			VisualStateManager.GoToState(DemoSwitch, "Disabled");
			SwitchState.Text = "State: Disabled";
		}
		else
		{
			var state = DemoSwitch.IsToggled ? "On" : "Off";
			VisualStateManager.GoToState(DemoSwitch, state);
			SwitchState.Text = $"State: {state}";
		}
	}

	void OnResetSwitch(object sender, EventArgs e)
	{
		DemoSwitch.IsEnabled = true;
		DemoSwitch.IsToggled = false;
		VisualStateManager.GoToState(DemoSwitch, "Off");
		SwitchState.Text = "State: Off";
	}
}