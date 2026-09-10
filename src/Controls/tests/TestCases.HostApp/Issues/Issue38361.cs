namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38361, "A singleton modal page with a manual disconnect policy renders blank when presented a second time on iOS", PlatformAffected.iOS)]
public class Issue38361 : TestShell
{
    const string ModalRoute = "Issue38361Modal";

    protected override void Init()
    {
        Routing.RegisterRoute(ModalRoute, typeof(Issue38361ModalPage));

        var mainPage = new ContentPage
        {
            Title = "Main",
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Children =
                {
                    new Button
                    {
                        Text = "Show modal",
                        AutomationId = "Issue38361ShowModalButton",
                        Command = new Command(async () =>
                            await Shell.Current.GoToAsync(ModalRoute))
                    }
                }
            }
        };

        AddContentPage(mainPage, "Issue38361Main");
    }
}

internal class Issue38361ModalPage : ContentPage
{
    public Issue38361ModalPage()
    {
        Title = "Modal";

        HandlerProperties.SetDisconnectPolicy(
            this,
            HandlerDisconnectPolicy.Manual);

        Shell.SetPresentationMode(
            this,
            PresentationMode.Modal);

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Children =
            {
                new Label
                {
                    Text = "Modal content",
                    AutomationId = "Issue38361ModalContent",
                    FontSize = 24
                },

                new Button
                {
                    Text = "Dismiss modal",
                    AutomationId = "Issue38361DismissModalButton",
                    Command = new Command(async () =>
                        await Shell.Current.GoToAsync(".."))
                }
            }
        };
    }
}

internal static class Issue38361Extensions
{
    public static MauiAppBuilder Issue38361RegisterServices(
        this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<Issue38361ModalPage>();

        return builder;
    }
}
