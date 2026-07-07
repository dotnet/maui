namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32867, "Shell Flyout Icon is always black", PlatformAffected.iOS)]
public class Issue32867 : Shell
{
    public Issue32867()
    {
        // Create a simple ShellContent with a content page
        var shellContent = new ShellContent
        {
            Title = "Home",

            Content = new Issue32867Page()
        };
        FlyoutBehavior = FlyoutBehavior.Flyout;
        FlyoutIcon = "shopping_cart.png";
        Items.Add(shellContent);

        // Set Shell ForegroundColor (affects FlyoutIcon tint)
        Shell.SetForegroundColor(this, Colors.Red);
    }
}
public class Issue32867Page : ContentPage
{
    public Issue32867Page()
    {
        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            Children =
            {
                new Label
                {
                    Text = "The test passes if the Flyout icon appears in red (not black).",
                    AutomationId = "Issue32867Label"
                },

            }
        };
    }
}