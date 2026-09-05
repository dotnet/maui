namespace Maui.Controls.Sample;

public partial class VisualStateManagerCheckBoxPage : ContentPage
{
	public VisualStateManagerCheckBoxPage()
	{
		InitializeComponent();
		VisualStateManager.GoToState(VSMCheckBox, "Normal");
		CheckBoxState.Text = "State: Normal";
	}

	void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!VSMCheckBox.IsEnabled)
		{
			CheckBoxState.Text = "State: Disabled";
			return;
		}

		var state = e.Value ? "Checked" : "Unchecked";
		VisualStateManager.GoToState(VSMCheckBox, state);
		CheckBoxState.Text = $"State: {state}";
	}

	void OnToggleCheckBoxDisabled(object sender, EventArgs e)
	{
		VSMCheckBox.IsEnabled = !VSMCheckBox.IsEnabled;
		CheckBoxDisableButton.Text = VSMCheckBox.IsEnabled ? "Disable" : "Enable";
		if (!VSMCheckBox.IsEnabled)
		{
			VisualStateManager.GoToState(VSMCheckBox, "Disabled");
			CheckBoxState.Text = "State: Disabled";
		}
		else
		{
			var state = VSMCheckBox.IsChecked ? "Checked" : "Unchecked";
			VisualStateManager.GoToState(VSMCheckBox, state);
			CheckBoxState.Text = $"State: {state}";
		}
	}

	void OnResetCheckBox(object sender, EventArgs e)
	{
		VSMCheckBox.IsEnabled = true;
		VSMCheckBox.IsChecked = false;
		CheckBoxDisableButton.Text = "Disable";
		VisualStateManager.GoToState(VSMCheckBox, "Normal");
		CheckBoxState.Text = "State: Normal";
	}
}

