using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 99999, "[Windows] FlyoutPage CollapsedPaneWidth Not Working", PlatformAffected.UWP)]
public class Issue99999 : TestFlyoutPage
{
	Microsoft.Maui.Controls.Label _button;
	protected override void Init()
	{
		this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetCollapseStyle(CollapseStyle.Partial);
		this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().CollapsedPaneWidth(100);

		// Set the flyout page properties
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

		// Create the flyout content
		var flyoutPage = new ContentPage
		{
			Title = "Master",
			BackgroundColor = Colors.Blue
		};

		var page1Button = new Button
		{
			Text = "Change",
			AutomationId = "FlyoutItem",
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center
		};
		page1Button.Clicked += (s, e) =>
		{
			this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().CollapsedPaneWidth(300);
		};

		flyoutPage.Content = new VerticalStackLayout
		{
			Children = { page1Button }
		};

		// Create the detail content
		var detailPage = new ContentPage
		{
			Title = "Detail",
			BackgroundColor = Colors.LightYellow
		};

		_button = new Microsoft.Maui.Controls.Label
		{
			Text = "Test for CollapsedPaneWidth",
			AutomationId = "CollapsedPaneLabel",
		};

		detailPage.Content = new VerticalStackLayout
		{
			Children = {
				new Microsoft.Maui.Controls.Label
				{
					Text = "Welcome to .NET MAUI!",
					TextColor = Colors.Black,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				},
				_button
			}
		};

		// Set the flyout and detail pages
		Flyout = flyoutPage;
		Detail = detailPage;
	}
}
