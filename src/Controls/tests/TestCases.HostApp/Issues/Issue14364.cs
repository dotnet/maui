namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14364, "Control size properties are not available during Loaded event", PlatformAffected.Android)]
public class Issue14364 : Shell
{
    public Issue14364()
    {
        ShellContent shellContent = new ShellContent
        {
            ContentTemplate = new DataTemplate(typeof(Issue14364Page)),
            Route = "Issue14364Page"
        };

        Items.Add(shellContent);
    }
}

public class Issue14364Page : ContentPage
{
    public Issue14364Page()
    {
        var grid = new Grid
        {
            HeightRequest = 300,
            WidthRequest = 200
        };

        var label = new Label
        {
            Text = "Size",
            AutomationId = "labelSize"
        };

        label.Loaded += Label_Loaded;

        grid.Children.Add(label);
        Content = grid;
    }

    void Label_Loaded(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var h1 = label.Height;
            var h2 = label.Width;

            label.Text = $"Height: {h1}, Width: {h2}";
        }
    }
}
