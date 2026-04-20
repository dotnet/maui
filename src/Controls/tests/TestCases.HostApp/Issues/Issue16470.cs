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
            Title = "New Tab 1",
            Content = new Label { Text = "Content 1", AutomationId = "Tab1Content" }
        });
        Children.Add(new ContentPage
        {
            Title = "New Tab 2",
            Content = new Label { Text = "Content 2", AutomationId = "Tab2Content" }
        });
        Children.Add(new ContentPage
        {
            Title = "New Tab 3",
            Content = new Label { Text = "Content 3", AutomationId = "Tab3Content" }
        });
        Children.Add(new ContentPage
        {
            Title = "New Tab 4",
            Content = new Label { Text = "Content 4", AutomationId = "Tab4Content" }
        });
        Children.Add(new ContentPage
        {
            Title = "New Tab 5",
            Content = new Label { Text = "Content 5", AutomationId = "Tab5Content" }
        });
        Children.Add(new ContentPage
        {
            Title = "New Tab 6",
            Content = new Label { Text = "Content 6", AutomationId = "Tab6Content" }
        });
        Children.Add(new ContentPage
        {
            Title = "New Tab 7",
            Content = new Label { Text = "Content 7", AutomationId = "Tab7Content" }
        });
    }
}
