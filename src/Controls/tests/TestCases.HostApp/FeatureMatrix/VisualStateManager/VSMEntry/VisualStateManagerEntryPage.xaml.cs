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
		EntryState.Text = DemoEntry.IsEnabled
			? (DemoEntry.IsFocused ? "State: Focused" : "State: Unfocused")
			: "State: Disabled";
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