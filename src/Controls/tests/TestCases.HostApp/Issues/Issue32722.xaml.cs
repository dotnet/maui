namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32722, "NavigationPage.TitleView does not expand with host window in iPadOS 26+", PlatformAffected.iOS)]
public class Issue32722NavPage : NavigationPage
{
	public Issue32722NavPage() : base(new Issue32722())
	{
	}
}

public partial class Issue32722 : ContentPage
{
	public Issue32722()
	{
		InitializeComponent();
	}
}
