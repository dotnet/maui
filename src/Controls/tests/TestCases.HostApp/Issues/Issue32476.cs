namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32476, "Binding RTL FlowDirection in Shell causes Flyout MenuIcon and native window controls to overlap", PlatformAffected.UWP)]
public class Issue32476 : Shell
{
    public Issue32476()
    {
        FlowDirection = FlowDirection.RightToLeft;
        Title = "Shell RTL Test";
        FlyoutBehavior = FlyoutBehavior.Flyout;

        var contentPage = new ContentPage
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 10,
                Children =
                    {
                        new Label
                        {
                            Text = "The Shell title and window controls must be positioned correctly with RTL FlowDirection without overlapping.",
                            AutomationId = "TestLabel",
                            LineBreakMode = LineBreakMode.WordWrap
                        }
                    }
            }
        };

        Items.Add(new FlyoutItem
        {
            Title = "Home",
            Items =
                {
                    new ShellContent
                    {
                        Title = "Main",
                        Content = contentPage
                    }
                }
        });
    }
}