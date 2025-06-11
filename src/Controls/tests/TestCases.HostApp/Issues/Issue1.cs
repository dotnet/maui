namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 1, "Page Busy indicator should be visible", PlatformAffected.UWP)]
public class Issue1 : ContentPage
{
	public Issue1()
	{
		var button = new Button
		{
			Text = "Set IsBusy to true",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "SetIsBusyButton"
		};
		button.Clicked += (sender, e) =>
		{
			IsBusy = !IsBusy;
			button.Text = "Set IsBusy to false";
		};
		var label = new Label
		{
			Text = "This page should show a busy indicator when IsBusy is set to true.",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "BusyIndicatorLabel"
		};
		Content = new StackLayout
		{
			Children =
			{
				label,
				button
			}
		};
	}
}