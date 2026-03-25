namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29919, "StackLayout Throws Exception on Windows When Orientation Is Set with HeightRequest of 0, Padding, and Opposing Alignment", PlatformAffected.UWP)]
public class Issue29919 : ContentPage
{
	public Issue29919()
	{
		var stack = new StackLayout();
		var label = new Label
		{
			Text = "VerticalStackLayout and HorizontalStackLayout should not crash when WidthRequest or HeightRequest is explicitly set to 0, respectively.",
			AutomationId = "29919DescriptionLabel",
		};

		var horizontalStack = new HorizontalStackLayout
		{
			Padding = new Thickness(5),
			HeightRequest = 0,
			VerticalOptions = LayoutOptions.Center
		};

		var verticalStack = new VerticalStackLayout
		{
			Padding = new Thickness(5),
			WidthRequest = 0,
			HeightRequest = 100,
			HorizontalOptions = LayoutOptions.Center
		};

		stack.Children.Add(label);
		stack.Children.Add(horizontalStack);
		stack.Children.Add(verticalStack);
		Content = stack;
	}
}