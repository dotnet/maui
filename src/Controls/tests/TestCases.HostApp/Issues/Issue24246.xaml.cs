namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 24246, "SafeArea arrange insets are currently insetting based on an incorrect Bounds", PlatformAffected.iOS)]
public partial class Issue24246 : ContentPage
{
	public Issue24246()
	{
		InitializeComponent();
	}
}