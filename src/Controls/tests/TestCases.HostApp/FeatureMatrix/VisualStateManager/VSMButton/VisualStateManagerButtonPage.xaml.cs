namespace Maui.Controls.Sample;

public partial class VisualStateManagerButtonPage : ContentPage
{
	public VisualStateManagerButtonPage()
	{
		InitializeComponent();
	}

	void OnDemoButtonPressed(object sender, EventArgs e)
	{
		if (!DemoButton.IsEnabled)
			return;
		VisualStateManager.GoToState(DemoButton, "Pressed");
		StateLabel.Text = "State: Pressed";
		DemoButton.Text = "Pressed";
	}

	void OnDemoButtonReleased(object sender, EventArgs e)
	{
		if (!DemoButton.IsEnabled)
			return;
		VisualStateManager.GoToState(DemoButton, "Normal");
		StateLabel.Text = "State: Normal/Released";
		DemoButton.Text = "Normal/Released";
	}

	void OnToggleButtonDisabled(object sender, EventArgs e)
	{
		DemoButton.IsEnabled = !DemoButton.IsEnabled;
		ButtonDisableButton.Text = DemoButton.IsEnabled ? "Disable" : "Enable";
		if (!DemoButton.IsEnabled)
		{
			VisualStateManager.GoToState(DemoButton, "Disabled");
			StateLabel.Text = "State: Disabled";
			DemoButton.Text = "Disabled";
		}
		else
		{
			VisualStateManager.GoToState(DemoButton, "Normal");
			StateLabel.Text = "State: Normal";
			DemoButton.Text = "Normal";
		}
	}

	void OnResetButtonState(object sender, EventArgs e)
	{
		DemoButton.IsEnabled = true;
		VisualStateManager.GoToState(DemoButton, "Normal");
		ButtonDisableButton.Text = "Disable";
		StateLabel.Text = "State: Normal";
		DemoButton.Text = "Press Me";
	}

	void OnDemoButtonPointerEntered(object sender, PointerEventArgs e)
	{if (!DemoButton.IsEnabled)
			return;
		if (DemoButton.IsEnabled)
			VisualStateManager.GoToState(DemoButton, "PointerOver");
		StateLabel.Text = "State: PointerOver Enter";
		DemoButton.Text = "PointerOver Enter";
	}

	void OnDemoButtonPointerExited(object sender, PointerEventArgs e)
	{
		if (!DemoButton.IsEnabled)
			return;
		if (DemoButton.IsEnabled)
			VisualStateManager.GoToState(DemoButton, "Normal");
		StateLabel.Text = "State: PointerOver Exit";
		DemoButton.Text = "PointerOver Exit";
	}

	void OnButtonFocused(object sender, FocusEventArgs e)
	{
		if (!DemoButton.IsEnabled)
			return;
		DemoButton.Focus();
		VisualStateManager.GoToState(DemoButton, "Focused");
		StateLabel.Text = "State: Focused";
		DemoButton.Text = "Focused";
	}
}