namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24468, "FlyoutPage toolbar button not updating on orientation change on Android", 
    PlatformAffected.Android)]
public class Issue24468 : TestFlyoutPage
{
    private Label _eventLabel;
    private Label _countLabel;
    private int _callCount = 0;

    protected override void Init()
    {
        Title = "Issue 24468";

        _eventLabel = new Label 
        { 
            Text = "ShouldShowToolbarButton is not called",
            AutomationId = "EventLabel"
        };

        _countLabel = new Label 
        { 
            Text = "0",
            AutomationId = "CountLabel"
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
                    _eventLabel,
                    _countLabel
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
            _eventLabel.Text = "ShouldShowToolbarButton called";
            _countLabel.Text = _callCount.ToString();
        }
        
        return shouldShow;
    }
}
