using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38765, "TabbedPage: Selected tab icon loses SelectedTabColor after rebuilding Children", PlatformAffected.Android)]
public class Issue38765 : TestTabbedPage
{
    private bool _populated;

    protected override void Init()
    {
        SelectedTabColor = Colors.Magenta;
        On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetToolbarPlacement(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_populated)
        {
            return;
        }

        _populated = true;
        PopulateTabs();
    }

    private void PopulateTabs()
    {
        Children.Clear();

        Children.Add(
        new ContentPage
        {
            Title = "Home",
            IconImageSource = new FontImageSource
            {
                Glyph = "●",
                Size = 24
            },
            Content = new VerticalStackLayout
            {
                Children =
            {
                new Label
                {
                    Text = "Home",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                },
                new Button
                {
                    AutomationId = "Issue38765RebuildTabs",
                    Text = "Rebuild tabs",
                    Command = new Command(PopulateTabs)
                }
            }
            }
        });

        Children.Add(
            new ContentPage
            {
                Title = "Search",
                IconImageSource = new FontImageSource
                {
                    Glyph = "■",
                    Size = 24
                },
                Content = new Label
                {
                    Text = "Search",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            });

        Children.Add(
            new ContentPage
            {
                Title = "Profile",
                IconImageSource = new FontImageSource
                {
                    Glyph = "▲",
                    Size = 24
                },
                Content = new Label
                {
                    Text = "Profile",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            });
    }
}
