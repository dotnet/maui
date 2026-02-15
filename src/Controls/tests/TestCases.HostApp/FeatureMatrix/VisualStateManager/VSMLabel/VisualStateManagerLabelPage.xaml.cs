namespace Maui.Controls.Sample;

public partial class VisualStateManagerLabelPage : ContentPage
{
	bool _labelSelected;
	bool _labelDisabled;

	public VisualStateManagerLabelPage()
	{
		InitializeComponent();
	}
	void OnToggleLabelSelected(object sender, EventArgs e)
	{
		if (_labelDisabled)
			return;
		_labelSelected = !_labelSelected;
		var state = _labelSelected ? "Selected" : "Normal";
		VisualStateManager.GoToState(SelectableLabelContainer, state);
		LabelState.Text = $"State: {state}";
	}

	void OnToggleLabelDisabled(object sender, EventArgs e)
	{
		_labelDisabled = !_labelDisabled;
		string state;
		LabelDisableButton.Text = _labelDisabled ? "Enable" : "Disable";
		if (_labelDisabled)
		{
			state = "Disabled";
			VisualStateManager.GoToState(SelectableLabelContainer, state);
		}
		else
		{
			state = _labelSelected ? "Selected" : "Normal";
			VisualStateManager.GoToState(SelectableLabelContainer, state);
		}
		LabelState.Text = $"State: {state}";
	}

	void OnResetLabel(object sender, EventArgs e)
	{
		_labelSelected = false;
		_labelDisabled = false;
		LabelDisableButton.Text = "Disable";
		VisualStateManager.GoToState(SelectableLabelContainer, "Normal");
		LabelState.Text = "State: Normal";
	}
}