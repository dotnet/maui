namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33206, "Shell NavigationBar colors persist when navigating between pages", PlatformAffected.iOS | PlatformAffected.UWP)]
public class IssueShellColorTest : Shell
{
    public IssueShellColorTest()
    {
        var tabBar = new TabBar { Title = "Tab1" };
        var tab = new Tab { Title = "Tab1" };

        var page1Content = new ShellContent
        {
            Title = "Page1",
            ContentTemplate = new DataTemplate(() => new IssueShellColorPage1())
        };

        var page2Content = new ShellContent
        {
            Title = "Page2",
            ContentTemplate = new DataTemplate(() => new IssueShellColorPage2())
        };

        tab.Items.Add(page1Content);
        tab.Items.Add(page2Content);
        tabBar.Items.Add(tab);
        Items.Add(tabBar);
    }
}

public class IssueShellColorPage1 : ContentPage
{
    public IssueShellColorPage1()
    {
        var layout = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        var label = new Label
        {
            Text = "Page1",
            AutomationId = "Page1Label"
        };

        layout.Add(label);
        Content = layout;
    }
}

public class IssueShellColorPage2 : ContentPage
{
    public IssueShellColorPage2()
    {
        Shell.SetBackgroundColor(this, Colors.Yellow);
        Shell.SetTitleColor(this, Colors.Orange);

        var layout = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        var label = new Label
        {
            Text = "Page2",
            AutomationId = "Page2Label"
        };

        layout.Add(label);
        Content = layout;
    }
}
