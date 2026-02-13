namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31799, "WinUI TabbedPage can have multiple tabs selected", PlatformAffected.UWP)]
public class Issue31799 : FlyoutPage
{
    public Issue31799()
    {
        FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

        Flyout = CreateFlyoutMenu();
        
        TabbedPage mainTabbedPage = new TabbedPage
        {
            Title = "Project",
            SelectedTabColor = Colors.Blue
        };

        mainTabbedPage.Children.Add(CreateSelectedTab(isMain: true));
        mainTabbedPage.Children.Add(CreateUnselectedTab(isMain: true));
        
        Detail = new NavigationPage(mainTabbedPage);
    }

    ContentPage CreateFlyoutMenu() => new()
    {
        Title = "Menu",
        Content = new VerticalStackLayout
        {
            Padding = 20,
            Children = { CreateLabel("Flyout Menu") }
        }
    };

    ContentPage CreateSelectedTab(bool isMain)
    {
        string labelText = isMain
            ? "This is the Selected tab"
            : "Test PASSES if only one tab is being selected, else FAILS";

        return new ContentPage
        {
            Title = "Selected Tab",
            Content = new VerticalStackLayout 
            { 
                Padding = 20,
                Children = { CreateLabel(labelText, isMain ? "SelectedTabLabel" : "NewSelectedTabLabel") }
            }
        };
    }

    ContentPage CreateUnselectedTab(bool isMain)
    {
        string labelText = isMain 
            ? "This is the Unselected tab" 
            : "This is the Unselected tab of the new TabbedPage";

        return new ContentPage
        {
            Title = "Unselected Tab",
            Content = new VerticalStackLayout
            { 
                Spacing = 20,
                Padding = 20,
                Children = 
                {
                    CreateLabel(labelText),
                    CreateButton("Create New TabbedPage", "CreateTabbedPageButton", OnCreateTabbedPageClicked)
                }
            }
        };
    }

    Label CreateLabel(string text, string automationId = null) => new()
    {
        Text = text,
        AutomationId = automationId,
        VerticalOptions = LayoutOptions.Center,
        HorizontalOptions = LayoutOptions.Center
    };

    Button CreateButton(string text, string automationId, EventHandler clickHandler)
    {
        Button button = new Button
        {
            Text = text,
            AutomationId = automationId,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        button.Clicked += clickHandler;
        return button;
    }

    async void OnCreateTabbedPageClicked(object sender, EventArgs e)
    {
        TabbedPage newTabbedPage = new TabbedPage
        {
            Title = "New Project",
            SelectedTabColor = Colors.Blue
        };

        newTabbedPage.Children.Add(CreateSelectedTab(isMain: false));
        newTabbedPage.Children.Add(CreateUnselectedTab(isMain: false));

        if (Detail is NavigationPage navPage)
        {
            await navPage.PushAsync(newTabbedPage);
        }
    }
}