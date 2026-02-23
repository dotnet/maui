namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29768, "Switch OffColor not displayed after minimizing and reopening the app", PlatformAffected.iOS)]
public class Issue29768 : ContentPage
{
	public Issue29768()
	{

		var defaultOffSwitch = new Switch
		{
			OffColor = Colors.Red,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var switchControl = new Switch
		{
			IsToggled = true,
			OffColor = Colors.Red,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var button = new Button
		{
			Text = "Toggle Switch 2",
			AutomationId = "toggleButton",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		button.Clicked += (sender, e) =>
		{
			switchControl.IsToggled = !switchControl.IsToggled;
		};

		var verticalStackLayout = new VerticalStackLayout()
		{
			Spacing = 20,
			Padding = new Thickness(20),
		};

		verticalStackLayout.Add(defaultOffSwitch);
		verticalStackLayout.Add(switchControl);
		verticalStackLayout.Add(button);

		Content = verticalStackLayout;
	}
}
