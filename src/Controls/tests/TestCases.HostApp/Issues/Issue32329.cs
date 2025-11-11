namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32329, "TabBar not visible on Mac Catalyst", PlatformAffected.macOS)]
public class Issue32329 : Shell
{
    public Issue32329()
    {
        Items.Add(CreateTabBar());
    }

    static TabBar CreateTabBar()
    {
        var tabBar = new TabBar();

        var homeTab = new Tab
        {
            Title = "Home",
        };
        homeTab.Items.Add(new ShellContent
        {
            Content = CreateContentPage("Home Page", "HomePageLabel"),
            Route = "HomePage"
        });

        var aboutTab = new Tab
        {
            Title = "About",
        };
        aboutTab.Items.Add(new ShellContent
        {
            Content = CreateContentPage("About Page", "AboutPageLabel"),
            Route = "AboutPage"
        });


        var settingsTab = new Tab
        {
            Title = "Settings",
        };
        settingsTab.Items.Add(new ShellContent
        {
            Content = CreateContentPage("Settings Page", "SettingsPageLabel"),
            Route = "SettingsPage"
        });

        tabBar.Items.Add(homeTab);
        tabBar.Items.Add(aboutTab);
        tabBar.Items.Add(settingsTab);

        return tabBar;
    }

    static ContentPage CreateContentPage(string text, string automationId)
    {
        return new ContentPage
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = text,
                        FontSize = 24,
                        HorizontalTextAlignment = TextAlignment.Center,
                        AutomationId = automationId
                    },
                    new Label
                    {
                        Text = "TabBar should be visible at the bottom with three tabs: Home, About, and Settings",
                        FontSize = 14,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    }
                }
            }
        };
    }
}