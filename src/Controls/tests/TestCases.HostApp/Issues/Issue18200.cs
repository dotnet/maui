using Microsoft.Maui.Controls.PlatformConfiguration;
using  Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18200, "Flyout Page SetCollapseStyle doesn't have any change", PlatformAffected.UWP)]
public class Issue18200 : TestFlyoutPage
{
	Button _button;
	protected override void Init()
    {
		// Set the platform-specific collapse style (equivalent to your NewPage1.xaml.cs)
		this.On<Windows>().SetCollapseStyle(CollapseStyle.Partial);

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
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };

        var page2Button = new Button
        {
            Text = "Page2",
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };

        flyoutPage.Content = new VerticalStackLayout
        {
            Children = { page1Button, page2Button }
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
            AutomationId = "ToggleFlyoutLayoutBehavior"
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

    private void OnCollapseStyleValueChanged(object sender, EventArgs e)
    {
		var currentCollapseStyle = this.On<Windows>().GetCollapseStyle();
		var newCollapseStyle = currentCollapseStyle == CollapseStyle.Full 
			? CollapseStyle.Partial 
			: CollapseStyle.Full;
		
		this.On<Windows>().SetCollapseStyle(newCollapseStyle);
    }
}
