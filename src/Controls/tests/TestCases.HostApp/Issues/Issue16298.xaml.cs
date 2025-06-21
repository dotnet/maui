namespace Maui.Controls.Sample.Issues;
[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, "16298", "keyboard should dismiss on unfocused event", PlatformAffected.Android)]
public partial class Issue16298 : Shell
{
    public Issue16298()
    {
        InitializeComponent();
    }

    private void FocusBtnClicked(object sender, EventArgs e)
    {
        searchHandler.Focus();
    }

    private void UnFocusBtnClicked(object sender, EventArgs e)
    {
        searchHandler.Unfocus();
    }

}