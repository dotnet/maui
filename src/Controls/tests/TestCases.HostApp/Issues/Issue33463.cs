namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33463, "[macOS]Picker items are not visible", PlatformAffected.macOS)]
public class Issue33463 : TestContentPage
{
    protected override void Init()
    {
        var picker = new Picker
        {
            AutomationId = "TestPicker",
            Title = "Select an item"
        };
        picker.Items.Add("Item 1");
        picker.Items.Add("Item 2");
        picker.Items.Add("Item 3");

        var entry = new Entry
        {
            AutomationId = "TestEntry",
            Placeholder = "Entry for TAB focus"
        };

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Spacing = 10,
            Children = { picker, entry }
        };
    }
}
