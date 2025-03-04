namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23921, "If a tap closes a SwipeView, the tap should not reach the children", PlatformAffected.Android)]
public partial class Issue23921 : ContentPage
{
	public Issue23921()
	{
		InitializeComponent();
	}

	private void button_Clicked(object sender, EventArgs e)
	{
		var button = sender as Button;
		button.Text = "tapped";
	}
}