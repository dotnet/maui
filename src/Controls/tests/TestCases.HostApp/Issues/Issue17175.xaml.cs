namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17175, "VisualStateManager should be able to set Style property dynamically", PlatformAffected.All)]
public partial class Issue17175 : ContentPage
{
    public Issue17175()
    {
        InitializeComponent();
    }

    private void OnGoToDisabledClicked(object sender, System.EventArgs e)
    {
        TestButton.IsEnabled = !TestButton.IsEnabled;
        StateLabel.Text = TestButton.Text;
    }
}