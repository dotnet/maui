namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38323, "[iOS] Shell navigation operations fail under the native More tab", PlatformAffected.iOS)]
public class Issue38323 : TestShell
{
    protected override void Init()
    {
        FlyoutBehavior = FlyoutBehavior.Disabled;

        for (int i = 1; i <= 7; i++)
        {
            var title = i == 6 ? "More 1" : $"Tab {i}";
            var page = i == 6
                ? new Issue38323RootPage()
                : new ContentPage { Title = title };

            AddBottomTab(page, title);
        }
    }
}

class Issue38323RootPage : ContentPage
{
    public Issue38323RootPage()
    {
        Title = "More 1";

        var pushButton = new Button
        {
            AutomationId = "PushPageButton",
            Text = "Push test page",
        };
        pushButton.Clicked += async (_, _) => await Navigation.PushAsync(new Issue38323StackPage());

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    AutomationId = "RootPageLabel",
                    Text = "More 1 root page",
                },
                pushButton,
            },
        };
    }
}

class Issue38323StackPage : ContentPage
{
    readonly Label _statusLabel;

    public Issue38323StackPage()
    {
        Title = "Test Page";

        _statusLabel = new Label
        {
            AutomationId = "OperationStatusLabel",
            Text = "Ready",
        };

        var insertButton = new Button
        {
            AutomationId = "InsertPageButton",
            Text = "Insert page before this",
        };
        insertButton.Clicked += InsertPageBeforeThis;

        var removeButton = new Button
        {
            AutomationId = "RemovePageButton",
            Text = "Remove current page",
        };
        removeButton.Clicked += (_, _) => Navigation.RemovePage(this);

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                _statusLabel,
                insertButton,
                removeButton,
            },
        };
    }

    void InsertPageBeforeThis(object sender, EventArgs e)
    {
        try
        {
            Navigation.InsertPageBefore(new Issue38323InsertedPage(), this);

            _statusLabel.Text = "Insert succeeded";
        }
        catch (Exception exception)
        {
            _statusLabel.Text = $"Insert failed: {exception.GetType().Name}";
        }
    }
}

class Issue38323InsertedPage : ContentPage
{
    public Issue38323InsertedPage()
    {
        Title = "Inserted Page";

        var popToRootButton = new Button
        {
            AutomationId = "PopToRootButton",
            Text = "Pop to root",
        };
        popToRootButton.Clicked += async (_, _) => await Navigation.PopToRootAsync();

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    AutomationId = "InsertedPageLabel",
                    Text = "Inserted page",
                },
                popToRootButton,
            },
        };
    }
}
