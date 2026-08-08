using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33785, "[Windows] FlyoutPage CollapsedPaneWidth Not Working", PlatformAffected.UWP)]
public class Issue33785 : TestFlyoutPage
{
	Microsoft.Maui.Controls.Label _label;
	protected override void Init()
	{
		this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetCollapseStyle(CollapseStyle.Partial);
		this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().CollapsedPaneWidth(50);

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
			this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().CollapsedPaneWidth(100);
			_label.Text = "CollapsedPaneWidth set to 100";
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

		_label = new Microsoft.Maui.Controls.Label
		{
			Text = "Test for CollapsedPaneWidth",
			AutomationId = "CollapsedPaneLabel",
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center,
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
				_label
			}
		};

		// Set the flyout and detail pages
		Flyout = flyoutPage;
		Detail = detailPage;
	}
}
