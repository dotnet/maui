namespace Maui.Controls.Sample;

public partial class VisualStateManagerEntryPage : ContentPage
{
	public VisualStateManagerEntryPage()
	{
		InitializeComponent();
	}

	void OnFocusEntry(object sender, EventArgs e)
	{
		if (!DemoEntry.IsEnabled)
		{
			return;
		}
		DemoEntry.Focus();
	}

	void OnToggleEntryDisabled(object sender, EventArgs e)
	{
		DemoEntry.IsEnabled = !DemoEntry.IsEnabled;
		EntryDisableButton?.Text = DemoEntry.IsEnabled ? "Disable" : "Enable";
		EntryState.Text = DemoEntry.IsEnabled
			? (DemoEntry.IsFocused ? "State: Focused" : "State: Unfocused")
			: "State: Disabled";
	}

	void OnResetEntry(object sender, EventArgs e)
	{
		DemoEntry.IsEnabled = true;
		DemoEntry.Text = string.Empty;
		EntryDisableButton?.Text = "Disable";
		EntryState.Text = "State: Unfocused";
	}

	void OnEntryFocused(object sender, FocusEventArgs e)
	{
		EntryState.Text = "State: Focused";
	}
	void OnEntryUnfocused(object sender, FocusEventArgs e)
	{
		EntryState.Text = "State: Unfocused";
	}
}