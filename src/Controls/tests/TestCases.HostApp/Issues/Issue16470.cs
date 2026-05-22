using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16470, "TabbedPage tab titles are truncated instead of scrolling on Android", PlatformAffected.Android)]
public class Issue16470 : TabbedPage
{
    public Issue16470()
    {
        Title = "Tab Scroll Issue";

        Children.Add(new ContentPage
        {
            Title = "Home Dashboard",
            Content = new Label { Text = "This is the Home Dashboard page", AutomationId = "Tab1Content", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        Children.Add(new ContentPage
        {
            Title = "Settings",
            Content = new Label { Text = "This is the Settings page", AutomationId = "Tab2Content", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        Children.Add(new ContentPage
        {
            Title = "Profile",
            Content = new Label { Text = "This is the Profile page", AutomationId = "Tab3Content", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        Children.Add(new ContentPage
        {
            Title = "Notifications",
            Content = new Label { Text = "This is the Notifications page", AutomationId = "Tab4Content", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        Children.Add(new ContentPage
        {
            Title = "Messages",
            Content = new Label { Text = "This is the Messages page", AutomationId = "Tab5Content", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        Children.Add(new ContentPage
        {
            Title = "Login",
            Content = new Label { Text = "This is the Login page", AutomationId = "Tab6Content", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
        Children.Add(new ContentPage
        {
            Title = "Help",
            Content = new Label { Text = "This is the Help page", AutomationId = "Tab7Content", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        });
    }
}
