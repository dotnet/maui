using System;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24489_3, "TitleBar with NavigationBar - Shell", PlatformAffected.macOS)]

public partial class Issue24489_ShellNavigation : Shell
{
	public Issue24489_ShellNavigation()
	{
		InitializeComponent();
		Routing.RegisterRoute("Issue24489_2", typeof(Issue24489_2));
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Current.GoToAsync("Issue24489_2");
	}
}
