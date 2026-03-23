namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25134, "Shell retains page references when replacing a page", PlatformAffected.Android)]
public class Issue25134 : Shell
{
    public Issue25134()
    {
        var mainContent = new ShellContent
        {
            ContentTemplate = new DataTemplate(() => new Issue25134InitialPage()),
            Title = "Initial",
            Route = "initial"
        };

        Items.Add(mainContent);
        Routing.RegisterRoute("Issue25134_child", typeof(Issue25134ChildPage));
        Routing.RegisterRoute("Issue25134_replace", typeof(Issue25134ReplacePage));
    }
}

public class Issue25134InitialPage : ContentPage
{
    public WeakReference<Page> ChildPageReference { get; set; }

    public Issue25134InitialPage()
    {
        Title = "Initial Page";

        var goToChildButton = new Button
        {
            Text = "Go to child page",
            AutomationId = "goToChildPage",
            VerticalOptions = LayoutOptions.Start,
            Command = new Command(() => Shell.Current.GoToAsync("Issue25134_child"))
        };

        Content = new VerticalStackLayout
        {
            goToChildButton
        };
    }
}

public class Issue25134ChildPage : ContentPage
{
    public Issue25134ChildPage()
    {
        Title = "Child Page";

        var button = new Button
        {
            Text = "Replace",
            AutomationId = "replace",
            VerticalOptions = LayoutOptions.Start,
            Command = new Command(() => Shell.Current.GoToAsync("../Issue25134_replace"))
        };

        Content = new VerticalStackLayout
        {
            button
        };
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent is ShellSection section)
        {
            var initialPage = (section.CurrentItem as IShellContentController).Page as Issue25134InitialPage;
            initialPage!.ChildPageReference = new WeakReference<Page>(this);
        }
    }
}

public class Issue25134ReplacePage : ContentPage
{
    public Issue25134ReplacePage()
    {
        Title = "Replace Page";

        Button checkRefButton = null;
        checkRefButton = new Button
        {
            Text = "Check reference",
            AutomationId = "checkReference",
            VerticalOptions = LayoutOptions.Start,
            Command = new Command(async () =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Yield();

                var section = (ShellSection)Parent;
                var initialPage = (Issue25134InitialPage)(section.CurrentItem as IShellContentController).Page;
                checkRefButton.Text = initialPage.ChildPageReference.TryGetTarget(out var page) ? "alive" : "gone";
            })
        };

        var backButton = new Button
        {
            Text = "Go back",
            AutomationId = "goBack",
            VerticalOptions = LayoutOptions.Start,
            Command = new Command(() => Shell.Current.GoToAsync(".."))
        };

        Content = new VerticalStackLayout
        {
            checkRefButton,
            backButton
        };
    }
}