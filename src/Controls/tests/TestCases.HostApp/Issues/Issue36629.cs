namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36629, "[Windows] SearchHandler FontSize, FontFamily, VerticalTextAlignment, and FontAttributes are not applied", PlatformAffected.All)]
public class Issue36629 : TestShell
{
    protected override void Init()
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;

        var contentPage = new ContentPage { Title = "Home" };

        Shell.SetSearchHandler(contentPage, new SearchHandler
        {
            AutomationId = "SearchHandler36629",
            Placeholder = "Search here",
            FontSize = 14,
            FontFamily = "Dokdo",
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Start,
        });

        contentPage.Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "SearchHandler font properties test",
                    AutomationId = "ContentLabel36629",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                }
            }
        };

        Items.Add(new ShellContent
        {
            Title = "Home",
            Route = "MainPage",
            Content = contentPage
        });
    }
}
