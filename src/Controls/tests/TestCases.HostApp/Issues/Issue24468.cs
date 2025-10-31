namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24468, "FlyoutPage toolbar button not updating on orientation change on Android", 
    PlatformAffected.Android)]
public class Issue24468 : TestFlyoutPage
{
    private Label _statusLabel;
    private int _callCount = 0;

    protected override void Init()
    {
        Title = "Issue 24468";

        _statusLabel = new Label 
        { 
            Text = "ShouldShowToolbarButton is not called",
            AutomationId = "StatusLabel"
        };

        Flyout = new ContentPage
        {
            Title = "Menu",
            Content = new Label { Text = "Flyout Menu" }
        };

        Detail = new NavigationPage(new ContentPage
        {
            Title = "Detail",
            Content = new StackLayout
            {
                Children =
                {
                    new Label { Text = "Rotate device to test toolbar button updates" },
                    _statusLabel
                }
            }, 
            AutomationId = "ContentPage"
        });

        FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnLandscape;
    }

    public override bool ShouldShowToolbarButton()
    {
        _callCount++;
        var shouldShow = base.ShouldShowToolbarButton();
        
        if (_callCount > 1)
        {
            _statusLabel.Text = "ShouldShowToolbarButton is called";
        }
        
        return shouldShow;
    }
}
