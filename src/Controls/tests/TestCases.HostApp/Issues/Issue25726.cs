namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 25726, "NullReferenceException in WillMoveToParentViewController When Removing Page During Navigation on iOS", PlatformAffected.iOS)]
public partial class Issue25726 : NavigationPage
{
    public Issue25726()
    {
        var page1 = new ContentPage
        {
            Title = "Page 1",
            Content = new Button
            {
                Text = "Navigate to Page 2",
                AutomationId = "NavigateToPage2Button",
                Command = new Command(async () =>
                {
                    var page2 = CreatePage2();
                    await PushAsync(page2);
                })
            }
        };

        PushAsync(page1);
    }

    private ContentPage CreatePage2()
    {
        var page2 = new ContentPage
        {
            Title = "Page 2",
            Content = new Button
            {
                Text = "Navigate to Page 3",
                AutomationId = "NavigateToPage3Button",
                Command = new Command(() =>
                {
                    var page3 = CreatePage3();
                    PushAsync(page3); 
                    
                    // Manipulate the stack immediately after starting navigation
                    Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                })
            }
        };
        return page2;
    }

    private ContentPage CreatePage3()
    {
        return new ContentPage
        {
            Title = "Page 3",
            Content = new Label { Text = "This is Page 3", AutomationId = "Page3Label" }
        };
    }
}