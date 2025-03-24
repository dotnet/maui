namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26792, "HideSoftInputOnTapped Not Working", PlatformAffected.Android)]
public class Issue26792 : ContentPage
{

	VerticalStackLayout stackLayout;
	Button _button;

	Entry _entry;
	public Issue26792()
	{
		this.HideSoftInputOnTapped = true;
		stackLayout = new VerticalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "Issue26792StackLayout",
		};
		_button = new Button
		{
			Text = "Click Me",
			AutomationId = "Issue26792Button",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		_entry = new Entry
		{
			Placeholder = "Enter text",
			AutomationId = "Issue26792Entry",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		stackLayout.Children.Add(_button);
		stackLayout.Children.Add(_entry);
		Content = stackLayout;
	}
}

