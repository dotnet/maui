namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30065, "DatePicker Ignores FlowDirection When Set to RightToLeft or MatchParent", PlatformAffected.iOS)]
public class Issue30065 : ContentPage
{
	public Issue30065()
	{
		DatePicker rtlDatePicker = new DatePicker
		{
			WidthRequest = 300,
			FlowDirection = FlowDirection.RightToLeft,
		};

		DatePicker ltrDatePicker = new DatePicker
		{
			WidthRequest = 300,
			FlowDirection = FlowDirection.LeftToRight,
		};

		Button toggleButton = new Button
		{
			AutomationId = "ToggleFlowDirectionBtn",
			Text = "Toggle FlowDirection",
		};

		toggleButton.Clicked += (s, e) =>
		{
			rtlDatePicker.FlowDirection = FlowDirection.LeftToRight;
			ltrDatePicker.FlowDirection = FlowDirection.RightToLeft;
		};

		VerticalStackLayout verticalStackLayout = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				rtlDatePicker,
				ltrDatePicker,
				toggleButton
			}
		};

		Content = verticalStackLayout;
	}
}