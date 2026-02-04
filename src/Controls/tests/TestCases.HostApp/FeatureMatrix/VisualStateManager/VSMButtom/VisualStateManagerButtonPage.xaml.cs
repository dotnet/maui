namespace Maui.Controls.Sample;

public partial class VisualStateManagerButtonPage : ContentPage
{
	public VisualStateManagerButtonPage()
	{
		InitializeComponent();
	}

	void OnDemoButtonPressed(object sender, EventArgs e)
	{
		VisualStateManager.GoToState(DemoButton, "Pressed");
		StateLabel.Text = "State: Pressed";
	}

	void OnDemoButtonReleased(object sender, EventArgs e)
	{
		VisualStateManager.GoToState(DemoButton, "Normal");
		StateLabel.Text = "State: Released";
	}

	void OnToggleButtonDisabled(object sender, EventArgs e)
	{
		DemoButton.IsEnabled = !DemoButton.IsEnabled;
	}

	void OnResetButtonState(object sender, EventArgs e)
	{
		DemoButton.IsEnabled = true;
		VisualStateManager.GoToState(DemoButton, "Normal");
		StateLabel.Text = "State: Normal";
	}

	async void OnDemoButtonClicked(object sender, EventArgs e)
	{
		if (!DemoButton.IsEnabled)
			return;
		StateLabel.Text = "State: Clicked";
		await DisplayAlert("VisualStateManager", "Button tapped", "OK");
	}

	void OnDemoButtonPointerEntered(object sender, PointerEventArgs e)
	{
		DemoButton.IsEnabled = true;
		if (DemoButton.IsEnabled)
			VisualStateManager.GoToState(DemoButton, "PointerOver");
		StateLabel.Text = "State: PointerOver Enter";
	}

	void OnDemoButtonPointerExited(object sender, PointerEventArgs e)
	{
		DemoButton.IsEnabled = true;
		if (DemoButton.IsEnabled)
			VisualStateManager.GoToState(DemoButton, "Normal");
		StateLabel.Text = "State: PointerOver Exit";
	}

	void OnButtonFocused(object sender, FocusEventArgs e)
	{
		VisualStateManager.GoToState(DemoButton, "Focused");
		StateLabel.Text = "State: Focused";
	}

	void OnButtonUnfocused(object sender, FocusEventArgs e)
	{
		VisualStateManager.GoToState(DemoButton, "Unfocused");
		StateLabel.Text = "State: Unfocused";
	}
}