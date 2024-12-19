namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23563, "Unable to set toolbar overflow menu color if not using shell", PlatformAffected.Android)]
public class Issue23563NavPage : NavigationPage
{
	public Issue23563NavPage() : base(new Issue23563())
	{
		SetIconColor(this, Colors.Red);
	}
}

public partial class Issue23563 : ContentPage
{
	public Issue23563()
	{
		InitializeComponent();
	}
}