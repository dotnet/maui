namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29277, "Shell SearchHandler.Unfocus() has no effect on iOS & Android", PlatformAffected.iOS | PlatformAffected.Android)]
public partial class Issue29277 : Shell
{
	public Issue29277()
	{
		InitializeComponent();
	}

	private void Focus_Clicked(object sender, EventArgs e)
	{
		searchHandler.Focus();	
	}

	private void Unfocus_Clicked(object sender, EventArgs e)
	{
		searchHandler.Unfocus();
	}

	private void searchHandler_Focused(object sender, EventArgs e)
	{
		label.Text="Focused";
	}

	private void searchHandler_Unfocused(object sender, EventArgs e)
	{
		label.Text = "Unfocused";
	}
}