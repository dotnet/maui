namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29693, "The default native on color is not displayed when the Switch on color is not explicitly set", PlatformAffected.UWP)]
public class Issue29693 : ContentPage
{
	public Issue29693()
	{
		var defaultOnSwitch = new Switch
		{
			IsToggled = true,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
		};

		var defaultOffSwitch = new Switch
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var switchControl1 = new Switch
		{
			IsToggled = true,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var switchControl2 = new Switch
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var button1 = new Button
		{
			Text = "Toggle Switch 3",
			AutomationId = "button1",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		var button2 = new Button
		{
			Text = "Toggle Switch 4",
			AutomationId = "button2",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
		};

		button1.Clicked += (sender, e) =>
		{
			switchControl1.IsToggled = !switchControl1.IsToggled;
		};

		button2.Clicked += (sender, e) =>
		{
			switchControl2.IsToggled = !switchControl2.IsToggled;
		};

		var verticalStackLayout = new VerticalStackLayout()
		{
			Spacing = 20,
			Padding = new Thickness(20),
		};
		verticalStackLayout.Add(defaultOnSwitch);
		verticalStackLayout.Add(defaultOffSwitch);
		verticalStackLayout.Add(switchControl1);
		verticalStackLayout.Add(switchControl2);
		verticalStackLayout.Add(button1);
		verticalStackLayout.Add(button2);

		Content = verticalStackLayout;
	}
}
