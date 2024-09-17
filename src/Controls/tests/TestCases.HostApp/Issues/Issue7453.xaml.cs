namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 7453, "ShellContent Title doesn't observe changes to bound properties", PlatformAffected.All)]
public partial class Issue7453 : Shell
{
	public Issue7453()
	{
		InitializeComponent();
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
		this.tab.Title = "Updated title";
    }
}