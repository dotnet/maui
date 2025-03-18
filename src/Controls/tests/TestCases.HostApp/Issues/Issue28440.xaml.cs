namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28440, "FlyoutPage IsPresented not updated properly in windows", PlatformAffected.UWP)]
public partial class Issue28440 : FlyoutPage
{
	public Issue28440()
	{
		InitializeComponent();
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		flyoutPage.IsPresented = true;
	}
}
