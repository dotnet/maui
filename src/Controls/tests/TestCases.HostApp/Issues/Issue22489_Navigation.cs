using System;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24489_2, "TitleBar with NavigationBar", PlatformAffected.macOS)]

public class Issue22489_Navigation : NavigationPage
{
	public Issue22489_Navigation()
	{
		Navigation.PushAsync(new Issue24489_2());
	}
}
