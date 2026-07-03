using Microsoft.Maui.Controls.PlatformConfiguration;
using  Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18200, "Flyout Page SetCollapseStyle doesn't have any change", PlatformAffected.UWP)]
public class Issue18200 : TestFlyoutPage
{
	Button _button;
	protected override void Init()
    {
		this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetCollapseStyle(CollapseStyle.Partial);

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
            Text = "Page1",
			AutomationId = "FlyoutItem",
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
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

        _button = new Button
        {
            Text = "Change Collapse Style",
            AutomationId = "CollapseStyleButton",
        };
        _button.Clicked += OnCollapseStyleValueChanged;

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

    void OnCollapseStyleValueChanged(object sender, EventArgs e)
    {
		var currentCollapseStyle = this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetCollapseStyle();
		var newCollapseStyle = currentCollapseStyle == CollapseStyle.Full 
			? CollapseStyle.Partial 
			: CollapseStyle.Full;
		
		this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetCollapseStyle(newCollapseStyle);
    }
}
