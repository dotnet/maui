namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35063, "[Android] Material3 - TabbedPage bottom tabs overflowing the contents", PlatformAffected.Android)]
public class Issue35063 : TabbedPage
{
    public Issue35063()
    {
        // Set tabs to bottom placement on Android
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);

        Children.Add(new ContentPage
        {
            Title = "Tab 1",
            BackgroundColor = Colors.Green,
            Content = new VerticalStackLayout
            {
                Children = {
                    new Label { AutomationId = "Label35063_1", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
                    }
                }
            }
        });

        Children.Add(new ContentPage
        {
            Title = "Tab 2",
            BackgroundColor = Colors.Green,
            Content = new VerticalStackLayout
            {
                Children = {
                    new Label { AutomationId = "Label35063_2", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
                    }
                }
            }
        });
    }
}