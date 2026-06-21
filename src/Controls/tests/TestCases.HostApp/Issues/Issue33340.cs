using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using TabbedPage = Microsoft.Maui.Controls.TabbedPage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33340, "NavigationBar overlaps StatusBar in TabbedPage", PlatformAffected.Android)]
public class Issue33340 : TabbedPage
{
    public Issue33340()
    {
        this.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
        var page1 = new ContentPage { Title = "Page1", Content = new Label { Text = "Page 1" } };
        NavigationPage.SetHasNavigationBar(page1, false);
        Children.Add(new NavigationPage(page1) { Title = "Tab1" });

        var page2 = new ContentPage { Title = "Page2", Content = new Label { Text = "Page 2" } };
        NavigationPage.SetHasNavigationBar(page2, false);
        Children.Add(new NavigationPage(page2) { Title = "Tab2" });

        var page3 = new ContentPage { Title = "Page3", Content = new Label { Text = "Page 3" } };
        NavigationPage.SetHasNavigationBar(page3, false);
        Children.Add(new NavigationPage(page3) { Title = "Tab3" });

        var page4 = new ContentPage { Title = "Page4", Content = new Label { Text = "Page 4 with NavigationBar" } };
        NavigationPage.SetHasNavigationBar(page4, true);
        Children.Add(new NavigationPage(page4) { Title = "Tab4" });

        CurrentPage = Children[0];
    }
}

