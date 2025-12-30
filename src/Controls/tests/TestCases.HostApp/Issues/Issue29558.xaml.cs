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
		switch (_action)
		{
			case 0:
				searchHandler.Unfocus();
				break;
			case 1:
				entry.Unfocus();
				break;
			case 2:
				editor.Unfocus();
				break;
			default:
				_action = 0;
				break;
		}

		_action = (_action + 1) % 3;
	}
}