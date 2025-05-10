using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17105, "Hide password hint which is showing when the entry is focused", PlatformAffected.macOS)]
public class Issue17105 : ContentPage
{
	public Issue17105()
	{
		var stackLayout = new StackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25
		};

		var entry = new UITestEntry
		{
			HorizontalOptions = LayoutOptions.Fill,
			Placeholder = "Password: testing_password",
			IsCursorVisible = false,
			IsPassword = true,
			AutomationId = "Entry"
		};

		stackLayout.Children.Add(entry);
		Content = stackLayout;
	}

}
