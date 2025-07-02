namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26154, "Button sizing broken by .NET9 when using the Mac UI Idiom", PlatformAffected.macOS)]
public partial class Issue26154 : ContentPage
{
	public Issue26154()
	{
		InitializeComponent();
	}
}