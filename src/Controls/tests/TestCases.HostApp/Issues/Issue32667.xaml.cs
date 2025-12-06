namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32667, "Navbar keeps reserving space after navigating to page with Shell.NavBarIsVisible=\"False\"", PlatformAffected.iOS)]
public partial class Issue32667 : Shell
{
	public Issue32667()
	{
		InitializeComponent();
		Routing.RegisterRoute("subpage", typeof(Issue32667SubPage));
	}
}
