namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33008, "SearchBar and SearchHandler ShowsCancelButton property", PlatformAffected.iOS)]
public class Issue33008Shell : Shell
{
	public Issue33008Shell()
	{
		Items.Add(new Issue33008());
	}
}

public partial class Issue33008 : ContentPage
{
	public Issue33008()
	{
		InitializeComponent();
	}

	private void OnSetText(object sender, EventArgs e)
	{
		SearchBarDefault.Text = "Test text";
		SearchBarTrue.Text = "Test text";
		SearchBarFalse.Text = "Test text";
		StatusLabel.Text = "Text set on all SearchBars";
		SearchHandler.Query = "Test text";
	}
}
