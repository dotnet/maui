namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 21328, "Link in Label with TextType HTML is not clickable", PlatformAffected.iOS | PlatformAffected.Android)]
public partial class Issues21328 : ContentPage
{
	public Issues21328()
	{
		InitializeComponent();
		label.Text = @"<a href=""https://www.example.com"">Example HTML link</a>";
		label2.Text = @"Normal Text and <a href=""https://www.example.com"">Example HTML link</a> here";
	}
	private void Button_Clicked(object sender, EventArgs e)
	{
		label2.Text = @"Visit dotnet from <a href=""https://dotnet.microsoft.com/en-us/""> here </a>";
	}
}