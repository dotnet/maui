namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29558, "Input controls should not change keyboard visibility", PlatformAffected.Android)]
public partial class Issue29558 : Shell
{
	int _action = 0;

	public Issue29558()
	{
		InitializeComponent();
	}

	private void UnfocusButton_Clicked(object sender, EventArgs e)
	{
		if (_action == 0)
		{
			searchHandler.Unfocus();
		}
		else if (_action == 1)
		{
			entry.Unfocus();
		}
		else if (_action == 2)
		{
			editor.Unfocus();
		}
		_action++;
	}
}