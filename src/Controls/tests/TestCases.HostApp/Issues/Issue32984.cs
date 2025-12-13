namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "32984", "Picker on resize not working on Windows", PlatformAffected.UWP)]
public class Issue32984 : ContentPage
{
	public Issue32984()
	{
		VerticalStackLayout layout = new VerticalStackLayout
		{
			WidthRequest = 600
		};

		Picker issue32984Picker = new Picker
		{
			AutomationId= "issue32984Picker",	
			Title = "Select an item",
			HorizontalOptions = LayoutOptions.Fill,
			ItemsSource = new List<string> { "Item1", "Item2", "Item3"}
		};

		Button button = new Button
		{
			AutomationId= "issue32984Button",
			Text = "Click me",
			HorizontalOptions = LayoutOptions.Fill
		};

		button.Clicked += (s, e) => { layout.WidthRequest = 200; };

		layout.Children.Add(issue32984Picker);
		layout.Children.Add(button);

		Content = layout;
	}
}