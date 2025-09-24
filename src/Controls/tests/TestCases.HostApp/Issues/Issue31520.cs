namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31520, "NavigatingFrom is triggered first when using PushAsync", PlatformAffected.Android)]
public class Issue31520 : NavigationPage
{
    public static int count = 0;
    public static Label statusLabel;
    public static Issue31520Page2 currentPage2;

    public Issue31520() : base(new Issue31520Page1())
    {

    }
}

public class Issue31520Page1 : ContentPage
{
    public Issue31520Page1()
    {
        SetupPageContent();

        Disappearing += (s, e) =>
        {
            Issue31520.count++;
        };

        NavigatingFrom += (s, e) =>
        {
            if (Issue31520.statusLabel is not null)
            {
                Issue31520.statusLabel.Text = (Issue31520.count == 0)
                    ? "NavigatingFrom triggered before Disappearing"
                    : "NavigatingFrom triggered after Disappearing";

                Issue31520.currentPage2?.UpdateNavigatingStatusLabel();
            }
        };
    }

    private void SetupPageContent()
    {
        Title = "Page 1";

        Issue31520.statusLabel = new Label
        {
            Text = "Ready",
            AutomationId = "statusLabel"
        };

        var button = new Button
        {
            Text = "Push",
            AutomationId = "PushButton"
        };
        button.Clicked += OnCounterClicked;

        Content = new VerticalStackLayout
        {
            Padding = new Thickness(30, 0),
            Spacing = 25,
            Children =
                {
                    Issue31520.statusLabel,
                    button
                }
        };
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Issue31520Page2());
    }
}

public class Issue31520Page2 : ContentPage
{
    private Label navigatingStatusLabel;

    public Issue31520Page2()
    {
        Issue31520.currentPage2 = this;

        Appearing += (s, e) =>
        {
            Issue31520.count++;
        };

        Title = "Page 2";

        var button = new Button
        {
            Text = "Pop",
            AutomationId = "ChildPageButton"
        };
        button.Clicked += OnButtonClicked;

        navigatingStatusLabel = new Label
        {
            Text = Issue31520.statusLabel?.Text ?? "Ready",
            AutomationId = "NavigatingStatusLabel"
        };

        Content = new StackLayout
        {
            Children =
                {
                    navigatingStatusLabel,
                    button
                }
        };
    }

    public void UpdateNavigatingStatusLabel()
    {
        if (navigatingStatusLabel is not null && Issue31520.statusLabel is not null)
            navigatingStatusLabel.Text = Issue31520.statusLabel.Text;
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}
