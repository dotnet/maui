namespace Maui.Controls.Sample;

public partial class VisualStateManagerCheckBoxPage : ContentPage
{
	public VisualStateManagerCheckBoxPage()
	{
		InitializeComponent();
	}

	void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!DemoCheckBox.IsEnabled)
		{
			CheckBoxState.Text = "State: Disabled";
			return;
		}

		var state = e.Value ? "Checked" : "Unchecked";
		VisualStateManager.GoToState(DemoCheckBox, state);
		CheckBoxState.Text = $"State: {state}";
	}

	void OnToggleCheckBoxDisabled(object sender, EventArgs e)
	{
		DemoCheckBox.IsEnabled = !DemoCheckBox.IsEnabled;
		if (!DemoCheckBox.IsEnabled)
		{
			VisualStateManager.GoToState(DemoCheckBox, "Disabled");
			CheckBoxState.Text = "State: Disabled";
		}
		else
		{
			var state = DemoCheckBox.IsChecked ? "Checked" : "Unchecked";
			VisualStateManager.GoToState(DemoCheckBox, state);
			CheckBoxState.Text = $"State: {state}";
		}
	}

	void OnResetCheckBox(object sender, EventArgs e)
	{
		DemoCheckBox.IsEnabled = true;
		DemoCheckBox.IsChecked = false;
		VisualStateManager.GoToState(DemoCheckBox, "Unchecked");
		CheckBoxState.Text = "State: Unchecked";
	}
}