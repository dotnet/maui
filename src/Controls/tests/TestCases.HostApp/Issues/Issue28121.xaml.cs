namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28121, "Picker Focused/Unfocused Events Do Not Fire", PlatformAffected.Android)]

public partial class Issue28121 : ContentPage
{
	public Issue28121()
	{
		InitializeComponent();
	}

	void Picker_Focused(object sender, FocusEventArgs e)
	{
		FocusedLabel.Text = "Focused: true";
	}

	void Picker_Unfocused(object sender, FocusEventArgs e)
	{
		UnfocusedLabel.Text = "Unfocused: true";
	}
}
