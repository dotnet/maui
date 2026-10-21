namespace Maui.Controls.Sample.Issues;

[Issue(
    IssueTracker.Github,
    38026,
    "Navigation stops working after changing Window.Page from Shell",
    PlatformAffected.Android)]
public class Issue38026 : ContentPage
{
    public Issue38026()
    {
        Content = new Button
        {
            AutomationId = "InstallNavigationPageButton",
            Text = "Install NavigationPage",
            Command = new Command(() =>
                Application.Current!.Windows[0].Page = new NavigationPage(new NavigationRootPage())),
        };
    }

    sealed class NavigationRootPage : ContentPage
    {
        public NavigationRootPage()
        {
            Content = new Button
            {
                AutomationId = "ShowShellButton",
                Text = "Show Shell",
                Command = new Command(() => Application.Current!.Windows[0].Page = new TestShell()),
            };
        }
    }

    sealed class TestShell : Shell
    {
        public TestShell()
        {
            FlyoutBehavior = FlyoutBehavior.Flyout;

            var openFlyoutButton = new Button
            {
                AutomationId = "OpenShellFlyoutButton",
                Text = "Open Shell flyout",
            };
            openFlyoutButton.Clicked += (_, _) => FlyoutIsPresented = true;

            Items.Add(new ShellContent
            {
                Title = "Home",
                Content = new ContentPage { Content = openFlyoutButton },
            });

            FlyoutFooter = new Button
            {
                AutomationId = "ReplaceShellButton",
                Text = "Replace Shell",
                Command = new Command(() =>
                    Application.Current!.Windows[0].Page = new NavigationPage(new ReplacementPage())),
            };
        }
    }

    sealed class ReplacementPage : ContentPage
    {
        public ReplacementPage()
        {
            Content = new Label
            {
                Text = "Replacement NavigationPage",
                AutomationId = "ReplacementPageLabel",
            };
        }
    }
}