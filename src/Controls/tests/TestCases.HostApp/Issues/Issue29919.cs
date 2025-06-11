using System;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29919, "StackLayout Throws Exception on Windows When Orientation Is Set with HeightRequest of 0, Padding, and Opposing Alignment", PlatformAffected.UWP)]
public class Issue29919 : ContentPage
{
    public Issue29919()
    {
		var stack = new StackLayout();
		var label = new Label
		{
			Text = "HorizontalStackLayout with HeightRequest 0 should not crash.",
			AutomationId = "29919DescriptionLabel",
		};

        var horizontalStack = new HorizontalStackLayout
        {
            Padding = new Thickness(5),
            HeightRequest = 0,
            VerticalOptions = LayoutOptions.Center
        };

		stack.Children.Add(label);
		stack.Children.Add(horizontalStack);
		Content = stack;
    }
}