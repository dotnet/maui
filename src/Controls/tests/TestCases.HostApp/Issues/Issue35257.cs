namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35257, "iOS 26 Switch initial Off/On/Thumb colors are incorrect until toggled", PlatformAffected.iOS)]
public class Issue35257 : ContentPage
{
	public Issue35257()
	{
		Switch initialOffSwitch = new Switch
		{
			AutomationId = "InitialOffSwitch",
			IsToggled = false,
			OnColor = Colors.Green,
			OffColor = Colors.Violet,
			ThumbColor = Colors.Orange,
			HorizontalOptions = LayoutOptions.Center,
		};

		Switch initialOnSwitch = new Switch
		{
			AutomationId = "InitialOnSwitch",
			IsToggled = true,
			OnColor = Colors.Green,
			OffColor = Colors.Violet,
			ThumbColor = Colors.Orange,
			HorizontalOptions = LayoutOptions.Center,
		};

		Label descriptionLabel = new Label
		{
			AutomationId = "DescriptionLabel",
			Text = "Test passes when OnColor, OffColor, and ThumbColor are correctly applied when toggled on and off.",
			HorizontalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Center,
		};

		Label firstSwitchStateLabel = new Label
		{
			AutomationId = "FirstSwitchStateLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		Label secondSwitchStateLabel = new Label
		{
			AutomationId = "SecondSwitchStateLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		void UpdateStateLabels()
		{
			firstSwitchStateLabel.Text = $"First switch state: {(initialOffSwitch.IsToggled ? "On" : "Off")}";
			secondSwitchStateLabel.Text = $"Second switch state: {(initialOnSwitch.IsToggled ? "On" : "Off")}";
		}

		initialOffSwitch.Toggled += (_, _) => UpdateStateLabels();
		initialOnSwitch.Toggled += (_, _) => UpdateStateLabels();

		Button cycleButton = new Button
		{
			AutomationId = "CycleSwitchStatesButton",
			Text = "Toggle Both Switches",
			HorizontalOptions = LayoutOptions.Center,
		};

		cycleButton.Clicked += (_, _) =>
		{
			initialOffSwitch.IsToggled = !initialOffSwitch.IsToggled;
			initialOnSwitch.IsToggled = !initialOnSwitch.IsToggled;
		};

		UpdateStateLabels();

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				descriptionLabel,
				initialOffSwitch,
				firstSwitchStateLabel,
				initialOnSwitch,
				secondSwitchStateLabel,
				cycleButton,
			}
		};
	}
}
