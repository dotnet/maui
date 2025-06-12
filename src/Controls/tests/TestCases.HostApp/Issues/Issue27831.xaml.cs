namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27831, "Adding GestureRecognizer To Editor prevents focus on Android & iOS", PlatformAffected.iOS|PlatformAffected.Android)]
public partial class Issue27831 : ContentPage
{
	public Issue27831()
	{
		InitializeComponent();
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		label.Text = "Tapped";
	}
}