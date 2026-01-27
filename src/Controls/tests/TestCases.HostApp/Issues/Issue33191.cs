namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33191, "NavBar visibility does not update when switching tabs", PlatformAffected.iOS)]
public class Issue33191 : Shell
{
    public Issue33191()
    {
        var tabBar = new TabBar { Title = "First Tab" };
        var tab = new Tab { Title = "First Tab" };

        var page1Content = new ShellContent
        {
            Title = "Page",
            ContentTemplate = new DataTemplate(() => new Issue33191Page1())
        };

        var page2Content = new ShellContent
        {
            Title = "Page 1",
            ContentTemplate = new DataTemplate(() => new Issue33191Page2())
        };

        tab.Items.Add(page1Content);
        tab.Items.Add(page2Content);
        tabBar.Items.Add(tab);
        Items.Add(tabBar);
    }
}

public class Issue33191Page1 : ContentPage
{
    public Issue33191Page1()
    {
        Title = "Page";

        var layout = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        var label = new Label
        {
            Text = "Page",
            AutomationId = "Page1Label"
        };

        layout.Add(label);
        Content = layout;
    }
}

public class Issue33191Page2 : ContentPage
{
    public Issue33191Page2()
    {
        Title = "Page 1";
        Shell.SetNavBarIsVisible(this, false);

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
            AutomationId = "Page2Label"
        };

        layout.Add(label);
        Content = layout;
    }
}
